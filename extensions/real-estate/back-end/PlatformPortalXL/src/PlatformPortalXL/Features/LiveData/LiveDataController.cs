using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.LiveDataApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Dto;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.Services.Assets;
using System.Net;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Api.Client;
using Willow.Common;
using Willow.Platform.Models;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.LiveData
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class LiveDataController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IConnectorApiService _connectorApi;
        private readonly ILiveDataApiService _liveDataApi;
        private readonly ISiteApiService _siteApiService;
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IConnectorService _connectorService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public LiveDataController(
            IAccessControlService accessControl,
            IConnectorApiService connectorApi,
            ILiveDataApiService liveDataApi,
            ISiteApiService siteApiService,
            IDigitalTwinAssetService digitalTwinService,
            IDirectoryApiService directoryApi,
            IDigitalTwinApiService digitalTwinApiService,
            IConnectorService connectorService,
            IUserAuthorizedSitesService userAuthorizedSitesService)
        {
            _accessControl = accessControl;
            _connectorApi = connectorApi;
            _liveDataApi = liveDataApi;
            _siteApiService = siteApiService;
            _digitalTwinService = digitalTwinService;
            _directoryApi = directoryApi;
            _digitalTwinApiService = digitalTwinApiService;
            _connectorService = connectorService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
        }

        [HttpGet("sites/{siteId}/categories")]
        [Authorize]
        [ProducesResponseType(typeof(IList<string>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of categories", Tags = new[] { "Sites" })]
        public async Task<ActionResult<IList<Category>>> GetCategoriesBySite([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            List<Category> categories;

                var assetCategories = await _digitalTwinService.GetAssetCategoriesTreeAsync(siteId, floorId: null, liveDataAssetsOnly: true, searchKeyword: string.Empty, isCategoryOnly: true);
                var flattenedAssetCategories = FlattenAssetCategory(assetCategories);
                categories = flattenedAssetCategories.Where(x => x.Assets != null && x.Assets.Count > 0)
                                                     .Select(x => new Category { Id = x.Id, Name = x.Name })
                                                     .OrderBy(x => x.Name)
                                                     .ToList();

            return categories;
        }

        [HttpPost("customers/{customerId}/portfolio/{portfolioId}/livedata/stats/connectors")]
        [Authorize]
        [ProducesResponseType(typeof(List<ConnectorStatsDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets connectors stats per portfolio", Tags = new[] { "Sites" })]
        public async Task<ActionResult<List<ConnectorStatsDto>>> GetConnectorsStatsForPortfolio(
            [FromRoute] Guid customerId,
            [FromRoute] Guid portfolioId,
            [FromBody] LiveDataConnectorStatsRequest request)
        {
            await _accessControl.EnsureAccessPortfolio(this.GetCurrentUserId(), new CanViewConnectors(), Permissions.ViewPortfolios, portfolioId);

            var siteIds = (await _siteApiService.GetSites(customerId, portfolioId)).Select(x => x.Id).ToArray();
            var connectors = await _connectorService.GetPortfolioConnectors(siteIds, request.ConnectorIds);
            var connectorStats = await _liveDataApi.GetConnectorStats(request.Start, request.End, connectors);
            var connectorStatsDto = ConnectorStatsDto.MapFromModels(connectors, connectorStats);
            var siteConnectorStatsDto = SiteConnectorStatsDto.MapFromModels(siteIds, connectorStatsDto);

            return Ok(siteConnectorStatsDto);
        }

        [HttpGet("sites/{siteId}/livedata/stats/connectors")]
        [Authorize]
        [ProducesResponseType(typeof(List<ConnectorStatsDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets connectors stats per site", Tags = new[] { "Sites" })]
        public async Task<ActionResult<List<ConnectorStatsDto>>> GetConnectorsStatsForSite(
            [FromRoute] Guid siteId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var connectors = await _connectorService.GetSiteConnectors(siteId, false);
            var connectorStats = await _liveDataApi.GetConnectorStats(start, end, connectors);
            var connectorStatsDto = ConnectorStatsDto.MapFromModels(connectors, connectorStats);

            return Ok(connectorStatsDto);
        }

        [Obsolete("Temporary solution for old shared links")]
        [HttpGet("timemachine/models")]
        [Authorize]
        [ProducesResponseType(typeof(List<QueryEquipmentsOrPointsResponseItem>), StatusCodes.Status200OK)]
        [SwaggerOperation("Query equipments or points", Tags = new[] { "Temporary" })]
        public async Task<IActionResult> QueryEquipmentsOrPoints([FromQuery] Guid[] equipmentIds, [FromQuery] Guid[] pointIds)
        {
            var result = new List<QueryEquipmentsOrPointsResponseItem>();
            foreach (var equipmentId in equipmentIds)
            {
                try
                {
                    var equipment = await _connectorApi.GetEquipment(equipmentId, true, true);
                    result.Add(new QueryEquipmentsOrPointsResponseItem
                    {
                        EquipmentId = equipmentId,
                        SiteId = equipment.SiteId
                    });
                }
                catch (RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
            foreach (var pointId in pointIds)
            {
                try
                {
                    var point = await _connectorApi.GetPointById(pointId);
                    result.Add(new QueryEquipmentsOrPointsResponseItem
                    {
                        PointId = pointId,
                        PointEntityId = point.EntityId,
                        SiteId = point.SiteId
                    });
                }
                catch (RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                }
            }
            return Ok(result);
        }

        [HttpGet("sites/{siteId}/categories/{categoryId}/equipments")]
        [Authorize]
        [ProducesResponseType(typeof(IList<Equipment>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of equipment", Tags = new[] { "Sites" })]
        public async Task<ActionResult<IList<Equipment>>> GetCategoryEquipments([FromRoute] Guid siteId, [FromRoute] Guid categoryId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);


           var assets = await _digitalTwinService.GetAssetsAsync(siteId, categoryId, floorId: null, liveDataAssetsOnly: true, subCategories: null, searchKeyword: string.Empty);
           var equipments = assets.Select(a => new Equipment
           {
               Id = a.Id,
               Name = a.Name,
               SiteId = siteId,
               FloorId = a.FloorId,
               PointTags = a.PointTags,
               Tags = a.Tags
           }).ToList();


            return equipments;
        }

        [Obsolete("Use new endpoint 'GET sites/{siteId}/equipments/{equipmentId}' instead")]
        [HttpGet("equipments/{equipmentId}")]
        [Authorize]
        [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get the equipment with its points", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetEquipmentWithPointsObsolete([FromRoute] Guid equipmentId)
        {
            var equipment = await _connectorApi.GetEquipment(equipmentId, true, true);
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, equipment.SiteId);

            if (equipment.SiteId == Guid.Parse("3148E57E-9366-4438-A9AF-90F0ED175CF6")
                || equipment.SiteId == Guid.Parse("53d380c2-d31a-4cd1-8958-795407407a82")) // DEMO: SiteID for Microsoft Building121
            {
                equipment.Points = equipment.Points.Select(x => new Point
                {
                    Id = x.EntityId,
                    EntityId = x.EntityId,
                    Name = x.Name,
                    EquipmentId = equipmentId,
                    ExternalPointId = x.Id.ToString(),
                    Tags = x.Tags.Select(t => new Tag { Name = t.Name, Feature = "feature" }).ToList(),
                }).ToList();
            }

            // Make sure all point.Tags are populated, so 'PointSimpleDto.HasFeaturedTags' can be calculated correctly
            foreach (var point in equipment.Points)
            {
                if (point.Tags == null)
                {
                    point.Tags = new Tag[0];
                }
            }

            return Ok(EquipmentDto.MapFrom(equipment));
        }

        [HttpGet("sites/{siteId}/equipments/{equipmentId}")]
        [Authorize]
        [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get the equipment with its points", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetEquipmentWithPoints([FromRoute] Guid siteId, [FromRoute] Guid equipmentId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, equipmentId);
            var equipment = new Equipment
            {
                Id = asset.Id,
                Name = asset.Name,
                SiteId = siteId,
                FloorId = asset.FloorId,
                PointTags = asset.PointTags,
                Tags = asset.Tags,
                Points = asset.Points.Select(x => new Point
                {
                    Id = x.Id,
                    EntityId = x.Id,
                    Name = x.Name,
                    EquipmentId = equipmentId,
                    ExternalPointId = x.ExternalId ?? string.Empty,
                    Tags = x.Tags.Select(t => new Tag { Name = t.Name, Feature = t.Feature }).ToList(),
                    DisplayPriority = x.DisplayPriority,
                    Unit = x.Unit,
                    Type = x.Type
                }).OrderBy(x => x.DisplayPriority).ToList()
            };

                // Make sure all point.Tags are populated, so 'PointSimpleDto.HasFeaturedTags' can be calculated correctly
            foreach (var point in equipment.Points)
            {
                if (point.Tags == null)
                {
                    point.Tags = new Tag[0];
                }
            }

            return Ok(EquipmentDto.MapFrom(equipment, true));
        }

        [Obsolete("Use new endpoint 'GET sites/{siteId}/points/{pointId}/livedata' instead")]
        [HttpGet("livedata/points/{pointId}/data")]
        [Authorize]
        [ProducesResponseType(typeof(PointLiveDataBase), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets live data of the given point", Tags = new[] { "Sites" })]
        public async Task<ActionResult<PointLiveDataBase>> GetPointLiveDataObsolete(
            [FromRoute] Guid pointId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "interval")] TimeSpan? interval)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();

            var point = await _connectorApi.GetPoint(pointId);

            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, point.SiteId);

            PointLiveDataBase result;
            switch (point.Type)
            {
                case PointType.Analog:
                    result = new PointLiveDataAnalog
                    {
                        TimeSeriesData = await _liveDataApi.GetTimeSeriesAnalogData(point.CustomerId, point.TwinId, start, end, interval),
                        PointType = PointType.Analog
                    };
                    break;
                case PointType.Binary:
                    result = new PointLiveDataBinary
                    {
                        TimeSeriesData = await _liveDataApi.GetTimeSeriesBinaryData(point.CustomerId, point.TwinId, start, end, interval),
                        PointType = PointType.Binary
                    };
                    break;
                case PointType.MultiState:
                    result = new PointLiveDataMultiState()
                    {
                        TimeSeriesData = await _liveDataApi.GetTimeSeriesMultiStateData(point.CustomerId, point.TwinId, start, end, interval),
                        PointType = PointType.MultiState,
                        ValueMap = point.Properties.TryGetValue("valueMap", out var property) ? property.Value.FromNewtonsoftJsonObject() : null,
                    };
                    break;
                case PointType.Sum:
                    result = new PointLiveDataSum
                    {
                        TimeSeriesData = await _liveDataApi.GetTimeSeriesSumData(point.CustomerId, point.TwinId, start, end, interval),
                        PointType = PointType.Analog
                    };
                    break;
                default:
                    throw new ArgumentException().WithData(new { pointType = point.Type });
            }

            result.PointId = point.Id;
            result.PointName = point.Name;
            result.Unit = point.Unit;
            result.PointEntityId = point.EntityId;

            if (point.SiteId == Guid.Parse("3148E57E-9366-4438-A9AF-90F0ED175CF6")
                || point.SiteId == Guid.Parse("53d380c2-d31a-4cd1-8958-795407407a82")) // DEMO: SiteID for Microsoft Building121
            {
                result.PointId = result.PointEntityId;
            }

            return result;
        }

        [HttpGet("sites/{siteId}/points/{pointId}/livedata")]
        [Authorize]
        [ProducesResponseType(typeof(PointLiveDataBase), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets live data of the given point", Tags = new[] { "Sites" })]
        public async Task<ActionResult<PointLiveDataBase>> GetPointLiveData(
            [FromRoute] Guid siteId,
            [FromRoute] Guid pointId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, CultureInfo.InvariantCulture, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            start = start.ToUniversalTime();
            end = end.ToUniversalTime();

            var site = await _siteApiService.GetSite(siteId);

            var point = await _digitalTwinService.GetPointAsync(siteId, pointId);
            // TODO Switch to using point.TwinId to start using the telemetry endpoint in LiveData.Core instead of using the trendid
            PointLiveDataBase result = point.Type switch
            {
                PointType.Analog => new PointLiveDataAnalog
                {
                    TimeSeriesData = await _liveDataApi.GetTimeSeriesAnalogData(site.CustomerId, point.TwinId, start, end, interval),
                    PointType = PointType.Analog
                },
                PointType.Binary => new PointLiveDataBinary
                {
                    TimeSeriesData = await _liveDataApi.GetTimeSeriesBinaryData(site.CustomerId, point.TwinId, start, end, interval),
                    PointType = PointType.Binary
                },
                PointType.MultiState => new PointLiveDataMultiState
                {
                    TimeSeriesData = await _liveDataApi.GetTimeSeriesMultiStateData(site.CustomerId, point.TwinId, start, end, interval),
                    PointType = PointType.MultiState,
                    ValueMap = point.Properties.TryGetValue("valueMap", out var property) ? property.Value.FromNewtonsoftJsonObject() : null,
                },
                PointType.Sum => new PointLiveDataSum
                {
                    TimeSeriesData = await _liveDataApi.GetTimeSeriesSumData(site.CustomerId, point.TwinId, start, end, interval),
                    PointType = PointType.Analog
                },
                _ => throw new ArgumentException().WithData(new
                {
                    pointType = point.Type
                })
            };

            result.PointId = point.Id;
            result.PointName = point.Name;
            result.Unit = point.Unit;
            result.PointEntityId = point.Id;
            result.DisplayPriority = point.DisplayPriority;
            return result;
        }

        [HttpPost("livedata/export/csv")]
        [Authorize]
        [ProducesResponseType(typeof(PointLiveDataBase), StatusCodes.Status200OK)]
        [SwaggerOperation("Export live data of the given points into csv", Tags = new[] { "Sites" })]
        public async Task<IActionResult> DownloadLiveDataCsv([FromBody] LiveDataCsvRequest request)
        {
            var interval = string.IsNullOrWhiteSpace(request.Interval) ||
                           !TimeSpan.TryParse(request.Interval, CultureInfo.InvariantCulture, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;
            var start = request.Start.ToUniversalTime();
            var end = request.End.ToUniversalTime();

            var groupedBySite = request.Points.GroupBy(p => p.SiteId).ToDictionary(sp => sp.Key, sp => sp.Select(p => p.PointId));

            var userId = this.GetCurrentUserId();

            var analoguePoints = new List<Point>();
            var binaryPoints = new List<Point>();
            var multiStatePoints = new List<Point>();
            var sumPoints = new List<Point>();
            foreach (var sp in groupedBySite)
            {
                var siteId = sp.Key;
                var pointIds = sp.Value;

                var adtPoints =  await _digitalTwinApiService.GetPointsByEntityIds(siteId, pointIds);

                var connectorCorePoints = await _connectorApi.GetPoints(pointIds.ToArray());

                var excludedConnectorCorePoints = connectorCorePoints
                    .Where(cp => !adtPoints.Select(ap => ap.Id)
                    .Contains(cp.Id));

                var sitePoints = adtPoints as List<Point>;
                sitePoints.AddRange(excludedConnectorCorePoints);

                var pointsByType = await SplitPointsByType(siteId, userId, sitePoints);

                analoguePoints.AddRange(pointsByType.AnalogPoints);
                multiStatePoints.AddRange(pointsByType.MultiStatePoints);
                binaryPoints.AddRange(pointsByType.BinaryPoints);
                sumPoints.AddRange(pointsByType.SumPoints);
            }

            var siteNameById = (await _siteApiService.GetSites(groupedBySite.Keys)).ToDictionary(x => x.Id, x => x.Name);
            var user = await _directoryApi.GetUser(userId);
            var customer = await _directoryApi.GetCustomer(user.CustomerId);

            var content = await GetFileContent(customer.Id, start, end, interval, analoguePoints, binaryPoints, multiStatePoints, sumPoints, siteNameById, request.TimeZoneId);

            return File(content, "application/octet-stream", "timeMachine.csv");
        }

        [Obsolete]
        [HttpGet("livedata/export/csv")]
        [Authorize]
        [ProducesResponseType(typeof(PointLiveDataBase), StatusCodes.Status200OK)]
        [SwaggerOperation("Export live data of the given points into csv", Tags = new[] { "Sites" })]
        public async Task<IActionResult> DownloadPointLiveDataCsv([FromQuery(Name = "pointEntityId"), BindRequired] Guid[] pointEntityIds,
                [FromQuery(Name = "start"), BindRequired] DateTime start,
                [FromQuery(Name = "end"), BindRequired] DateTime end, [FromQuery(Name = "interval")] string selectedInterval)

        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, CultureInfo.InvariantCulture, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            start = start.ToUniversalTime();
            end = end.ToUniversalTime();


            // currently for all DTCore calls, a site id is needed
            // however the result is not filtered by the site id passed
            // so here the first site id for the current customer is used
            var site = await GetFirstAdtSiteForCurrentUser();

            var adtPoints = site == null
                                ? new List<Point>()
                                : await _digitalTwinApiService.GetPointsByEntityIds(site.Id, pointEntityIds);

            var connectorCorePoints = await _connectorApi.GetPoints(pointEntityIds);
            var points = new List<Point>();
            var excludedConnectorCorePoints =
                connectorCorePoints.Where(cp => !adtPoints.Select(ap => ap.Id)
                                                          .Contains(cp.Id));

            points.AddRange(adtPoints);
            points.AddRange(excludedConnectorCorePoints);

            var usedSiteIds = new HashSet<Guid>();
            var binaryPoints = new List<Point>(points.Count);
            var analoguePoints = new List<Point>(points.Count);
            var multiStatePoints = new List<Point>(points.Count);
            var sumPoints = new List<Point>(points.Count);
            foreach (var point in points)
            {
                if (!usedSiteIds.Contains(point.SiteId))
                {
                    await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, point.SiteId);
                    usedSiteIds.Add(point.SiteId);
                }

                switch (point.Type)
                {
                    case PointType.Analog:
                        analoguePoints.Add(point);
                        break;
                    case PointType.Binary:
                        binaryPoints.Add(point);
                        break;
                    case PointType.MultiState:
                        multiStatePoints.Add(point);
                        break;
                    case PointType.Sum:
                        sumPoints.Add(point);
                        break;
                    default:
                        throw new ArgumentException().WithData(new { pointType = point.Type, pointId = point.Id });
                }
            }

            var siteNameById = (await _siteApiService.GetSites(usedSiteIds)).ToDictionary(x => x.Id, x => x.Name);
            var userId = this.GetCurrentUserId();
            var user = await _directoryApi.GetUser(userId);
            var customer = await _directoryApi.GetCustomer(user.CustomerId);

            var content = await GetFileContent(customer.Id, start, end, interval, analoguePoints, binaryPoints, multiStatePoints, sumPoints, siteNameById);

            return File(content, "application/octet-stream", "timeMachine.csv");
        }

        private async Task<(List<Point> AnalogPoints, List<Point> MultiStatePoints, List<Point> BinaryPoints, List<Point> SumPoints)> SplitPointsByType(Guid siteId, Guid userId, List<Point> points)
        {
            await _accessControl.EnsureAccessSite(userId, Permissions.ViewSites, siteId);

            var binaryPoints = new List<Point>(points.Count);
            var multiStatePoints = new List<Point>(points.Count);
            var analoguePoints = new List<Point>(points.Count);
            var sumPoints = new List<Point>(points.Count);
            foreach (var point in points)
            {
                switch (point.Type)
                {
                    case PointType.Analog:
                        analoguePoints.Add(point);
                        break;
                    case PointType.Binary:
                        binaryPoints.Add(point);
                        break;
                    case PointType.MultiState:
                        multiStatePoints.Add(point);
                        break;
                    case PointType.Sum:
                        sumPoints.Add(point);
                        break;
                    default:
                        throw new ArgumentException().WithData(new { pointType = point.Type, pointId = point.Id });
                }
            }

            return (analoguePoints, multiStatePoints, binaryPoints, sumPoints);
        }

        private async Task<byte[]> GetFileContent(
            Guid customerId,
            DateTime start,
            DateTime end,
            TimeSpan? interval,
            List<Point> analoguePoints,
            List<Point> binaryPoints,
            List<Point> multiStatePoints,
            List<Point> sumPoints,
            Dictionary<Guid, string> siteNameById,
            string timeZoneId = null)
        {
            TimeZoneInfo timeZoneInfo = null;
            var timeZone = string.Empty;
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                timeZoneInfo = TimeZoneConverter.TZConvert.GetTimeZoneInfo(timeZoneId);
                timeZone = TimeZoneNames.TZNames.GetDisplayNameForTimeZone(timeZoneId,"en");
            }

            await using var memoryStream = new MemoryStream();
            await using var writer = new StreamWriter(memoryStream);

            await writer.WriteLineAsync("ExternalPointID,Tags,Parent Equipment Name,Site Name,FrequencyUnit,Time,Timezone,OnCount,OffCount,Average,Minimum,Maximum,Sum,State");

            await WritePointsToCsv(customerId, start, end, interval, analoguePoints, siteNameById, writer,
                                   _liveDataApi.GetTimeSeriesAnalogData,
                                   data => $",{data.Timestamp: MM/dd/yyyy hh:mm:ss tt},\"{timeZone}\",,,{data.Average},{data.Minimum},{data.Maximum},", timeZoneInfo);

            await WritePointsToCsv(customerId, start, end, interval, multiStatePoints, siteNameById, writer,
                _liveDataApi.GetTimeSeriesMultiStateData,
                data => $",{data.Timestamp: MM/dd/yyyy HH:mm:ss tt},\"{timeZone}\",,,,,,,{System.Text.Json.JsonSerializer.Serialize(data.State)}", timeZoneInfo);

            await WritePointsToCsv(customerId, start, end, interval, binaryPoints, siteNameById, writer,
                                   _liveDataApi.GetTimeSeriesBinaryData,
                                   data => $",{data.Timestamp: MM/dd/yyyy HH:mm:ss tt},\"{timeZone}\",{data.OnCount},{data.OffCount},,,,", timeZoneInfo);

            await WritePointsToCsv(customerId, start, end, interval, sumPoints, siteNameById, writer,
                                   _liveDataApi.GetTimeSeriesSumData,
                                   data => $",{data.Timestamp: MM/dd/yyyy HH:mm:ss tt},\"{timeZone}\",,,,,,{data.Average}", timeZoneInfo);

            await writer.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream.ToArray();
        }

        private static async Task WritePointsToCsv<T>(
            Guid customerId,
            DateTime start,
            DateTime end,
            TimeSpan? interval,
            IList<Point> points,
            IReadOnlyDictionary<Guid, string> siteNameById,
            TextWriter writer,
            GetTimeSeriesData<T> getTimeSeriesData,
            Func<T, string> dataSeriesPostfixFunc,
            TimeZoneInfo timeZoneInfo)
            where T : TimeSeriesData
        {
            if (!points.Any())
            {
                return;
            }

            var seriesDataByPointId = await getTimeSeriesData(customerId,
                                                              points.Select(x => x.TwinId),
                                                              start,
                                                              end,
                                                              interval);
            foreach (var point in points)
            {
                if (seriesDataByPointId == null || !seriesDataByPointId.TryGetValue(point.TwinId, out var seriesData))
                {
                    continue;
                }

                var siteName = siteNameById[point.SiteId];
                var equipmentName = string.Join(",", point.Equipment.Select(x => x.Name));
                var tagsFormatted = string.Join(",", point.Tags.Select(x => x.Name));
                var linePrefix =
                    $"{point.ExternalPointId},\"{tagsFormatted}\",\"{equipmentName}\",{siteName},\"{point.Unit}\"";
                foreach (var data in seriesData)
                {
                    if (timeZoneInfo != null)
                    {
                        data.Timestamp = TimeZoneInfo.ConvertTime(data.Timestamp, timeZoneInfo);
                    }

                    await writer.WriteLineAsync(linePrefix + dataSeriesPostfixFunc(data));
                }
            }
        }

        private async Task<Site> GetFirstAdtSiteForCurrentUser()
        {
            var userId = this.GetCurrentUserId();
            return (await _userAuthorizedSitesService.GetAuthorizedSites(userId, Permissions.ViewSites)).FirstOrDefault();

        }

        private static List<AssetCategory> FlattenAssetCategory(IList<AssetCategory> categories)
        {
            var result = new List<AssetCategory>();
            foreach (var category in categories)
            {
                if (category.Categories != null)
                {
                    result.AddRange(FlattenAssetCategory(category.Categories));
                }
            }
            result.AddRange(categories);
            return result;
        }
    }
}
