using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Api.Client;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Workflow;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Services
{
    public interface IPortfolioDashboardService
    {
        Task<PortfolioDashboardSiteStatus> GetDashboardDataForSiteAsync(Guid siteId);
        Task<List<PortfolioDashboardSiteStatus>> GetDashboardDataForUserAsync(User user, List<Site> allowedSites);
        Task<List<PortfolioDashboardConnectorLog>> GetConnectivityHistoryForConnectorAsync(Guid connectorId);
        Task<PortfolioDashboardSiteStatus> GetSiteSimpleDashboardDataAsync(Site site);
    }

    public class PortfolioDashboardService : IPortfolioDashboardService
    {
        private readonly ISiteApiService _siteApi;
        private readonly IConnectorApiService _connectorApi;
        private readonly IMemoryCache _memoryCache;
        private readonly IDigitalTwinApiService _digitalTwin;
        private readonly ILogger<PortfolioDashboardService> _logger;
        private readonly IInsightApiService _insightApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IDirectoryApiService _directoryApi;

        private static readonly TimeSpan CachePeriod = TimeSpan.FromMinutes(5);

        public PortfolioDashboardService(
            ISiteApiService siteApi,
            IConnectorApiService connectorApi,
            IMemoryCache memoryCache,
            IDigitalTwinApiService digitalTwin,
            IInsightApiService insightApi,
            IWorkflowApiService workflowApi,
            IDirectoryApiService directoryApi,
            ILogger<PortfolioDashboardService> logger)
        {
            _siteApi = siteApi;
            _connectorApi = connectorApi;
            _memoryCache = memoryCache;
            _digitalTwin = digitalTwin;
            _insightApi = insightApi;
            _workflowApi = workflowApi;
            _directoryApi = directoryApi;
            _logger = logger;
        }

        public async Task<PortfolioDashboardSiteStatus> GetDashboardDataForSiteAsync(Guid siteId)
        {
            return await _memoryCache.GetOrCreateAsync($"SiteDashboardDataForSite_{siteId}", async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = CachePeriod;
                var site = await _siteApi.GetSite(siteId);
                return await GetDashboardDataForSiteAsync(site);
            });
        }

        public async Task<List<PortfolioDashboardSiteStatus>> GetDashboardDataForUserAsync(User user, List<Site> allowedSites)
        {
            return await _memoryCache.GetOrCreateAsync($"SiteDashboardDataForUser_{user.Id}", async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = CachePeriod;
                var sites = await _siteApi.GetSites(user.CustomerId, null);
                var allowedSiteIds = allowedSites.Select(x => x.Id).ToList();
                var allowedSitesInSiteCore = sites.Where(x => allowedSiteIds.Contains(x.Id)).ToList();

                foreach (var siteInSiteCore in allowedSitesInSiteCore)
                {
                    siteInSiteCore.Features = allowedSites.First(x => x.Id == siteInSiteCore.Id).Features;
                }

                var results = await GetDashboardDataForSitesAsync(allowedSitesInSiteCore);
                return results.ToList();
            });
        }

        public async Task<List<PortfolioDashboardConnectorLog>> GetConnectivityHistoryForConnectorAsync(Guid connectorId)
        {
            return await _memoryCache.GetOrCreateAsync($"ConnectivityHistoryForConnector_{connectorId}", async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = CachePeriod;
                var startTime = PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddDays(-1));

                var logRecords = await _connectorApi.GetLogsForConnectorAsync(connectorId, startTime, PortfolioDashboardConnectorLog.MapQuarterHour(DateTime.UtcNow.AddMinutes(15)));
                return PortfolioDashboardConnectorLog.MapFrom(startTime, logRecords);
            });
        }

        private async Task<PortfolioDashboardSiteStatus> GetDashboardDataForSiteAsync(Site site)
        {
            return await _memoryCache.GetOrCreateAsync($"GetDashboardDataForSite_{site.Id}", async (entry) => {
                entry.AbsoluteExpirationRelativeToNow = CachePeriod;
                try
                {
                    site.Features = await _directoryApi.GetSiteFeatures(site.Id);
                    var gateways = await GetGatewayAndConnectorStatusForSiteAsync(site.Id);
                    var siteList = new List<Site>{ site };

                    return new PortfolioDashboardSiteStatus
                    {
                        SiteId = site.Id,
                        Name = site.Name,
                        Country = site.Country,
                        State = site.State,
                        Suburb = site.Suburb,
                        Status = GetSiteStatus(site, gateways),
                        PointCount = await GetPointCountAsync(site.Id, gateways),
                        Insights = PortfolioDashboardSiteInsights.MapFrom(await GetSitesInsightsAsync(siteList))?.FirstOrDefault(),
                        Tickets = PortfolioDashboardSiteTickets.MapFrom(await GetSitesTicketsAsync(siteList))?.FirstOrDefault(),
                        Gateways = gateways
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get portfolio dashboard for site", site.Id);
                }

                return null;
            });
        }

        private async Task<List<PortfolioDashboardSiteStatus>> GetDashboardDataForSitesAsync(List<Site> sites)
        {
            var insightsList = await GetSitesInsightsAsync(sites);
            var ticketsList = await GetSitesTicketsAsync(sites);

            var connectivityStatistics = await _connectorApi.GetGatewaysForMultiSitesAsync(sites.Select(x => x.Id).ToList());

            var portfolioDashboardSiteStatus = new List<PortfolioDashboardSiteStatus>();

            foreach(var site in sites)
            {
                try
                {
                    var gateways = await GetGatewayAndConnectorStatusForMultiSitesAsync(connectivityStatistics.Where(x => x.SiteId == site.Id).FirstOrDefault());

                    portfolioDashboardSiteStatus.Add(new PortfolioDashboardSiteStatus
                    {
                        SiteId = site.Id,
                        Name = site.Name,
                        Country = site.Country,
                        State = site.State,
                        Suburb = site.Suburb,
                        Status = GetSiteStatus(site, gateways),
                        PointCount = await GetPointCountAsync(site.Id, gateways),
                        Insights = insightsList?.Where(x => x.Id == site.Id)
                                            .Select(x => new PortfolioDashboardSiteInsights{
                                                    UrgentCount = x.UrgentCount,
                                                    OpenCount = x.OpenCount,
                                                    HighCount = x.HighCount,
                                                    MediumCount = x.MediumCount
                                            })?.FirstOrDefault(),
                        Tickets = ticketsList?.Where(x => x.Id == site.Id)
                                            .Select(x => new PortfolioDashboardSiteTickets{
                                                    OverdueCount = x.OverdueCount,
                                                    UnresolvedCount = 0,
                                                    ResolvedCount = 0,
                                                    HighCount = x.HighCount,
                                                    MediumCount = x.MediumCount,
                                                    UrgentCount = x.UrgentCount
                                            })?.FirstOrDefault(),
                        Gateways = gateways
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get portfolio dashboard for site", site.Id);
                }
            }
            return portfolioDashboardSiteStatus;
        }

        public async Task<PortfolioDashboardSiteStatus> GetSiteSimpleDashboardDataAsync(Site site)
        {
            return await _memoryCache.GetOrCreateAsync($"GetSiteSimpleDashboardDataAsync_{site.Id}", async (entry) => {
                entry.AbsoluteExpirationRelativeToNow = CachePeriod;
                try
                {
                    var gateways = await GetGatewayAndConnectorStatusForSiteAsync(site.Id);
                    return new PortfolioDashboardSiteStatus
                    {
                        SiteId = site.Id,
                        Status = GetSiteStatus(site, gateways),
                        Gateways = gateways
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get portfolio dashboard for site", site.Id);
                }

                return null;
            });
        }

        private async Task<List<PortfolioDashboardGatewayStatus>> GetGatewayAndConnectorStatusForSiteAsync(Guid siteId)
        {
            var output = new List<PortfolioDashboardGatewayStatus>();

            foreach (var gateway in await GetGatewayAsync(siteId))
            {
                var gatewayOutput = new PortfolioDashboardGatewayStatus
                {
                    Connectors = await MapConnectorsAsync(gateway.Connectors),
                    GatewayId = gateway.Id,
                    Name = gateway.Name,
                    LastUpdated = gateway.LastUpdatedAt,
                    Status = MapGatewayStatus(gateway)
                };
                output.Add(gatewayOutput);
            }

            return output;
        }

        private async Task<List<PortfolioDashboardGatewayStatus>> GetGatewayAndConnectorStatusForMultiSitesAsync(ConnectivityStatistics connectivityStatistics)
        {
            var output = new List<PortfolioDashboardGatewayStatus>();

            if (!connectivityStatistics.gateways.Any())
            {
                //If no gateway defined, add all connectors to a default gateway, and presume it is online
                if (connectivityStatistics.connectors.Any())
                {
                    connectivityStatistics.gateways = new List<Gateway> { 
                        new Gateway
                        {
                            Name = "Unknown Gateway",
                            Connectors = connectivityStatistics.connectors,
                            CustomerId = connectivityStatistics.connectors.First().ClientId,
                            Host = null,
                            Id = Guid.Empty,
                            IsEnabled = true,
                            IsOnline = true,
                            LastUpdatedAt = DateTime.UtcNow,
                            SiteId = connectivityStatistics.SiteId
                        } 
                    };
                }
            }

            foreach (var gateway in connectivityStatistics.gateways)
            {
                var gatewayOutput = new PortfolioDashboardGatewayStatus
                {
                    Connectors = await MapConnectorsAsync(gateway.Connectors),
                    GatewayId = gateway.Id,
                    Name = gateway.Name,
                    LastUpdated = gateway.LastUpdatedAt,
                    Status = MapGatewayStatus(gateway)
                };
                output.Add(gatewayOutput);
            }

            return output;
        }

        private async Task<List<Gateway>> GetGatewayAsync(Guid siteId)
        {
            var gateways = await _connectorApi.GetGatewaysForSiteAsync(siteId);

            if (!gateways.Any())
            {
                //If no gateway defined, add all connectors to a default gateway, and presume it is online
                var connectors = await _connectorApi.GetConnectorsAsync(siteId, true);

                if (connectors.Any())
                {
                    gateways = new List<Gateway> { 
                        new Gateway
                        {
                            Name = "Unknown Gateway",
                            Connectors = connectors,
                            CustomerId = connectors.First().ClientId,
                            Host = null,
                            Id = Guid.Empty,
                            IsEnabled = true,
                            IsOnline = true,
                            LastUpdatedAt = DateTime.UtcNow,
                            SiteId = siteId
                        } 
                    };
                }
            }

            return gateways;
        }

        private static ServiceStatus MapGatewayStatus(Gateway gateway) =>
            gateway.IsEnabled
                ? gateway.IsOnline.GetValueOrDefault() ? ServiceStatus.Online : ServiceStatus.Offline
                : ServiceStatus.NotOperational;

        private async Task<List<PortfolioDashboardConnectorStatus>> MapConnectorsAsync(IEnumerable<Connector> connectors)
        {
            var output = new List<PortfolioDashboardConnectorStatus>();

            if (connectors == null)
            {
                return new List<PortfolioDashboardConnectorStatus>();
            }
            foreach (var connector in connectors.Where(c => c.IsEnabled))
            {
                var logRecord = (await _connectorApi.GetLatestConnectorLogsAsync(connector.Id, count:1, includeErrors:true, source:"Connector")).FirstOrDefault();
                output.Add(PortfolioDashboardConnectorStatus.MapFrom(connector, logRecord));
            }

            return output;
        }

        private static ServiceStatus GetSiteStatus(Site site, List<PortfolioDashboardGatewayStatus> gateways)
        {
            if (site.Status != SiteStatus.Operations || gateways == null || !gateways.Any())
            {
                return ServiceStatus.NotOperational;
            }

            var hasOnlineGateways = gateways.Any(g => g.Status == ServiceStatus.Online);
            var hasOnlineConnectors = gateways.SelectMany(g => g.Connectors).Select(c => c.Status).Any(s => s == ServiceStatus.Online || s == ServiceStatus.OnlineWithErrors);

            if (hasOnlineConnectors || hasOnlineGateways)
            {
                return ServiceStatus.Online;
            }

            if (hasOnlineGateways)
            {
                var connectorStatuses = gateways.Where(g => g.Status == ServiceStatus.Online).SelectMany(g => g.Connectors).Select(c => c.Status);

                if (connectorStatuses.Any(s => s == ServiceStatus.Offline || s == ServiceStatus.OnlineWithErrors))
                {
                    return connectorStatuses.Any(s => s != ServiceStatus.Offline) ? ServiceStatus.OnlineWithErrors : ServiceStatus.Offline;
                }
                else if (connectorStatuses.Any(s => s == ServiceStatus.Online))
                {
                    return ServiceStatus.Online;
                }
                else
                {
                    return ServiceStatus.NotOperational;
                }
            }
            else
            {
                return site.Status != SiteStatus.Operations || gateways.Any(g => g.Status == ServiceStatus.NotOperational)
                    ? ServiceStatus.NotOperational
                    : ServiceStatus.Offline;
            }
        }

        private async Task<int> GetPointCountAsync(Guid siteId, List<PortfolioDashboardGatewayStatus> gateways)
        {
            try
            {
                return await _digitalTwin.GetPointCountAsync(siteId);
            }
            catch (RestException ex)
            {
                _logger.LogError(ex, "Failed to get point count for site from DigitalTwinCore", siteId);
            }

            var allConnectors = gateways.SelectMany(g => g.Connectors).ToList();
            return allConnectors.Sum(c => c.PointCount.GetValueOrDefault());
        }

        private async Task<List<SiteInsightStatistics>> GetSitesInsightsAsync(List<Site> sites)
        {
            var siteIdsWithInsightEnabled = sites.Where(x => !x.Features.IsInsightsDisabled)
                                                    .Select(x => x.Id)
                                                    .ToList();
            if(siteIdsWithInsightEnabled == null || siteIdsWithInsightEnabled.Count <= 0)
            {
                return null;
            }

            var insightStatisticsList =  await _insightApi.GetInsightStatisticsBySiteIds(siteIdsWithInsightEnabled);
            return insightStatisticsList.StatisticsByPriority;
        }

        private async Task<List<SiteTicketStatistics>> GetSitesTicketsAsync(List<Site> sites)
        {
            var siteIdsWithTicketingEnabled = sites.Where(x => !x.Features.IsTicketingDisabled)
                                                    .Select(x => x.Id)
                                                    .ToList();
            if(siteIdsWithTicketingEnabled == null || siteIdsWithTicketingEnabled.Count <= 0)
            {
                return null;
            }
            var siteStatisticsList =  await _workflowApi.GetSiteStatistics(siteIdsWithTicketingEnabled);
            return siteStatisticsList;
        }
    }
}
