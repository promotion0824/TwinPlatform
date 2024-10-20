using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Controllers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.LiveDataApi;
using PlatformPortalXL.Services.MarketPlaceApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Insights
{
    public class InsightService : IInsightService
    {
        private readonly IInsightApiService _insightApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IDirectoryApiService _directoryApiService;
        private readonly IFloorsService _floorsService;
        private readonly ILogger<InsightService> _logger;
        private readonly IMarketPlaceApiService _marketPlaceApi;
        private readonly ILiveDataApiService _liveDataApiService;
        private readonly IConfiguration _configuration;
        private readonly IDigitalTwinApiService _digitalTwinApiService;


        public InsightService(IInsightApiService insightApi,
                              IWorkflowApiService workflowApi,
                              IFloorsService floorsService,
                              ILogger<InsightService> logger,
                              IDirectoryApiService directoryApiService,
                              IMarketPlaceApiService marketPlaceApi,
                              ILiveDataApiService liveDataApiService,
                              IConfiguration configuration,
                              IDigitalTwinApiService digitalTwinApiService)
        {
            _insightApi = insightApi;
            _workflowApi = workflowApi;
            _directoryApiService = directoryApiService;
            _floorsService = floorsService;
            _logger = logger;
            _marketPlaceApi = marketPlaceApi;
            _liveDataApiService = liveDataApiService;
            _configuration = configuration;
            _digitalTwinApiService = digitalTwinApiService;
        }

        public async Task<List<InsightOccurrenceDto>> GetInsightOccurrencesAsync(Guid insightId)
        {
            var occurrences = await _insightApi.GetInsightOccurrencesAsync(insightId);
            return InsightOccurrenceDto.MapFromModels(occurrences, insightId);
        }

        public async Task<List<InsightSimpleDto>> GetInsightStatistics(List<InsightSimpleDto> insights)
        {
            if (insights != null && insights.Count != 0)
            {
                var insightStatistics = await _workflowApi.GetInsightStatistics(insights.Select(x => x.Id).ToList());

                foreach (var insight in insights)
                {
                    insight.TicketCount = insightStatistics.FirstOrDefault(x => x.Id == insight.Id)?.TotalCount ?? 0;
                }
            }

            return insights;
        }

        public async Task<List<InsightTicketStatistics>> GetSiteInsightStatistics(List<Guid> siteIds)
        {
            if (siteIds != null && siteIds.Count != 0)
            {
                return await _workflowApi.GetSiteInsightStatistics(siteIds);
            }

            return new List<InsightTicketStatistics>();
        }

        public async Task<List<InsightActivityDto>> GetInsightActivitiesAsync(Guid siteId, Guid insightId)
        {
            var ticketActivities = await _workflowApi.GetInsightTicketActivitiesAsync(insightId);
            var insightActivities = await _insightApi.GetInsightActivitiesAsync(siteId, insightId);

            insightActivities = FilterInsightActivity(insightActivities);

            var activities = InsightActivityDto.MapFromInsightTicketActivities(ticketActivities);
            activities.AddRange(InsightActivityDto.MapFromInsightActivities(insightActivities));

            var userIds = activities.Where(x => x.UserId.HasValue).Select(x => x.UserId.Value).Distinct().ToList();
            var appIds = activities.Where(x => x.SourceId.HasValue).Select(x => x.SourceId.Value).Distinct().ToList();


            if (userIds.Any())
            {
                var userFullNames = await _directoryApiService.GetFullNamesByUserIdsAsync(userIds);
                foreach (var activity in activities.Where(x => x.UserId.HasValue))
                {
                    activity.FullName = userFullNames
                            .Where(x => x.UserId == activity.UserId.Value)
                            .Select(x => $"{x.FirstName} {x.LastName}")
                            .FirstOrDefault();
                }
            }

            return activities.OrderBy(x => x.ActivityDate).ToList();
        }

        public async Task<List<InsightSimpleDto>> MapFloorData(List<InsightSimpleDto> insights)
        {
            try
            {
                var insightWithFloorIds = insights
                                        .Where(x => x.FloorId.HasValue && string.IsNullOrEmpty(x.FloorCode))
                                        .ToList();

                if (insightWithFloorIds.Any())
                {
                    var floorIds = insightWithFloorIds.Select(x => x.FloorId.Value).Distinct().ToList();
                    var floors = await _floorsService.GetFloorsAsync(floorIds);
                    foreach (var insight in insightWithFloorIds)
                    {
                        insight.FloorCode = floors?.FirstOrDefault(x => x.Id == insight.FloorId)?.Code;
                    }

                }

                var insightWithFloorCodes = insights
                                            .Where(x => !string.IsNullOrEmpty(x.FloorCode) && !x.FloorId.HasValue)
                                            .ToList();

                if (insightWithFloorCodes.Any())
                {
                    var floorList = new List<Floor>();
                    var insightSiteIds = insightWithFloorCodes.Select(x => x.SiteId).Distinct().ToList();
                    foreach (var siteId in insightSiteIds)
                    {
                        var floors = await _floorsService.GetFloorsAsync(siteId);
                        floorList.AddRange(floors);
                    }

                    foreach (var insight in insightWithFloorCodes)
                    {
                        insight.FloorId = floorList.FirstOrDefault(x => x.Code == insight.FloorCode)?.Id;
                    }
                }


                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to map insight floors data in MapFloorData {Message} ", ex.Message);
                return insights;
            }

        }
        public async Task<InsightPointsDto> GetInsightPointsAsync(Guid siteId, Guid insightId)
        {
            return await _insightApi.GetInsightPointsAsync(siteId, insightId);
        }

        public async Task<ImpactScoresLiveData> GetLiveDataByExternalId(Guid customerId, string externalId, DateTime start, DateTime end, string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                          !TimeSpan.TryParse(selectedInterval, CultureInfo.InvariantCulture, out var parsedInterval)
               ? (TimeSpan?)null
               : parsedInterval;

            var connectorId = _configuration.GetValue<Guid>("RulesEngineConnectorId");
            if (connectorId == Guid.Empty)
            {
                throw new InvalidCastException("RulesEngineConnectorId is not configured");
            }
            var startUtc = start.ToUniversalTime();
            var endUtc = end.ToUniversalTime();

            var timeseriesData = await _liveDataApiService.GetTimeSeriesAnalogByExternalId(customerId, connectorId, externalId, startUtc, endUtc, interval);

            return new ImpactScoresLiveData
            {
                ExternalId = externalId,
                TimeSeriesData = timeseriesData
            };
        }

        public async Task<List<TwinInsightStatisticsResponseDto>> GetTwinsWithGeometryIdInsightStatisticsAsync(InsightTwinStatisticsRequest request)
        {
            var twins = await _digitalTwinApiService.GetTwinsWithGeometryIdAsync(request);
            if (twins == null || !twins.Any())
                return null;

            var insightStatistics =
                await _insightApi.GetTwinsInsightStatistics(new TwinInsightStatisticsRequest
                {
                    TwinIds = twins.Select(c => c.TwinId).ToList(),
                    IncludeRuleId = request.IncludeRuleId,
                    ExcludeRuleId = request.ExcludeRuleId
                });

            var result = new List<TwinInsightStatisticsResponseDto>();
            foreach (var twin in twins)
            {
                var twinStatistic = new TwinInsightStatisticsResponseDto
                {
                    TwinId = twin.TwinId,
                    GeometryViewerId = twin.GeometryViewerId,
                    UniqueId = twin.UniqueId
                };
                var twinInsightStatistic = insightStatistics.FirstOrDefault(c => c.TwinId == twin.TwinId);
                if (twinInsightStatistic != null)
                {
                    twinStatistic.HighestPriority = twinInsightStatistic.HighestPriority;
                    twinStatistic.InsightCount = twinInsightStatistic.InsightCount;
                    twinStatistic.RuleIds = twinInsightStatistic.RuleIds;
                }
                result.Add(twinStatistic);
            }
            return result;
        }

        /// <summary>
        /// Twin insight statistics
        /// </summary>
        /// <param name="request">List of twin identifiers</param>
        /// <returns>Insight and ticket statistics</returns>
        public async Task<List<TwinInsightStatisticsDto>> GetTwinInsightStatisticsAsync(InsightTwinStatisticsRequest request)
        {
            return (await _insightApi.GetTwinsInsightStatistics(new()
            {
                TwinIds = request.DtIds,
                IncludePriorityCounts = true
            })).DistinctBy(item => item.TwinId).ToList();
        }

        /// <summary>
        /// Only show the activities with New status if:
        ///  it's first time the insight is created OR
        ///  if the insight has been resolved or ignored
        ///  example: New => Resolved => New => Open => New => Ignored => New => New
        ///  should be filtered to New => Resolved => New => Open => Ignored => New
        ///  and keep only the most recent New activity
        /// </summary>
        /// <param name="insightActivities"></param>
        private List<InsightActivity> FilterInsightActivity(List<InsightActivity> insightActivities)
        {
            if (!insightActivities.Any())
            {
                return insightActivities;
            }

            var filteredList = new List<InsightActivity>();
            insightActivities = insightActivities.OrderBy(x => x.StatusLog.CreatedDateTime).ToList();

            var lastNewActivityIndex = 0;

            // if the  New activity duplicated we just want to keep the most recent one
            // because the last New activity in the filtered list is not always the last element in the list
            // we need to keep track of the last New activity index in the filtered list to update it with most recent New activity
            var lastNewActivityIndexInFilteredList = 0;

            var isFirstNewActivity = false;

            for (int i = 0; i < insightActivities.Count; i++)
            {
                if (insightActivities[i].StatusLog.Status == InsightStatus.New)
                {
                    // take subset of the list from the last new activity to the current one
                    var subSet = insightActivities.Skip(lastNewActivityIndex).Take(i - lastNewActivityIndex);
                    // check if any of the activities in the subset is resolved or ignored
                    var isResolvedOrIgnored = subSet.Any(x => x.StatusLog.Status == InsightStatus.Resolved || x.StatusLog.Status == InsightStatus.Ignored);

                    if (isResolvedOrIgnored)
                    {
                        filteredList.Add(insightActivities[i]);
                        lastNewActivityIndexInFilteredList = filteredList.Count - 1;
                    }
                    // check the last element with New status and update it to the most recent New activity
                    else if (filteredList.Count > 0 && filteredList[lastNewActivityIndexInFilteredList].StatusLog.Status == InsightStatus.New)
                    {
                        filteredList[lastNewActivityIndexInFilteredList] = insightActivities[i];
                    }
                    else
                    {
                        // we have to add first New status when the insight is created and it doesn't have to be resolved or ignored before
                        if (!isFirstNewActivity)
                        {
                            filteredList.Add(insightActivities[i]);
                            isFirstNewActivity = true;
                        }
                    }

                    lastNewActivityIndex = i;

                }
                else
                {
                    filteredList.Add(insightActivities[i]);
                }

            }

            return filteredList;
        }
    }
}
