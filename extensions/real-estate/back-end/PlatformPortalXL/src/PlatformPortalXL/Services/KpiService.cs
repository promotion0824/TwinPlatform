using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KPI.Service.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Api.Client;
using Willow.Data;
using Willow.ExceptionHandling.Exceptions;
using Willow.KPI.Service;
using Willow.Platform.Models;

namespace PlatformPortalXL.Services
{
    public interface IKPIService
    {
        Task<IEnumerable<Metric>> GetByMetric(Guid portfolioId, string viewName, object filter = null, bool sortByX = true, bool mapSiteName = true);
        Task<DatedKPIValuesWithTrendResponse> GetDatedBuildingScore(Guid portfolioId, string spaceTwinId, string score, KPIRequest filter = null);
        Task<List<KPIBuildingComfortDashboardDto>> GetBuildingDashboardData(string scopeId, KPIViewNames viewName, KPIBaseRequest request);
        Task<List<BuildingPerformanceScoresResponse>> GetPerformanceScoresByDate(KPIPerformanceScoresRequest request);
    }

    public class KPIService : IKPIService
    {
        private readonly Guid _customerId;
        private readonly ICoreKPIService _kpiApi;
        private readonly IReadRepository<Guid, Site> _siteRepo;
        private readonly IDigitalTwinApiService _dtApiService;
        private readonly IMemoryCache _cache;
        private readonly int _cacheDuration;
        private readonly ILogger<KPIService> _logger;

