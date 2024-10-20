using InsightCore.Controllers.Requests;
using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Infrastructure.Configuration;
using InsightCore.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Batch;
using Newtonsoft.Json;
using Willow.ExceptionHandling.Exceptions;
using IDateTimeService = Willow.Infrastructure.Services.IDateTimeService;
using Willow.Infrastructure;

namespace InsightCore.Services
{
	public interface IInsightService
    {
        Task<BatchDto<InsightCardDto>> GetInsightCards(BatchRequestDto request);
        Task<BatchDto<InsightDto>> GetInsights(BatchRequestDto request, bool addFloor=false, bool ignoreQueryFilters = false);
        Task<InsightDto> GetInsight(Guid insightId, bool ignoreQueryFilters = false);
        Task<InsightDto> CreateInsight(Guid siteId, CreateInsightRequest request);
        Task<InsightDto> UpdateInsight(Guid siteId, Guid insightId, UpdateInsightRequest request, bool ignoreQueryFilters = false);
        Task<List<InsightOccurrenceDto>> GetInsightOccurrencesAsync(Guid insightId);
		Task<List<StatusLog>> GetInsightStatusLog(Guid insightId, Guid siteId);
        Task<InsightPointsDto> GetPointsAsync(Guid siteId, Guid insightId);
        Task AddMissingTwinDetailsToInsightsAsync(int batchSize, CancellationToken stoppingToken);
        Task BatchUpdateInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request);
        Task<List<InsightActivity>> GetInsightActivities(Guid siteId, Guid insightId);
        Task<List<InsightMapViewDto>> GetInsightListForMapViewAsync(List<Guid> siteIds);
        Task<List<InsightDiagnosticDto>> GetInsightDiagnosticAsync( Guid insightId, DateTime start, DateTime end,double interval);
        Task<DiagnosticsSnapshotDto> GetDiagnosticsSnapshot(Guid insightId);
        Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request, bool ignoreQueryFilters = false);
        Task<InsightFilterDto> GetInsightFiltersAsync(GetInsightFilterRequest request);
        Task<InsightDto> UpdateInsightFromAppAsync(Guid siteId, Guid insightId, UpdateInsightRequest request);
        Task<List<InsightSourceDto>> GetInsightSources();
        string GetSourceName(SourceType? sourceType, Guid? sourceId);
    }

    public class InsightService : IInsightService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IInsightRepository _repository;
        private readonly IAnalyticsService _analyticsService;
        private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;
        private readonly IWorkflowServiceApi _workflowServiceApi;
		private readonly ILogger<InsightService> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly IMemoryCache _memoryCache;

        public InsightService(IDateTimeService dateTimeService, IInsightRepository repository, IAnalyticsService analyticsService, IDigitalTwinServiceApi digitalTwinServiceApi, IWorkflowServiceApi workflowServiceApi, ILogger<InsightService> logger, IOptions<AppSettings> options, IMemoryCache memoryCache)
        {
            _dateTimeService = dateTimeService;
            _repository = repository;
            _analyticsService = analyticsService;
			_digitalTwinServiceApi=digitalTwinServiceApi;
			_workflowServiceApi = workflowServiceApi;
			_logger = logger;
            _options = options;
            _memoryCache = memoryCache;
        }

        public async Task<InsightFilterDto> GetInsightFiltersAsync(GetInsightFilterRequest request)
        {
            var insightFilters = new List<InsightFilter>();
            var siteActivities = new List<SiteInsightTicketStatisticsDto>();

            var insightFilterTask = _repository.GetInsightFiltersAsync(request.SiteIds,request.StatusList);
            var siteActivityTask =  _workflowServiceApi.GetSiteInsightStatistics(request.SiteIds);

            var filtersTask = Task.WhenAll(insightFilterTask, siteActivityTask);
           
            try
            {
                await filtersTask;
            }
            catch (Exception ex)
            {
                if(filtersTask.Exception is not null)
                {
                    _logger.LogError(filtersTask.Exception, "Insight filters task error");
                }
                else
                {
                    _logger.LogError(ex, "Insight filters error");
                }
                
            }
            if (insightFilterTask.IsCompletedSuccessfully)
            {
                insightFilters = insightFilterTask.Result;
            }
            if (siteActivityTask.IsCompletedSuccessfully)
            {
                siteActivities = siteActivityTask.Result;
            }

            return new InsightFilterDto
            {
                Filters = new Dictionary<string, List<string>>
                {
                    {
                        InsightFilterNames.InsightTypes,
                        insightFilters.Select(x => x.Type.ToString()).Distinct().ToList() ?? []
                    },
                    { InsightFilterNames.SourceNames, GetSourceData(insightFilters).ToList() },
                    { InsightFilterNames.Activity, GetActivityFilter(insightFilters, siteActivities) },
                    {
                        InsightFilterNames.PrimaryModelIds,
                        insightFilters.Select(x => x.PrimaryModelId).Distinct().ToList() ?? []
                    },
                    {
                        InsightFilterNames.DetailedStatus,
                        insightFilters.Select(x => x.Status.ToString()).Distinct().ToList() ?? []
                    }
                }
            };

        }

        public async Task<List<InsightSourceDto>> GetInsightSources()
        {
            var insightSources=await _repository.GetInsightSources();
            return insightSources.Select(c => InsightSourceDto.MapFromModel(c, GetSourceName(c.SourceType,c.SourceId))).ToList();
        }

        public async Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request, bool ignoreQueryFilters = false)
        {
            return await _repository.GetImpactScoresSummary(request, ignoreQueryFilters);
        }

        public async Task<BatchDto<InsightCardDto>> GetInsightCards(BatchRequestDto request)
        {
            var insightCards = await _repository.GetInsightCards(request);
            return new BatchDto<InsightCardDto>()
            {
                After = insightCards.After,
                Before = insightCards.Before,
                Items = insightCards.Items.Select(c => InsightCardDto.MapFrom(c, GetSourceName( SourceType.App, c.SourceId))).ToArray(),
                Total = insightCards.Total
            };
        }

        public async Task<BatchDto<InsightDto>> GetInsights(BatchRequestDto request, bool addFloor=false, bool ignoreQueryFilters = false)
        {
            var batchInsights = await _repository.GetInsights(request, ignoreQueryFilters);

            if(addFloor)
                batchInsights.Items = await MapFloorId(batchInsights.Items);

            return new BatchDto<InsightDto>
            {
                After = batchInsights.After,
                Before = batchInsights.Before,
                Items = batchInsights.Items.Select(c => InsightDto.MapFrom(c, GetSourceName(c.SourceType,c.SourceId))).ToArray(),
                Total = batchInsights.Total
            };
        }

        public async Task<List<InsightMapViewDto>> GetInsightListForMapViewAsync(List<Guid> siteIds)
        {
            var ruleIds = new List<string>
            {
                "plane-docked-at-gate-",
                "plane-not-connected-to-ground-power-unit-"
            };
            var insights= await _repository.GetSiteInsightsWithOccurrences(siteIds,ruleIds: ruleIds);
            return insights.Select(c => InsightMapViewDto.MapFrom(c, GetSourceName(c.SourceType,c.SourceId))).ToList();
        }

        public async Task<List<InsightOccurrenceDto>> GetInsightOccurrencesAsync(Guid insightId)
        {
			var insightOccurrences= await _repository.GetInsightOccurrencesAsync(insightId);
			return InsightOccurrenceDto.MapFrom(insightOccurrences);
        }

        public async Task<InsightDto> GetInsight(Guid insightId, bool ignoreQueryFilters = false)
        {
            var insight = await _repository.GetInsight(insightId, ignoreQueryFilters);
            if (insight == null || insight.Status == InsightStatus.Deleted)
            {
                throw new NotFoundException($"insight: {insightId}");
            }

            insight= await LookUpTwinNameById(insight);
            return InsightDto.MapFrom(insight, GetSourceName(insight.SourceType,insight.SourceId));
        }
		/// <summary>
		/// create insight
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="request"></param>
		/// <returns></returns>
        public async Task<InsightDto> CreateInsight(Guid siteId, CreateInsightRequest request)
        {
            var existingInsights = await _repository.GetActiveUniqueInsight(siteId, request.TwinId, request.Name);
            if (existingInsights.Any())
            {
                var insightEntity = request.SourceType != SourceType.Inspection ?
                                        existingInsights.First() :
                                        existingInsights.FirstOrDefault(i =>
                                                            i.Description.Equals(request.Description,
                                                                                StringComparison.InvariantCultureIgnoreCase));

                if (insightEntity != null)
                {
                    insightEntity.OccurrenceCount = request.OccurrenceCount;
					insightEntity.LastOccurredDate = request.OccurredDate;

					await _repository.UpdateInsight(insightEntity, statusLog: null);
                    return InsightDto.MapFrom(insightEntity, GetSourceName(insightEntity.SourceType, insightEntity.SourceId));
                }
            }
			if (!request.CreatedUserId.HasValue && !request.SourceId.HasValue)
			{
				throw new BadRequestException($"The insight's createdUserId and sourceId are null, at least one of them must have values. SiteId:{siteId}, InsightName:{request.Name}");
			}
            var inspectionRuleOption= (request.SourceType == SourceType.Inspection && request.Type == InsightType.Alert) ? _options?.Value.InspectionOptions : null;
            var insight = new Insight
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                SiteId = siteId,
                TwinId =request.TwinId,
                Type = request.Type,
                Name = request.Name,
                Description = request.Description,
                Recommendation = request.Recommendation,
                ImpactScores = request.ImpactScores,
                Priority = request.Priority,
                Status = InsightStatus.New,
                State = request.State,
                LastOccurredDate = request.OccurredDate,
                DetectedDate = request.DetectedDate,
                CreatedDate = _dateTimeService.UtcNow,
                UpdatedDate = _dateTimeService.UtcNow,
                SourceType = request.SourceType,
                SourceId = request.SourceId,
                ExternalId = request.ExternalId,
                ExternalStatus = request.ExternalStatus,
                ExternalMetadata = request.ExternalMetadata,
                OccurrenceCount = request.OccurrenceCount,
				CreatedUserId= request.CreatedUserId,
				RuleId= inspectionRuleOption?.RuleId?? request.RuleId,
				RuleName = inspectionRuleOption?.RuleName ?? request.RuleName,
                PrimaryModelId = request.PrimaryModelId,
				InsightOccurrences = request.InsightOccurrences,
				SequenceNumber = await _repository.GenerateSequenceNumber(request.SequenceNumberPrefix),
				Dependencies = request.Dependencies,
                Points = request.Points,
                Locations = request.Locations,
                Tags = request.Tags
            };
            insight = await LookUpTwinNameById(insight);
          
            var createdInsight = await _repository.CreateInsight(insight);

            _analyticsService.TrackInsightCreation(request.Type, 
                                                    request.Name, 
                                                    request.Description, 
                                                    request.Priority, 
                                                    request.OccurredDate,
                                                    request.AnalyticsProperties);
            
            return InsightDto.MapFrom(createdInsight, GetSourceName(createdInsight.SourceType, createdInsight.SourceId)); 
        }

        public async Task BatchUpdateInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request)
        {
            if (!request.UpdatedByUserId.HasValue && !request.SourceId.HasValue)
            {
                throw new BadRequestException($"The insight's updatedByUserId and appId are null, at least one of them must have value. SiteId:{siteId}, InsightIds:{string.Join(',',request.Ids)}");
            }

            var filters = new FilterSpecificationDto[]
            {
                new ()
                {
                    Field = nameof(InsightEntity.Id),
                    Operator = FilterOperators.ContainedIn,
                    Value = request.Ids
                }
            };

            if (request.Status != InsightStatus.Ignored)
            {
                filters = filters.Upsert(nameof(InsightEntity.Status), FilterOperators.NotEquals, request.Status);
            }

            var insights = (await _repository.GetInsights(new BatchRequestDto { FilterSpecifications = filters }))?.Items?.ToList();

            if (insights != null  && insights.Any())
            {
                var statusLogs = new List<StatusLog>();

                foreach (var insight in insights)
                {
                    var statusLog = await ValidateStatusChangeRequestAsync(insight,
                        new InsightStatusChangeDto()
                        {
                            Reason = request.Reason,
                            SourceId = request.SourceId,
                            Status = request.Status,
                            UserId = request.UpdatedByUserId
                        });

                    insight.Status = statusLog != null ? statusLog.Status : request.Status.Value;

                    statusLogs.Add(statusLog);
                }

                await _repository.UpdateInsightsAsync(insights, statusLogs.Where(x => x != null).ToArray());
            }
        }

        /// <summary>
        /// this method is used to update the insight from the UI
        /// it need to be optimized to include only data required to be updated
        /// and return data required by UI
        /// work item for this work https://dev.azure.com/willowdev/Unified/_workitems/edit/127504
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="insightId"></param>
        /// <param name="request"></param>
        /// <param name="ignoreQueryFilters"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        public async Task<InsightDto> UpdateInsight(Guid siteId, Guid insightId, UpdateInsightRequest request, bool ignoreQueryFilters = false)
        {
	        if (!request.UpdatedByUserId.HasValue && !request.SourceId.HasValue)
	        {
		        throw new BadRequestException($"The insight's updatedByUserId and appId are null, at least one of them must have value. SiteId:{siteId}, InsightId:{insightId}");
	        }

            var insight = await _repository.GetInsight(insightId, ignoreQueryFilters /* Rules Engine does not want to see 404 for deleted insights */);
            if (insight == null)
            {
                throw new NotFoundException($"insight: {insightId}");
            }

            if (insight.Status == InsightStatus.Deleted)
            {
	            return null;
            }

			StatusLog statusLog = null;
			var requestedStatus = request.LastStatus ?? request.Status?.Convert();
            if (requestedStatus != null)
			{
				statusLog = await ValidateStatusChangeRequestAsync(insight, new InsightStatusChangeDto()
                {
                    Reason = request.Reason,
                    SourceId = request.SourceId,
                    Status = requestedStatus,
                    UserId = request.UpdatedByUserId
                });

                insight.Status = statusLog != null ? statusLog.Status : requestedStatus.Value;
			}

			if (!string.IsNullOrEmpty(request.Name))
            {
                insight.Name = request.Name;
            }
            if (request.Description != null)
            {
                insight.Description = request.Description;
            }
            if (request.Recommendation != null)
            {
                insight.Recommendation = request.Recommendation;
            }
            if (request.ImpactScores != null)
            {
                insight.ImpactScores = request.ImpactScores;
            }
            if (request.Priority != null)
            {
                insight.Priority = request.Priority.Value;
            }
            if (request.Type != null)
            {
                insight.Type = request.Type.Value;
            }
            if (request.State != null)
            {
                if (request.State == InsightState.Active && insight.State == InsightState.Archived)
                {
                    insight.OccurrenceCount++;
                    insight.LastOccurredDate = DateTime.UtcNow;
                }

                insight.State = request.State.Value;
            }
            if (request.InsightOccurrences != null)
            {
                insight.InsightOccurrences = request.InsightOccurrences;
            }
            if (request.OccurredDate != null)
            {
	            if (request.OccurredDate.Value > insight.LastOccurredDate)
	            {
                    insight.OccurrenceCount++;
                    insight.NewOccurrence = true;
                    insight.LastOccurredDate = request.OccurredDate.Value;
                }
            }
            if (request.DetectedDate != null)
            {
                insight.DetectedDate = request.DetectedDate.Value;
            }
            if (request.ExternalId != null)
            {
                insight.ExternalId = request.ExternalId;
            }
            if (request.ExternalStatus != null)
            {
                insight.ExternalStatus = request.ExternalStatus;
            }
            if (request.ExternalMetadata != null)
            {
                insight.ExternalMetadata = request.ExternalMetadata;
            }
			if (request.OccurrenceCount != 0)
            {
                insight.OccurrenceCount = request.OccurrenceCount;
            }
			if (request.PrimaryModelId != null)
			{
				insight.PrimaryModelId = request.PrimaryModelId;
			}
			
			if (!string.IsNullOrEmpty(request.RuleName))
			{
				insight.RuleName = request.RuleName;
			}
			if(request.Dependencies is not null)
			{
				insight.Dependencies = request.Dependencies;
			}
            if (request.Points is not null)
            {
                insight.Points=request.Points;
            }
            if (request.Locations is not null)
            {
                insight.Locations = request.Locations;
            }
            if (request.Tags is not null)
            {
                insight.Tags = request.Tags;
            }
            if (request.Reported.HasValue)
            {
                insight.Reported = request.Reported.Value;
            }
			insight.UpdatedDate = _dateTimeService.UtcNow;
            await _repository.UpdateInsight(insight, statusLog, request.SourceId,request.UpdatedByUserId);
            var updatedInsight= await _repository.GetInsight(insightId, ignoreQueryFilters);
            // updatedInsight is null when its status is deleted
            return updatedInsight!=null? InsightDto.MapFrom(updatedInsight, GetSourceName(updatedInsight.SourceType, updatedInsight.SourceId)):null; 

        }
        public async Task<List<StatusLog>> GetInsightStatusLog(Guid insightId, Guid siteId)
		{
			return await _repository.GetInsightStatusLog(insightId, siteId);
		}

        /// <summary>
        /// Get all point for an insight from insight points and impact scores
        /// point id = trend id in adt
        /// </summary>
        /// <param name="insightId"></param>
        /// <returns></returns>
        public async Task<InsightPointsDto> GetPointsAsync(Guid siteId, Guid insightId)
        {
            var insightPointsDto = new InsightPointsDto();
            var insightPoints = await _repository.GetInsightPointsAsync(insightId);
            if (!string.IsNullOrEmpty(insightPoints.PointsJson))
            {
                var points = JsonConvert.DeserializeObject<List<Point>>(insightPoints.PointsJson);
                if (points?.Any() ?? false)
                {
                    var pointTwinDto = await _digitalTwinServiceApi.GetPointsByTwinIdsAsync(siteId, points.Select(c => c.TwinId).ToList());
                    if (pointTwinDto?.Any() ?? false)
                    {
                        insightPointsDto.InsightPoints = pointTwinDto;
                    }
                }
            }
            var impactScoresPoints = insightPoints.ImpactScores?.Where(x => !string.IsNullOrEmpty(x.ExternalId));
            if (impactScoresPoints.Any())
            {
                var impactScorePoints  = impactScoresPoints.Select(c => new ImpactScorePointDto
                {
                    Name = c.Name,
                    ExternalId = c.ExternalId,
                    Unit = c.Unit
                }).ToList();

                insightPointsDto.ImpactScorePoints = impactScorePoints;
            }
          
            return insightPointsDto;
        }

        public async Task AddMissingTwinDetailsToInsightsAsync(int batchSize, CancellationToken stoppingToken)
        {
            var pageNumber = 1;
            var insights = await _repository.GetPagedInsightsWithMissingTwinDetailAsync(pageNumber, batchSize);
            while (insights != null && insights.Any())
            {
                try
                {

                    var siteTwins = await LookUpTwinNameById(insights);
                    if (siteTwins != null && siteTwins.Any())
                    {
                        foreach (var insight in insights)
                        {
                            var twin = siteTwins.FirstOrDefault(c =>
                                c.Id.Equals(insight.TwinId, StringComparison.InvariantCultureIgnoreCase));
                            insight.PrimaryModelId =twin?.ModelId;
                             insight.TwinName=twin?.Name;
                        }
                    }
                    await _repository.UpdateInsightsAsync(insights);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to get twinId  in AddTwinIdToInspectionsAsync {Message}", ex.Message);
                }
                pageNumber += 1;
                insights = await _repository.GetPagedInsightsWithMissingTwinDetailAsync(pageNumber, batchSize);

            }
        }

        public async Task<List<InsightActivity>> GetInsightActivities(Guid siteId, Guid insightId)
        {
            var data = await _repository.GetInsightActivities(siteId, insightId);
            if(data is null)
            {
                return null;
            }

            var insightActivities = InsightActivity.MapFrom(data);
            foreach (var activity in insightActivities)
            {
                if (activity.StatusLog?.Status == InsightStatus.New)
                {
                    var occurrence = data.Occurrences.Where(x => x.IsFaulted && x.Started < activity?.StatusLog.CreatedDateTime).MaxBy(x => x.Started);
                    activity.InsightOccurrence = InsightOccurrenceEntity.MapTo(occurrence);
                }
            }
            return insightActivities;
        }

        public async Task<List<InsightDiagnosticDto>> GetInsightDiagnosticAsync(Guid insightId, DateTime start, DateTime end,double interval)
        {
            var insight = await GetInsightWithException(insightId);
            end= ValidateEndDateForTimeSeries(start,end,interval);
            var result = new List<InsightDiagnosticDto>();

            if (!HasDependencies(insight))
                return result;

            var dependentInsights = await _repository.GetSiteInsightsWithOccurrences(insightIds: insight.Dependencies.Select(c => c.InsightId).ToList());
            var filteredOccurrences = FilterOccurrences(insight.InsightOccurrences, start, end);
            result.AddRange(InsightDiagnosticDto.DiagnosticInsightDtoMapper(dependentInsights, insight, filteredOccurrences,start,end,interval));

            foreach (var dependent in dependentInsights)
            {
                if (!HasDependencies(dependent))
                    continue;

                var dependentDependentInsights = await _repository.GetSiteInsightsWithOccurrences(insightIds: dependent.Dependencies.Select(c => c.InsightId).ToList());
                filteredOccurrences = FilterOccurrences(dependent.InsightOccurrences, start, end);
                result.AddRange(InsightDiagnosticDto.DiagnosticInsightDtoMapper(dependentDependentInsights, dependent, filteredOccurrences,start, end,interval));
            }

            return result;
        }

        public async Task<DiagnosticsSnapshotDto> GetDiagnosticsSnapshot(Guid insightId)
        {
            var insight = await _repository.GetInsightAlias(insightId);

            var diagnostics = new DiagnosticsSnapshotDto()
            {
                Id = insightId,
                Name = insight.Name,
                RuleName = insight.RuleName,
                Check = true,
                Diagnostics = []
            };

            var lastFaultyOccurrence = await _repository.GetLastFaultyOccurrence(insightId);
            if (lastFaultyOccurrence != null)
            {
                diagnostics.Started = lastFaultyOccurrence.Started;
                diagnostics.Ended = lastFaultyOccurrence.Ended;
                diagnostics.Check = !lastFaultyOccurrence.IsFaulted;
                diagnostics.Diagnostics = await GetChildrenDiagnostics(diagnostics.Id, diagnostics.Started, diagnostics.Ended);
            }

            return diagnostics;
        }
        /// <summary>
        /// Update insight from an app
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="insightId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="ResourceNotFoundException"></exception>
        public async Task<InsightDto> UpdateInsightFromAppAsync(Guid siteId, Guid insightId, UpdateInsightRequest request)
        {
            if (!request.UpdatedByUserId.HasValue && !request.SourceId.HasValue)
            {
                throw new BadRequestException($"The insight's updatedByUserId and appId are null, at least one of them must have value. SiteId:{siteId}, InsightId:{insightId}");
            }

            var insight = await _repository.GetSimpleInsightAsync(insightId, true /* Rules Engine does not want to see 404 for deleted insights */);
            if (insight == null)
            {
                throw new NotFoundException($"insight: {insightId}");
            }

            if (insight.Status == InsightStatus.Deleted)
            {
                return null;
            }

            StatusLog statusLog = null;
            var requestedStatus = request.LastStatus ?? request.Status?.Convert();
            if (requestedStatus != null)
            {
                statusLog = await ValidateStatusChangeRequestAsync(insight, new InsightStatusChangeDto()
                {
                    Reason = request.Reason,
                    SourceId = request.SourceId,
                    Status = requestedStatus,
                    UserId = request.UpdatedByUserId
                });

                insight.Status = statusLog != null ? statusLog.Status : requestedStatus.Value;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                insight.Name = request.Name;
            }
            if (request.Description != null)
            {
                insight.Description = request.Description;
            }
            if (request.Recommendation != null)
            {
                insight.Recommendation = request.Recommendation;
            }
            if (request.ImpactScores != null)
            {
                insight.ImpactScores = request.ImpactScores;
            }
            if (request.Priority != null)
            {
                insight.Priority = request.Priority.Value;
            }
            if (request.Type != null)
            {
                insight.Type = request.Type.Value;
            }
            if (request.State != null)
            {
                if (request.State == InsightState.Active && insight.State == InsightState.Archived)
                {
                    insight.OccurrenceCount++;
                    insight.LastOccurredDate = DateTime.UtcNow;
                }

                insight.State = request.State.Value;
            }
            if (request.InsightOccurrences != null)
            {
                insight.InsightOccurrences = request.InsightOccurrences;
            }
            if (request.OccurredDate != null)
            {
                if (request.OccurredDate.Value > insight.LastOccurredDate)
                {
                    insight.OccurrenceCount++;
                    insight.NewOccurrence = true;
                    insight.LastOccurredDate = request.OccurredDate.Value;
                }
            }
            if (request.DetectedDate != null)
            {
                insight.DetectedDate = request.DetectedDate.Value;
            }
            if (request.ExternalId != null)
            {
                insight.ExternalId = request.ExternalId;
            }
            if (request.ExternalStatus != null)
            {
                insight.ExternalStatus = request.ExternalStatus;
            }
            if (request.ExternalMetadata != null)
            {
                insight.ExternalMetadata = request.ExternalMetadata;
            }
            if (request.OccurrenceCount != 0)
            {
                insight.OccurrenceCount = request.OccurrenceCount;
            }
            if (request.PrimaryModelId != null)
            {
                insight.PrimaryModelId = request.PrimaryModelId;
            }

            if (!string.IsNullOrEmpty(request.RuleName))
            {
                insight.RuleName = request.RuleName;
            }
            if (request.Dependencies is not null)
            {
                insight.Dependencies = request.Dependencies;
            }
            if (request.Points is not null)
            {
                insight.Points = request.Points;
            }
            if (request.Reported.HasValue)
            {
                insight.Reported = request.Reported.Value;
            }
            if (request.Locations is not null)
            {
                insight.Locations = request.Locations;
            }
            insight.UpdatedDate = _dateTimeService.UtcNow;
            await _repository.UpdateInsight(insight, statusLog, request.SourceId, request.UpdatedByUserId);
           
            var updatedInsight= await _repository.GetSimpleInsightAsync(insightId, true);
            return InsightDto.MapFrom(updatedInsight,
                GetSourceName(updatedInsight.SourceType, updatedInsight.SourceId));
        }

        public string GetSourceName(SourceType? sourceType, Guid? sourceId)
        {
            if (sourceType != SourceType.App || !sourceId.HasValue)
                return $"{sourceType}";

            var sourceName = _options?.Value.WillowActivateOptions?.AppName ?? string.Empty;

            if (string.IsNullOrEmpty(_options?.Value.MappedIntegrationConfiguration?.AppId) ||
                !Guid.TryParse(_options.Value.MappedIntegrationConfiguration.AppId, out var mappedAppId))
                return sourceName;

            return sourceId.Value == mappedAppId ? _options.Value.MappedIntegrationConfiguration.AppName : sourceName;
        }

        #region Private Methods

        private DateTime ValidateEndDateForTimeSeries(DateTime start, DateTime end, double interval)
        {
            // If end is before start, return the original end date
            if (end < start)
                return end;

            // Calculate the total minutes between start and end
            var totalMinutes = (end - start).TotalMinutes;

            if (totalMinutes < interval)
                return start.AddMinutes(interval);

            return totalMinutes % interval == 0 ? end : end.AddMinutes(totalMinutes % interval);
        }
        private async Task<List<DiagnosticsSnapshotDto>> GetChildrenDiagnostics(Guid insightId, DateTime started, DateTime? ended)
        {
            var childrenDiagnostics = await _repository.GetChildrenDiagnostics(insightId, started, ended);

            foreach (var child in childrenDiagnostics)
            {
                child.Diagnostics = await GetChildrenDiagnostics(child.Id, started, ended);
            }

            return childrenDiagnostics;
        }
        private async Task<Insight[]> MapFloorId(Insight[] insights)
        {
            var insightTwins = await LookUpTwinNameById(insights);
            if (insightTwins == null || !insightTwins.Any())
            {
                return insights;
            }

            foreach (var insight in insights.Where(c => !string.IsNullOrEmpty(c.TwinId)))
            {
                var insightTwinData = insightTwins
                    .FirstOrDefault(c => c.SiteId == insight.SiteId && c.Id == insight.TwinId);
                insight.FloorId = insightTwinData?.FloorId;
            }
            return insights;
        }
        private async Task<Insight> GetInsightWithException(Guid insightId)
        {
            var insight = (await _repository.GetSiteInsightsWithOccurrences(insightIds: [insightId])).FirstOrDefault();
            if (insight is null)
            {
                throw new NotFoundException($"insight: {insightId}");
            }
            return insight;
        }

        private static bool HasDependencies(Insight insight)
        {
            return insight.Dependencies != null && insight.Dependencies.Any();
        }

        private static List<InsightOccurrence> FilterOccurrences(IEnumerable<InsightOccurrence> occurrences, DateTime start, DateTime end)
        {
            return occurrences?.Where(c => c.Started < end && c.Ended > start).ToList();
        }

        private async Task ValidateStatusChangeAsync(Guid insightId, InsightStatus currentStatus, InsightStatus requestedStatus)
        {
            if (currentStatus != requestedStatus)
			{
				var errorMessage = $"The insight is {currentStatus} and couldn't change to {requestedStatus}";

				switch (currentStatus)
				{
					case InsightStatus.New:
						{
							if (requestedStatus == InsightStatus.Resolved)
							{
								throw new BadRequestException(errorMessage);
							}
							break;
						}
					case InsightStatus.Open:
						{
							if (requestedStatus == InsightStatus.Resolved)
							{
								throw new BadRequestException(errorMessage);
							}

							break;
						}
					case InsightStatus.InProgress:
						{
							if (requestedStatus != InsightStatus.Resolved)
							{
								throw new BadRequestException(errorMessage);
							}

							var hasOpenTickets = await _workflowServiceApi.GetInsightNumberOfOpenTicketsAsync(insightId);
							if (hasOpenTickets)
							{
                                throw new BadRequestException($"{errorMessage}, because it has open tickets.");
							}

							break;
						}
					case InsightStatus.Resolved:
						{
							if (requestedStatus != InsightStatus.New)
							{
								throw new BadRequestException(errorMessage);
							}

							break;
						}
					case InsightStatus.Ignored:
						{
							if (requestedStatus != InsightStatus.New && requestedStatus != InsightStatus.Deleted)
							{
								throw new BadRequestException(errorMessage);
							}

							break;
						}
					case InsightStatus.Deleted:
						{
							throw new BadRequestException(errorMessage);
						}
				}
			}
        }

		private async Task<StatusLog> ValidateStatusChangeRequestAsync(Insight insight, InsightStatusChangeDto dto)
		{
			if (dto.SourceId == null && dto.UserId == null)
			{
				throw new BadRequestException($"Insight status changes requires SourceId or UserId.");
			}

			if (insight.Status != dto.Status)
			{
                // Bypass the workflow validation for Willow Activate (RulesEngine)
                if (!dto.SourceId.HasValue
                    || string.IsNullOrEmpty(_options?.Value.WillowActivateOptions?.AppId)
                    || dto.SourceId.Value != Guid.Parse(_options.Value.WillowActivateOptions.AppId))
                {
                    await ValidateStatusChangeAsync(insight.Id, insight.Status, dto.Status.Value);
                }
                else
                {
                    if (insight.Status == InsightStatus.InProgress && dto.Status.Value == InsightStatus.Resolved)
                    {
                        var hasOpenTickets = await _workflowServiceApi.GetInsightNumberOfOpenTicketsAsync(insight.Id);
                        if (hasOpenTickets)
                        {
                            dto.Status = InsightStatus.ReadyToResolve;
                        }
                    }
                }
				    
                return StatusLog.MapFrom(insight, dto);
            }
            // Ignored insights can be unignored by re-requesting a status change to Ignored and certain conditions are met
            // https://dev.azure.com/willowdev/Unified/_workitems/edit/133164
            else if (dto.Status == InsightStatus.Ignored && insight.Status == InsightStatus.Ignored) 
            {
                var lastOccurrence = await _repository.GetLastOccurrence(insight.Id);

                if (lastOccurrence != null)
                {
                    dto.Status = InsightStatus.Resolved;
                    var action = "resolved";

                    if (lastOccurrence.IsValid && lastOccurrence.IsFaulted)
                    {
                        dto.Status = InsightStatus.New;
                        action = "reactivated";
                    }

                    dto.Reason = $"Willow Admin {action} this insight to bring it out of the Ignored state.";

                    return StatusLog.MapFrom(insight, dto);
                }
            }

			return null;
		}

        private async Task<List<TwinSimpleDto>> LookUpTwinNameById(Insight[] insights)
        {
            if (insights == null || insights.All(c => string.IsNullOrEmpty(c.TwinId)))
            {
                return null;
            }
            List<TwinSimpleDto> twinSimpleDtos = null;
            try
            {
                List<KeyValuePair<string, Guid>> notCachedTwins = null;
                var twinIds = insights.Where(c => !string.IsNullOrEmpty(c.TwinId)).Select(c =>new KeyValuePair<string,Guid>(c.TwinId,c.SiteId)).Distinct();
                foreach (var twin in twinIds)
                {
                    if (!_memoryCache.TryGetValue($"Twin_{twin.Key}_{twin.Value}", out TwinSimpleDto cachedTwin))
                    {
                        notCachedTwins??=new List<KeyValuePair<string, Guid>>();
                        notCachedTwins.Add(twin);
                        continue;
                    }
                    twinSimpleDtos ??= new List<TwinSimpleDto>();
                    twinSimpleDtos.Add(cachedTwin);
                }
                if(notCachedTwins != null )
                {
                    var request = notCachedTwins.GroupBy(c => c.Value).Select(c =>
                    new SiteTwinIdsRequestDto
                    {
                        SiteId = c.Key,
                        TwinIds = c.Select(t => t.Key).Distinct().ToList()
                    }).ToList();
                    var fetchedTwinDtos = await _digitalTwinServiceApi.GetTwinsByIdsAsync(request);
                    if (fetchedTwinDtos != null && fetchedTwinDtos.Any())
                    {
                        twinSimpleDtos ??= new List<TwinSimpleDto>();
                        twinSimpleDtos.AddRange(fetchedTwinDtos);
                        foreach (var twin in fetchedTwinDtos)
                        {
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
                            _memoryCache.Set($"Twin_{twin.Id}_{twin.SiteId}", twin, cacheEntryOptions);
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to process LookUpTwinNameById {Message}", ex.Message);
               
            }

            return twinSimpleDtos;
        }
        private async Task<Insight> LookUpTwinNameById(Insight insight)
        {
            var insightTwin = (await LookUpTwinNameById(new[] { insight }))?.FirstOrDefault(c=>c.Id==insight.TwinId && c.SiteId==insight.SiteId);
           
            insight.FloorId = insightTwin?.FloorId;
            insight.TwinName = insightTwin?.Name;
            insight.EquipmentId=insightTwin?.UniqueId;
            insight.PrimaryModelId = string.IsNullOrWhiteSpace(insight.PrimaryModelId)
                ? insightTwin?.ModelId
                : insight.PrimaryModelId;
            return insight;
        }
        private IEnumerable<string> GetSourceData(IEnumerable<InsightFilter> insightFilters)
        {
            return insightFilters.Select(x => new { SourceName = GetSourceName(x.SourceType,x.SourceId), x.SourceId }).DistinctBy(x => x.SourceId).ToList().Select(sourceObj => JsonSerializerExtensions.Serialize(new { sourceObj.SourceName, sourceObj.SourceId }));
        }

        private static List<string> GetActivityFilter(List<InsightFilter> insightFilters, List<SiteInsightTicketStatisticsDto> siteStatistics)
        {
            var activityFilter = new List<string>();

            if (siteStatistics != null && siteStatistics.Any(x => x.TotalCount > 0)) activityFilter.Add(InsightActivityType.Tickets.ToString());
            if (insightFilters.Any(x => x.StatusLogs.Any(c => c == InsightStatus.Resolved))) activityFilter.Add(InsightActivityType.PreviouslyResolved.ToString());
            if (insightFilters.Any(x => x.StatusLogs.Any(c => c == InsightStatus.Ignored))) activityFilter.Add(InsightActivityType.PreviouslyIgnored.ToString());
            if (insightFilters.Any(x => x.Reported)) activityFilter.Add(InsightActivityType.Reported.ToString());

            return activityFilter;
        }
       
        #endregion Private Methods
    }
}
