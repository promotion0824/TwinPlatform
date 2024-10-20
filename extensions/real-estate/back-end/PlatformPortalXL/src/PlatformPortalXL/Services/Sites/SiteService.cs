using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Features.Scopes;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Batch;
using Willow.Common;
using Willow.Platform.Models;
using Willow.Workflow;
using Willow.Workflow.Models;

namespace PlatformPortalXL.Services.Sites
{
    public interface ISiteService
    {
        Task<List<SiteDetailDto>> GetSites(Guid userId, bool includeWeather = false, bool includeDtIds = false);

        Task<string> MapToTwinId(Guid siteId);

        Task Broadcast(Guid siteId, IEnumerable<string> urns);

        Task<Guid> GetCustomerId(Guid siteId);
        Task<Guid> GetCustomerId(IEnumerable<Guid> siteIds);

        Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, string scopeId = null, IEnumerable<Guid> siteIds = null, Expression < Func<Site, bool>> predicate = null);
        Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, List<string> scopeIds, Expression<Func<Site, bool>> predicate = null);
        Task<List<TwinDto>> GetUserSiteTwinsByScopeIdAsync(ScopeIdRequest request);

        Task CheckScopePermission(Guid userId, string permissionId, string scopeId);

        Task<BatchDto<SiteMiniDto>> GetSitesV2(Guid userId, BatchSitesRequest request);
    }

    public class SiteService : ISiteService
    {
        private readonly IDirectoryApiService _directoryApi;
        private readonly IInsightApiService _insightApiService;
        private readonly ISiteApiService _siteApi;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IWeatherService _weatherService;
        private readonly IPortfolioDashboardService _portfolioDashboard;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IGeometryViewerMessagingService _geometryViewerMessagingService;
        private readonly ILogger<SiteService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IWorkflowApiService _workflowApiService;
        private readonly ISiteIdToTwinIdMatchingService _siteIdToTwinIdMatch;


        public SiteService(IDirectoryApiService directoryApi,
            IInsightApiService insightApiService,
            ISiteApiService siteApi,
            IImageUrlHelper imageUrlHelper,
            ITimeZoneService timeZoneService,
            IWeatherService weatherService,
            IPortfolioDashboardService portfolioDashboard,
            IDigitalTwinApiService digitalTwinApiService,
            IGeometryViewerMessagingService geometryViewerMessagingService,
            ILogger<SiteService> logger,
            IMemoryCache cache,
            IWorkflowApiService workflowApiService,
            ISiteIdToTwinIdMatchingService siteIdToTwinIdMatchingService)
        {
            _directoryApi = directoryApi ?? throw new ArgumentNullException(nameof(directoryApi));
            _insightApiService = insightApiService ?? throw new ArgumentNullException(nameof(insightApiService));
            _siteApi = siteApi ?? throw new ArgumentNullException(nameof(siteApi));
            _imageUrlHelper = imageUrlHelper ?? throw new ArgumentNullException(nameof(imageUrlHelper));
            _timeZoneService = timeZoneService ?? throw new ArgumentNullException(nameof(timeZoneService));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _portfolioDashboard = portfolioDashboard ?? throw new ArgumentNullException(nameof(portfolioDashboard));
            _digitalTwinApiService =
                digitalTwinApiService ?? throw new ArgumentNullException(nameof(digitalTwinApiService));
            _geometryViewerMessagingService = geometryViewerMessagingService ??
                                              throw new ArgumentNullException(nameof(geometryViewerMessagingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache;
            _workflowApiService = workflowApiService ?? throw new ArgumentNullException(nameof(workflowApiService));
            _siteIdToTwinIdMatch = siteIdToTwinIdMatchingService;
        }

        public async Task<string> MapToTwinId(Guid siteId)
        {
            var twinSearchResponse = await _digitalTwinApiService.Search(new Features.Twins.TwinSearchRequest
            {
                ModelId = AdtConstants.SiteModelId,
                SiteIds = new[] { siteId }
            });

            return twinSearchResponse.Twins.FirstOrDefault(x => x.SiteId == siteId)?.Id;
        }

        public async Task<string> MapToTwinIdOrDefault(Guid siteId)
        {
            try
            {
                return await MapToTwinId(siteId);
            }
            catch
            {
                return siteId.ToString();
            }
        }

        public async Task Broadcast(Guid siteId, IEnumerable<string> urns)
        {
            if (urns?.Any() ?? false)
            {
                var twinId = await MapToTwinIdOrDefault(siteId);

                foreach (var urn in urns)
                {
                    await _geometryViewerMessagingService.Send(siteId, twinId, urn);
                }
            }
        }

        public async Task<List<SiteDetailDto>> GetSites(Guid userId, bool includeWeather = false, bool includeDtIds = false)
        {
            var userDetails = await _directoryApi.GetUserDetailsAsync(userId);
            if (!userDetails.UserAssignments.Any())
            {
                return new List<SiteDetailDto>();
            }
            var sites = await _siteApi.GetSitesByCustomerAsync(userDetails.CustomerId);

            var isConnectivityEnabled = userDetails.Customer.Features.IsConnectivityViewEnabled;
            //accessibleSites are sites that users have permission to access
            var accessibleSites = sites
                .Where(
                    s =>
                        userDetails.UserAssignments.Any(
                            a => a.resourceId == s.CustomerId
                              || a.resourceId == s.PortfolioId
                              || a.resourceId == s.Id
                        )
                )
                .ToList();

            var adminAssignments = userDetails.UserAssignments.Where(x => x.permissionId == Permissions.ManageFloors);
            var siteTasks =
                accessibleSites.Select(site => GetSite(
                    site,
                    () => {
                        return Task.FromResult(adminAssignments.Any(x => x.resourceId == site.CustomerId
                                                        || x.resourceId == site.PortfolioId
                                                        || x.resourceId == site.Id));
                    },
                    isConnectivityEnabled,
                    includeWeather,
                    includeDtIds));

            var result = await Task.WhenAll(siteTasks);

            await AppendInsightsAndTicketStats(result);

            return result.ToList();
        }

        internal async Task AppendInsightsAndTicketStats(IEnumerable<SiteDetailDto> siteDetailDtos)
        {
            // Get Tickets and Insights Status

            var siteIds = siteDetailDtos.Select(x => x.Id).ToList();

            var insightStatusTask = _insightApiService.GetInsightStatisticsBySiteIds(siteIds);
            var ticketStatusTask = _workflowApiService.GetTicketStatisticsBySiteIdsAsync(siteIds);

            var statusTasks = Task.WhenAll(insightStatusTask, ticketStatusTask);

            try
            {
                await statusTasks;
            }
            catch (Exception ex)
            {
                if (statusTasks.Exception is not null)
                {
                    _logger.LogError(statusTasks.Exception, "Get Site status task error");
                }
                else
                {
                    _logger.LogError(ex, "Get Site status error");
                }

            }
            foreach (var site in siteDetailDtos)
            {
                if (insightStatusTask.IsCompletedSuccessfully)
                {
                    var insightStatusResult = insightStatusTask.Result;
                    var statsByPriority = insightStatusResult.StatisticsByPriority.FirstOrDefault(x => x.Id == site.Id);
                    var statsByStatus = insightStatusResult?.StatisticsByStatus.FirstOrDefault(x => x.Id == site.Id); ;
                    site.InsightsStats = SiteInsightStatistics.MapTo(statsByPriority);
                    site.InsightsStatsByStatus = SiteInsightStatisticsByStatus.MapTo(statsByStatus);
                }
                if (ticketStatusTask.IsCompletedSuccessfully)
                {
                    var ticketStatusResult = ticketStatusTask.Result;
                    var statsByPriority = ticketStatusResult.StatisticsByPriority.FirstOrDefault(x => x.Id == site.Id);
                    var statsByStatus = ticketStatusResult?.StatisticsByStatus.FirstOrDefault(x => x.Id == site.Id);
                    site.TicketStats = TicketStatisticsByPriority.MapTo(statsByPriority);
                    site.TicketStatsByStatus = TicketStatisticsByStatus.MapTo(statsByStatus);
                }
            }
        }

        public async Task<BatchDto<SiteMiniDto>> GetSitesV2(Guid userId, BatchSitesRequest request)
        {
            var userSites= await _directoryApi.GetUserSitesPaged(userId, request);

            var result = new BatchDto<SiteMiniDto>
            {
                After = userSites.After,
                Before = userSites.Before,
                Items = SiteMiniDto.Map(userSites.Items, _imageUrlHelper).ToArray(),
                Total = userSites.Total
            };

            // Append extra fields like weather requested from F/E
            if (request.ProjectFields.Any())
            {
                AppendProjectFields(request, result);
            }
            return result;
        }

        public async Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, string scopeId = null, IEnumerable<Guid> siteIds = null, Expression<Func<Site, bool>> predicate = null)
        {
            var userSites = await _directoryApi.GetUserSites(userId, Permissions.ViewSites);

            return await GetAuthorizedSiteIdsInternal(userSites, userId, scopeId, siteIds, predicate);
        }

        internal async Task<List<Guid>> GetAuthorizedSiteIdsInternal(List<Site> userSites, Guid userId, string scopeId = null, IEnumerable<Guid> siteIds = null, Expression<Func<Site, bool>> predicate = null)
        {
            if (predicate != null)
            {
                userSites = userSites?.AsQueryable().Where(predicate)?.ToList();
            }

            var userSiteIds = userSites?.Select(c => c.Id).ToList();

            if (!string.IsNullOrEmpty(scopeId))
            {
                userSiteIds = await GetUserSiteIdsByScopeIdAsync(new ScopeIdRequest() { DtId = scopeId, UserSites = userSites });
            }
            else if (siteIds != null && siteIds.Any())
            {
                userSiteIds = userSiteIds?.Intersect(siteIds).ToList();
            }

            if (userSiteIds == null || userSiteIds.Count==0)
            {
                throw new UnauthorizedAccessException().WithData(new { userId, scopeId });
            }

            return userSiteIds;
        }

        public async Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, List<string> scopeIds, Expression<Func<Site, bool>> predicate = null)
        {
            var userSites = await _directoryApi.GetUserSites(userId, Permissions.ViewSites);

            if (predicate != null)
            {
                userSites = userSites?.AsQueryable().Where(predicate)?.ToList();
            }

            var userSiteIds= await GetUserSiteIdsByScopeIdsAsync(userSites,scopeIds);
            if (userSiteIds == null || userSiteIds.Count == 0)
            {
                throw new UnauthorizedAccessException().WithData(new { userId, scopeIds });
            }
            return userSiteIds;
        }
        public async Task<Guid> GetCustomerId(Guid siteId)
        {
            return await _cache.GetOrCreateAsync(
                $"CustomerIdForSite_{siteId}",
                async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1000);

                    return (await _siteApi.GetSite(siteId)).CustomerId;
                }
            );
        }

        public async Task<Guid> GetCustomerId(IEnumerable<Guid> siteIds)
        {
            return await GetCustomerId(siteIds?.FirstOrDefault() ?? Guid.Empty);
        }

        public async Task<List<TwinDto>> GetUserSiteTwinsByScopeIdAsync(ScopeIdRequest request)
        {
            var siteTwins = await _cache.GetOrCreateAsync(
                    request.CacheKey,
                    async (entry) =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                        return await _digitalTwinApiService.GetSitesByScopeAsync(new GetSitesByScopeIdRequest { Scope = request });
                    }
                );

            return siteTwins.Where(x => x.SiteId.HasValue && request.SiteIds.Contains(x.SiteId.Value)).Distinct().ToList();
        }

        /// <summary>
        /// Since we don't have implemented the scope permission check in the directorycore,
        /// here we check if the user accessible sites under the passed permission belong to the scope.
        /// </summary>
        /// <param name="userId">Current user Id</param>
        /// <param name="permissionId">Permission to check</param>
        /// <param name="scopeId">Scope Id</param>
        public async Task CheckScopePermission(Guid userId, string permissionId, string scopeId)
        {
            var userSites = await _directoryApi.GetUserSites(userId, permissionId);

            await CheckScopePermissionInternal(userSites, userId, scopeId);
        }

        internal async Task CheckScopePermissionInternal(List<Site> userSites, Guid userId, string scopeId)
        {
            var userSiteIds = userSites?.Select(c => c.Id).ToList();

            if (!string.IsNullOrEmpty(scopeId))
            {
                userSiteIds = (await GetUserSiteIdsByScopeIdAsync(new ScopeIdRequest() { DtId = scopeId, UserSites = userSites })).ToList();
            }

            if (userSiteIds == null || !userSiteIds.Any())
            {
                throw new UnauthorizedAccessException().WithData(new { userId, scopeId });
            }
        }

        private async Task<List<Guid>> GetUserSiteIdsByScopeIdAsync(ScopeIdRequest request)
        {
            return (await GetUserSiteTwinsByScopeIdAsync(request)).Where(x => x.SiteId.HasValue).Select(x => x.SiteId.Value).ToList();
        }

        private async Task<List<Guid>> GetUserSiteIdsByScopeIdsAsync(List<Site> userSites, List<string> scopeIds)
        {
            if (userSites == null || userSites.Count == 0)
                return null;
            if (scopeIds == null || scopeIds.Count == 0)
                return userSites.Select(x => x.Id).ToList();
            var scopeTasks = scopeIds.Select(x =>
                GetUserSiteTwinsByScopeIdAsync(new ScopeIdRequest { DtId = x, UserSites = userSites }));
            var scopesSites =await Task.WhenAll(scopeTasks);
            return scopesSites.SelectMany(x=>x).Where(x => x.SiteId.HasValue).Select(x => x.SiteId.Value).ToList();
        }

        internal async Task<SiteDetailDto> GetSite(
            Site siteDetail,
            Func<Task<bool>> isSiteAdmin,
            bool isConnectivityViewEnabled,
            bool includeWeather = false,
            bool includeDtIds = false
        )
        {
            var siteDto = SiteDetailDto.Map(siteDetail, _imageUrlHelper);

            siteDto.TimeZone = _timeZoneService.GetTimeZoneType(siteDto.TimeZoneId);
            siteDto.Features = SiteFeaturesDto.Map(siteDetail.Features);
            siteDto.ArcGisLayers = ArcGisLayerDto.Map(siteDetail.ArcGisLayers);

            siteDto = await GetSiteInfo(siteDetail, siteDto, isConnectivityViewEnabled);

            if (await isSiteAdmin())
            {
                siteDto.UserRole = "admin";
            }

            if (includeWeather && siteDto.Latitude != null && siteDto.Longitude != null)
            {
                try
                {
                    siteDto.Weather = _weatherService.GetWeather(siteDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while retrieving weather information from Weatherbit");
                }
            }

            if (includeDtIds)
            {
                siteDto.TwinId = await _siteIdToTwinIdMatch.FindMatchToMostSignificantSpatialTwin(siteDto.Id);
            }

            return siteDto;
        }

        private async Task<SiteDetailDto> GetSiteInfo(
            Site site,
            SiteDetailDto siteDto,
            bool isConnectivityViewEnabled
        )
        {
            try
            {
                if (isConnectivityViewEnabled)
                {
                    siteDto.IsOnline = (await _portfolioDashboard.GetSiteSimpleDashboardDataAsync(site))?.IsOnline;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to get the site info {Message} ", ex.Message);
            }
            return siteDto;
        }

        private void AppendProjectFields(BatchSitesRequest request, BatchDto<SiteMiniDto> result)
        {
            if (request.ProjectFields.Any(x => x.Name == "Weather"))
            {
                foreach (var item in result.Items)
                {
                    item.Weather = _weatherService.GetWeather(item.Id);
                }
            }
        }
    }
}