        public KPIService(Guid customerId, ICoreKPIService coreKPIApi, IReadRepository<Guid, Site> siteRepo, IDigitalTwinApiService dtApiService, IMemoryCache cache, int cacheDurationInHours, ILogger<KPIService> logger)
        {
            _customerId = customerId;
            _kpiApi = coreKPIApi ?? throw new ArgumentNullException(nameof(coreKPIApi));
            _siteRepo = siteRepo ?? throw new ArgumentNullException(nameof(siteRepo));
            _dtApiService = dtApiService ?? throw new ArgumentNullException(nameof(dtApiService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheDuration = cacheDurationInHours;
            _logger = logger;
        }

        public async Task<IEnumerable<Metric>> GetByMetric(Guid portfolioId, string viewName, object filter, bool sortByX = true, bool mapSiteName = true)
        {
            // Remove prefixes and suffixes
            if (viewName.StartsWith("get_", StringComparison.InvariantCultureIgnoreCase))
                viewName = viewName["get_".Length..];

            if (viewName.EndsWith("_data", StringComparison.InvariantCultureIgnoreCase))
                viewName = viewName[..^"_data".Length];

            // Allow some variation on names
            switch (viewName)
            {
                case "operationaltrends":
                case "operational_trend_metrics": viewName = "trends"; break;
                case "overallperformance": viewName = "overall_performance"; break;
                case "buildings": viewName = "building"; break;
                default: break;
            }

            // These views should sort
            sortByX = viewName == "trends";

            // Check if it's in the cache
            var filterHash = filter != null ? filter.GetHashCode().ToString() : "";
            var cacheKey = $"{_customerId}-{portfolioId}-{viewName}-{filterHash}";

            if (_cache.TryGetValue(cacheKey, out object value))
                return value as IEnumerable<Metric>;

            var result = await _kpiApi.GetByMetric(portfolioId, viewName, filter, sortByX);

            // Map these view's site id's to building names
            if (mapSiteName)
                switch (viewName)
                {
                    case "building":
                    case "kpis":
                        await MapSiteNames(result);
                        break;
                }

            // Cache for two hours
            _cache.Set(cacheKey, result, TimeSpan.FromHours(_cacheDuration));

            return result;
        }


        public async Task<DatedKPIValuesWithTrendResponse> GetDatedBuildingScore(Guid portfolioId, string spaceTwinId, string score, KPIRequest request)
        {
            var viewName = "building";

            // Check if it's in the cache
            var requestHash = request != null ? request.GetHashCode().ToString() : "";
            var cacheKey = $"{viewName}-{spaceTwinId}-{score}-{requestHash}";

            if (_cache.TryGetValue(cacheKey, out object value))
                return value as DatedKPIValuesWithTrendResponse;

            if (!string.IsNullOrWhiteSpace(spaceTwinId))
            {
                var twin = await _dtApiService.GetTwin<TwinDto>(twinId: spaceTwinId);
                if (!_dtApiService.IsBuildingScopeModelId(twin?.Metadata?.ModelId))
                {
                    throw new ArgumentOutOfRangeException(spaceTwinId, "Twin must be a 'Building' model type.");
                }
                request.SiteIds = twin.SiteId.Value.ToString();
            }
            else if (string.IsNullOrWhiteSpace(request.SiteIds))
            {
                throw new ArgumentNullException("request.SiteIds", "SiteIds must be provided if spaceTwinId is empty.");
            }

            // Retrieve target dated results
            request.GroupBy = "Date";
            var scoreDatedResults = (await _kpiApi.GetByMetric(portfolioId, viewName, request.ToDictionary(),isDateMetric:true)).FirstOrDefault(c => c.Name.Contains(score,StringComparison.OrdinalIgnoreCase));

            // Trend result (previous period
            var trendRequest = new KPIRequest()
            {
                SiteIds = request.SiteIds,
                StartDate = request.StartDate?.AddDays(-1 * ((request.EndDate - request.StartDate).Value.Days + 1)),
                EndDate = request.StartDate?.AddDays(-1),
                GroupBy = "Date"
            };

            var result = new DatedKPIValuesWithTrendResponse
            {
                Name = scoreDatedResults?.Name,
                ValuesByDate = scoreDatedResults?.Values?.Select(MapToDatedValue)?.ToList(),
                Unit = scoreDatedResults?.YUOM 
            };

            var trendResults =(await _kpiApi.GetByMetric(portfolioId, viewName, trendRequest.ToDictionary())).FirstOrDefault(c => c.Name.Contains(score));
            var trend = new Trend
            {
                Average = trendResults?.Values?.Average(c => TryParseDouble(c.YValue)) ?? 0,
                Unit = trendResults?.YUOM,
                Difference = result.Average - (trendResults?.Values?.Average(c => TryParseDouble(c.YValue)) ?? 0)
            };
            
            result.Trend=trend;

            // Cache for two hours
            _cache.Set(cacheKey, result, TimeSpan.FromHours(_cacheDuration));

            return result;
        }

        public async Task<List<KPIBuildingComfortDashboardDto>> GetBuildingDashboardData( string scopeId, KPIViewNames viewName, KPIBaseRequest request)
        {
            // Check if it's in the cache
            var requestHash = request != null ? request.GetHashCode().ToString() : "";
            var cacheKey = $"{viewName}-{scopeId}-{requestHash}";

            if (_cache.TryGetValue(cacheKey, out object value))
                return value as List<KPIBuildingComfortDashboardDto>;

            var rawData=await _kpiApi.GetRawData(viewName,scopeId,request);
            switch (viewName)
            {
                case KPIViewNames.BuildingComfortDashboard:
                    return rawData.Select(KPIMapper.MapToComfortBuildingDashboard).ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(viewName), viewName, null);
            }
        }

        public async Task<List<BuildingPerformanceScoresResponse>> GetPerformanceScoresByDate(KPIPerformanceScoresRequest request)
        {

            var rawData = await _kpiApi.GetRawDataForPerformanceScoresByDate(request);

            var kpiData = rawData.Select(KPIMapper.MapToBuildingPerformanceScores).ToList();

            var energyMetricName = "EnergyScore_LastValue";
            var comfortMetricName = "ComfortScore_LastValue";


            // Group by date and calculate the average of the performance scores
            // if one of the metrics is missing for a date, the average will be calculated based on the available metric
            var results = kpiData
                            .Where(x => x.MetricName == energyMetricName || x.MetricName == comfortMetricName)
                            .GroupBy(x => x.Date.Date)
                            .Select(g => new BuildingPerformanceScoresResponse(g.Key, g.Select(y => y.Value).Average()))
                            .ToList();

            return results;
        }

        static DatedValue MapToDatedValue(DataPoint dataPoint)
        {
           return new DatedValue
            {
                Date = dataPoint?.XValue,
                Value = TryParseDouble(dataPoint?.YValue)
            };
         
        }
        static double TryParseDouble(object value)
        {
            if (double.TryParse(value?.ToString(), out double result))
            {
                return result;
            }
            return 0;
        }
        private async Task MapSiteNames(IEnumerable<Willow.KPI.Service.Metric> metrics)
        {
            // Convert siteId in X value to building name
            foreach (var metric in metrics)
            {
                foreach (var datapoint in metric.Values)
                {
                    if (datapoint.XValue != null && Guid.TryParse(datapoint.XValue.ToString(), out Guid siteId))
                    {
                        try
                        {
                            var site = await _siteRepo.Get(siteId);

                            datapoint.XValue = site.Name;
                        }
                        catch (RestException e) when (e.StatusCode == HttpStatusCode.NotFound)
                        {
                            // Do nothing
                        }
                    }
                }
            }
        }
 
        private int CalculateDaysBetweenDates(DateTime startDate, DateTime endDate)
        {
            TimeSpan duration = endDate - startDate;
            return duration.Days;
        }

    }

}
