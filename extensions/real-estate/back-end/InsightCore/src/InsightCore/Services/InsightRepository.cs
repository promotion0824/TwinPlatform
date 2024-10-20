using InsightCore.Dto;
using InsightCore.Entities;
using InsightCore.Infrastructure.Configuration;
using InsightCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;
using Willow.ExceptionHandling.Exceptions;
using Willow.Notifications.Enums;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;


namespace InsightCore.Services
{
    public interface IInsightRepository
    {
        Task<BatchDto<InsightCard>> GetInsightCards(BatchRequestDto request);
        Task<BatchDto<Insight>> GetInsights(BatchRequestDto request, bool ignoreQueryFilters = false);
        Task<Insight> GetInsight(Guid insightId, bool ignoreQueryFilters = false);
        Task<InsightAlias> GetInsightAlias(Guid insightId);
        Task<Insight> CreateInsight(Insight insight);
        Task UpdateInsight(Insight insight, StatusLog statusLog, Guid? sourceId=null,Guid? updatedByUserId=null);
        Task<string> GenerateSequenceNumber(string sequenceNumberPrefix);
        Task<List<Insight>> GetActiveUniqueInsight(Guid siteId, string twinId, string name);
        Task<List<InsightSource>> GetInsightSources();
        Task<List<InsightStatisticsByPriority>> GetInsightStatisticsByPriorityList(IList<Guid> siteIds);
        Task<List<InsightStatisticsByStatus>> GetInsightStatisticsByStatusList(IList<Guid> siteIds);
        Task<List<InsightSnackbarByStatus>> GetInsightSnackbarByStatus(IEnumerable<FilterSpecificationDto> filters);
        Task<List<InsightOccurrence>> GetInsightOccurrencesAsync(Guid insightId);
        Task<InsightOccurrence> GetLastFaultyOccurrence(Guid insightId);
        Task<InsightOccurrence> GetLastOccurrence(Guid insightId);
        Task<List<StatusLog>> GetInsightStatusLog(Guid insightId, Guid siteId);
        Task<InsightPoints> GetInsightPointsAsync(Guid insightId);
        Task<Insight[]> GetPagedInsightsWithMissingTwinDetailAsync(int pageNumber, int pageSize = 500);
        Task UpdateInsightsAsync(IEnumerable<Insight> insights, StatusLog[] statusLogs=null);
        Task<InsightActivityData> GetInsightActivities(Guid siteId, Guid insightId);
        Task<List<Insight>> GetSiteInsightsWithOccurrences(List<Guid> siteIds=null, List<Guid> insightIds = null, List<string> ruleIds = null);
        Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request, bool ignoreQueryFilters = false);
        Task<List<InsightFilter>> GetInsightFiltersAsync(List<Guid> siteIds, List<InsightStatus> StatusList);
        Task<List<DiagnosticsSnapshotDto>> GetChildrenDiagnostics(Guid insightId, DateTime started, DateTime? ended);

        Task<Insight> GetSimpleInsightAsync(Guid insightId, bool ignoreQueryFilters = false);

        Task<List<TwinInsightStatisticsDto>> GetInsightStatisticsByTwinIds(
            IList<string> twinIds,
            string includeRuleId,
            string excludeRuleId,
            bool includePriorityCounts = false);
        Task<BatchDto<SkillDto>> GetSkillsAsync(BatchRequestDto request, bool ignoreQueryFilters);
        Task<List<InsightOccurrencesCountByDate>> GetInsightOccurrencesByDate(string spaceTwinId, DateTime startDate, DateTime endDate);
        Task<List<ActiveInsightByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit);
    }

    public class InsightRepository : IInsightRepository
    {
        private readonly InsightDbContext _insightDbContext;
        private readonly INotificationService _notificationService;
        private readonly AppSettings _appSettings;

        public InsightRepository(InsightDbContext insightDbContext, INotificationService notificationService, IConfiguration configuration)
        {
            _insightDbContext = insightDbContext;
            _notificationService = notificationService;
            _appSettings = configuration.Get<AppSettings>(); ;
        }

        public async Task<List<ActiveInsightByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit)
        {
            var activeStatuses=new List<InsightStatus> { InsightStatus.Open, InsightStatus.New, InsightStatus.ReadyToResolve, InsightStatus.InProgress };
            return await _insightDbContext.Insights.Where(i => activeStatuses.Contains(i.Status) && i.State == InsightState.Active && i.Type != InsightType.Diagnostic
                                                               && i.Locations.Any(l => l.LocationId == spaceTwinId))
                        .GroupBy(i=>i.PrimaryModelId)
                        .Select(i => new ActiveInsightByModelIdDto
                        {
                           ModelId = i.Key,
                           Count = i.Count()
                        }).OrderByDescending(x => x.Count).Take(limit).ToListAsync();
        }
        public async Task<List<InsightOccurrencesCountByDate>> GetInsightOccurrencesByDate(string spaceTwinId, DateTime startDate,
            DateTime endDate)
        {
            return await _insightDbContext.InsightOccurrences
                .Include(x => x.Insight.Locations.Where(l => l.LocationId == spaceTwinId))
                .Where(x => x.Started >= startDate && x.Started <= endDate && x.IsFaulted && x.Insight.Locations.Any(l=>l.LocationId == spaceTwinId) )
                .GroupBy(x => x.Started.Date)
                .Select(x => new InsightOccurrencesCountByDate
                {
                    Date = x.Key,
                    Count = x.Count(),
                    AverageDuration =x.Average(y => (y.Ended - y.Started).TotalHours)
                }).OrderBy(x=>x.Date).ToListAsync();
        }
        public async Task<List<Insight>> GetSiteInsightsWithOccurrences(List<Guid> siteIds=null,List<Guid> insightIds=null, List<string> ruleIds=null)
        {
            IQueryable<InsightEntity> query = _insightDbContext.Insights
                .Include(i => i.ImpactScores)
                .Include(i => i.StatusLogs)
                .Include(i => i.Dependencies)
                .Include(i => i.InsightOccurrences)
                .AsSplitQuery()
                .IgnoreQueryFilters();

            if(siteIds!=null && siteIds.Any())
                query = query.Where(i => siteIds.Contains(i.SiteId) );
            if (insightIds != null && insightIds.Any())
                query = query.Where(x => insightIds.Contains(x.Id));
            if (ruleIds != null && ruleIds.Any())
                query = query.Where(x => ruleIds.Contains(x.RuleId));
            var insightEntities = await query.ToListAsync();
            return InsightEntity.MapTo(insightEntities);
        }

        public async Task<List<InsightOccurrence>> GetInsightOccurrencesAsync(Guid insightId)
        {
	        var insightOccurrenceEntities = await _insightDbContext.InsightOccurrences
                .Where(c => c.InsightId == insightId)
                .ToListAsync();

	        return InsightOccurrenceEntity.MapTo(insightOccurrenceEntities);
        }

        public async Task<InsightOccurrence> GetLastFaultyOccurrence(Guid insightId)
        {
            var faultyOccurrence = await _insightDbContext.InsightOccurrences
                .Where(x => (x.InsightId == insightId) && x.IsFaulted)
                .OrderByDescending(x => x.Started)
                .FirstOrDefaultAsync();

            return InsightOccurrenceEntity.MapTo(faultyOccurrence);
        }

        public async Task<InsightOccurrence> GetLastOccurrence(Guid insightId)
        {
            var occurrence = await _insightDbContext.InsightOccurrences
                .Where(x => (x.InsightId == insightId))
                .OrderByDescending(x => x.Started)
                .FirstOrDefaultAsync();

            return InsightOccurrenceEntity.MapTo(occurrence);
        }

        public async Task UpdateInsightsAsync(IEnumerable<Insight> insights, StatusLog[] statusLogs = null)
        {
            if (statusLogs != null && statusLogs.Any())
            {
                await _insightDbContext.StatusLog.AddRangeAsync(StatusLogEntity.MapFrom(statusLogs));
            }

            var entities = InsightEntity.MapFrom(insights);
            foreach (var entity in entities)
            {
                _insightDbContext.Entry(entity).State = EntityState.Modified;
            }

            _insightDbContext.SaveChanges();
        }

        public async Task<Insight[]> GetPagedInsightsWithMissingTwinDetailAsync(int pageNumber, int pageSize = 500)
        {
            var insightEntities = await _insightDbContext.Insights.Where(c => !string.IsNullOrEmpty(c.TwinId) && (string.IsNullOrWhiteSpace(c.PrimaryModelId) || string.IsNullOrWhiteSpace(c.TwinName))).OrderBy(c => c.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                 .ToArrayAsync();
            return (InsightEntity.MapTo(insightEntities)).ToArray();
        }

        public async Task<BatchDto<Insight>> GetInsights(BatchRequestDto request, bool ignoreQueryFilters = false)
		{
            var insightsDbContext = ignoreQueryFilters ? _insightDbContext.Insights.IgnoreQueryFilters() : _insightDbContext.Insights;

			return await insightsDbContext
                .Include(i => i.ImpactScores)
				.Include(i=>i.StatusLogs)
				.FilterBy(request.FilterSpecifications)
                .SortBy(request.SortSpecifications)
                .Paginate(request.Page,request.PageSize, InsightEntity.MapTo);
        }

        public async Task<BatchDto<SkillDto>> GetSkillsAsync(BatchRequestDto request, bool ignoreQueryFilters)
        {
           request.FilterSpecifications= request.FilterSpecifications.Upsert(nameof(Insight.Type), FilterOperators.NotEquals,
                InsightType.Diagnostic);
            var query = ignoreQueryFilters ? _insightDbContext.Insights.IgnoreQueryFilters() : _insightDbContext.Insights;
            return await query.FilterBy(request.FilterSpecifications)
                          .SortBy(request.SortSpecifications)
                          .GroupBy(c=> c.RuleId)
                          .Select(c=>new Skill {Id=c.Key,Name = c.First().RuleName,Category = c.First().Type})
                          .Paginate<Skill,SkillDto>(request.Page, request.PageSize, Skill.MapTo);
        }
        public Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request, bool ignoreQueryFilters = false)
        {
            var insightsDbContext = ignoreQueryFilters ? _insightDbContext.Insights.IgnoreQueryFilters() : _insightDbContext.Insights;

            return Task.FromResult(insightsDbContext
                .Where(i => i.Type != InsightType.Diagnostic)
                .Include(i => i.ImpactScores)
                .Include(i => i.StatusLogs)
                .FilterBy(request.FilterSpecifications)
                .SelectMany(x => x.ImpactScores)
                .ToList()
                .GroupBy(x => x.FieldId)
                .ToDictionary(x => x.Key, x => new ImpactScore
                {
                    FieldId = x.Key,
                    Name = x.Max(y => y.Name),
                    Value = x.Sum(y => y.Value),
                    Unit = x.Max(y => y.Unit)
                }).Select(x => x.Value).ToList());
        }

        public async Task<List<InsightFilter>> GetInsightFiltersAsync(List<Guid> siteIds, List<InsightStatus> StatusList)
        {
            return await _insightDbContext.Insights
                .Include(c =>
                    c.StatusLogs.Where(x => x.Status == InsightStatus.Ignored || x.Status == InsightStatus.Resolved))
                .Where(c => siteIds.Contains(c.SiteId) && c.Type != InsightType.Diagnostic && (StatusList == null || !StatusList.Any() || StatusList.Contains(c.Status)))
                .Select(x => new InsightFilter()
                {
                    Id= x.Id,
                    SiteId=x.SiteId,
                    Type= x.Type,
                    SourceId= x.SourceId,
                    SourceType = x.SourceType,
                    Reported = x.Reported,
                    Status = x.Status,
                    PrimaryModelId = x.PrimaryModelId,
                    StatusLogs=x.StatusLogs.Select(c=>c.Status)
                }).ToListAsync();
        }

        public async Task<BatchDto<InsightCard>> GetInsightCards(BatchRequestDto request)
        {
            var query = await _insightDbContext.Insights
                .FilterBy(request.FilterSpecifications)
                .GroupBy(x => x.RuleId)
                .Select(x => new InsightCard
                {
                     RuleId = x.Key,
                     RuleName = string.IsNullOrEmpty(x.Key) ? null : x.Max(y => y.RuleName),
                     InsightType = string.IsNullOrEmpty(x.Key) ? null : x.Max(y => y.Type),
                     InsightCount = x.Sum(y => 1),
                     LastOccurredDate = x.Max(y => y.LastOccurredDate),
                     Priority = x.Min(y => y.Priority),
                     SourceId = string.IsNullOrEmpty(x.Key) ? null : x.Max(y => y.SourceId),
                     PrimaryModelId = x.Max(y => y.PrimaryModelId),
                     Recommendation = x.Max(y => y.Recommendation)
                })
                .SortBy(request.SortSpecifications)
                .Paginate(request.Page, request.PageSize);

            var cards = query.Items.ToArray();
            var impactScores = _insightDbContext.ImpactScores
                .Join(_insightDbContext.Insights, s => s.InsightId, i => i.Id, (s, i) => new
                {
                    s.InsightId,
                    s.RuleId,
                    s.Unit,
                    s.Value,
                    s.FieldId,
                    s.Name,
                    s.ExternalId,
                    i.CustomerId,
                    i.SiteId,
                    i.SequenceNumber,
                    i.EquipmentId,
                    i.Type,
                    i.Description,
                    i.Priority,
                    i.Status,
                    i.CreatedDate,
                    i.UpdatedDate,
                    i.LastOccurredDate,
                    i.DetectedDate,
                    i.SourceType,
                    i.SourceId,
                    i.TwinId,
                    i.TwinName

                })
                .FilterBy(request.FilterSpecifications)
                .GroupBy(x => new { x.RuleId, x.FieldId })
                .Select(x => new ImpactScoreEntity
                {
                    RuleId = x.Key.RuleId,
                    FieldId = x.Key.FieldId,
                    Value = x.Key.RuleId == null ? 0: (ImpactScore.Priority.Contains(x.Key.FieldId) ? x.Max(y => y.Value) : x.Sum(y => y.Value)),
                    Name = x.Max(y => y.Name),
                    Unit = x.Max(y => y.Unit)
                }).ToList();

            foreach(var card in cards)
            {
                card.ImpactScores = ImpactScoreEntity.MapTo(impactScores?.Where(x => x.RuleId == card.RuleId)?.ToList()) ?? new List<ImpactScore>();
            }

            return new BatchDto<InsightCard>
            {
                After = query.After,
                Before = query.Before,
                Total = query.Total,
                Items = cards
            };
        }

        public async Task<Insight> GetInsight(Guid insightId, bool ignoreQueryFilters = false)
        {
            var query = _insightDbContext.Insights
                .Include(i => i.ImpactScores)
                .Include(i=>i.StatusLogs)
				.Include(i=>i.Dependencies)
                .AsSplitQuery()
                .Where(i => i.Id == insightId);

			if (ignoreQueryFilters)
			{
				query = query.IgnoreQueryFilters();
			}

			var insight = await query.FirstOrDefaultAsync();
            return InsightEntity.MapTo(insight);
        }

        public async Task<InsightAlias> GetInsightAlias(Guid insightId)
        {
            var insight = await _insightDbContext.Insights
                .Where(i => i.Id == insightId)
                .Select(x => new InsightAlias
                {
                    Name = x.Name,
                    RuleName = x.RuleName
                })
                .FirstOrDefaultAsync();

            if (insight == null)
            {
                throw new NotFoundException ($"insight: {insightId}");
            }

            return insight;
        }

        public static int ConvertPriority(double impactScorePriority)
        {
            // impact score priority is expected to be between 0 to 100.
            // it is converted to a value between 1-4 where 100 corresponds to 1
            // return (int)Math.Floor(5 - (double)(impactScorePriority / 25))))

            if (impactScorePriority > 75)
            {
                return 1;
            }
            else if (impactScorePriority > 50)
            {
                return 2;
            }
            else if (impactScorePriority > 25)
            {
                return 3;
            }
            return 4;
        }

        public async Task<Insight> CreateInsight(Insight insight)
        {
            if (insight.ImpactScores is not null && insight.ImpactScores.Any(x => ImpactScore.Priority.Contains(x.FieldId)))
            {
                var impactScorePriority = insight.ImpactScores.First(x => ImpactScore.Priority.Contains(x.FieldId)).Value;
                insight.Priority = ConvertPriority(impactScorePriority);
            }

            var entity = InsightEntity.MapFrom(insight);
            await _insightDbContext.Insights.AddAsync(entity);
            await _insightDbContext.StatusLog.AddAsync(StatusLogEntity.MapFrom(insight));
            await _insightDbContext.SaveChangesAsync();
            await SendNotificationAsync(entity);
            return InsightEntity.MapTo(entity);
        }

        public async Task UpdateInsight(Insight insight, StatusLog statusLog, Guid? sourceId = null, Guid? updatedByUserId = null)
        {
            // decide if we should send notification
            var sendNotification = false;
            if (insight.ImpactScores is not null && insight.ImpactScores.Any(x => ImpactScore.Priority.Contains(x.FieldId)))
            {
                var impactScorePriority = insight.ImpactScores.First(x => ImpactScore.Priority.Contains(x.FieldId)).Value;
                insight.Priority = ConvertPriority(impactScorePriority);
            }

            var entity = InsightEntity.MapFrom(insight);

			if (entity.ImpactScores != null)
			{
				_insightDbContext.RemoveRange(_insightDbContext.ImpactScores.Where(x => x.InsightId == entity.Id));
				await _insightDbContext.AddRangeAsync(entity.ImpactScores);
			}

            if (entity.Locations != null)
            {
                _insightDbContext.RemoveRange(_insightDbContext.InsightLocations.Where(x => x.InsightId == entity.Id));
                await _insightDbContext.AddRangeAsync(entity.Locations);
            }

            if (statusLog != null)
            {
                await _insightDbContext.StatusLog.AddAsync(StatusLogEntity.MapFrom(statusLog));
            }

            if (entity.InsightOccurrences != null)
            {
                /*
                 * The occurrences provided on update should be the source of truth regarding occurrence data within the timespan provided.
                   When updating occurrence data for an insight:
                        Determine the timespan provided by the update request.
                        Delete all saved occurrences from the earliest starttime of the request to the latest.
                        Add all the occurrences of the request.
                 */
                var orderedStarted = insight.InsightOccurrences.Select(x => x.Started).Order();
                var firstStarted = orderedStarted.FirstOrDefault();
                var lastStarted = orderedStarted.LastOrDefault();

                var occurrencesToReplace = _insightDbContext.InsightOccurrences.Where(x => x.InsightId == insight.Id && x.Started >= firstStarted && x.Started <= lastStarted);
                var occurrencesToKeep = _insightDbContext.InsightOccurrences.Where(x => x.InsightId == insight.Id && (x.Started < firstStarted || x.Started > lastStarted));

                _insightDbContext.InsightOccurrences.RemoveRange(occurrencesToReplace);
                _insightDbContext.InsightOccurrences.AddRange(entity.InsightOccurrences);

                entity.OccurrenceCount = occurrencesToKeep.Count(x => x.IsFaulted) + entity.InsightOccurrences.Count(x => x.IsFaulted);

                var changeStatusToNew = entity.InsightOccurrences.Any(x => x.IsFaulted);
                // we should change the status to new if the current status is Resolved and there is a new faulted occurrences and also there is no status log (means user didn't change the status)
                if (changeStatusToNew && entity.Status == InsightStatus.Resolved && statusLog == null)
                {
                    entity.Status = InsightStatus.New;
                    await _insightDbContext.StatusLog.AddAsync(StatusLogEntity.MapFrom(StatusLog.MapFrom(insight, new InsightStatusChangeDto
                    {
                        SourceId = sourceId,
                        Status = InsightStatus.New,
                        UserId = updatedByUserId
                    })));

                    sendNotification = true;
                }
            }
           
			if(entity.Dependencies is not null)
			{
				_insightDbContext.RemoveRange(_insightDbContext.Dependencies.Where(x => x.FromInsightId == entity.Id));
				await _insightDbContext.AddRangeAsync(entity.Dependencies);
			}

            _insightDbContext.Entry(entity).State = EntityState.Modified;

			await _insightDbContext.SaveChangesAsync();
            if (sendNotification)
            {
                await SendNotificationAsync(entity);
            }
        }

        public async Task<string> GenerateSequenceNumber(string sequenceNumberPrefix)
        {
            var insightNextNumber = await _insightDbContext.InsightNextNumbers.AsTracking().FirstOrDefaultAsync(x => x.Prefix == sequenceNumberPrefix);
            if (insightNextNumber == null)
            {
                insightNextNumber = new InsightNextNumberEntity
                {
                    Prefix = sequenceNumberPrefix,
                    NextNumber = 1
                };
                _insightDbContext.InsightNextNumbers.Add(insightNextNumber);
            }
            var sequenceNumber = insightNextNumber.NextNumber;
            insightNextNumber.NextNumber++;
            await _insightDbContext.SaveChangesAsync();
            return $"{sequenceNumberPrefix}-I-{sequenceNumber}";
        }

        public async Task<List<Insight>> GetActiveUniqueInsight(Guid siteId,string twinId, string name)
        {
            var query = _insightDbContext.Insights
                                            .Include(i => i.ImpactScores)
                                            .Where(i => i.SiteId == siteId &&
                                                         (i.Status == InsightStatus.Open || i.Status == InsightStatus.New || i.Status == InsightStatus.ReadyToResolve) &&
                                                        i.State == InsightState.Active &&
                                                        i.Name == name &&
                                                        i.TwinId == twinId);
            var insights = await query.ToListAsync();
            return InsightEntity.MapTo(insights);
        }

        public async Task<List<InsightSource>> GetInsightSources()
        {
            var result = await _insightDbContext.Insights
                .Select(x => new InsightSource { SourceId = x.SourceId, SourceType = x.SourceType })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<List<InsightStatisticsByPriority>> GetInsightStatisticsByPriorityList(IList<Guid> siteIds)
        {
            var insightStatisticsList = await _insightDbContext.Insights.AsNoTracking()
                .Where(x => siteIds.Contains(x.SiteId)
                && x.Type != InsightType.Diagnostic
                && (x.Status == InsightStatus.New || x.Status == InsightStatus.Open || x.Status == InsightStatus.InProgress || x.Status == InsightStatus.ReadyToResolve))
                .GroupBy(x => x.SiteId)
                .Select(g => new InsightStatisticsByPriority
                {
                    Id = g.Key,
                    OpenCount = g.Count(x => x.Status == InsightStatus.New || x.Status == InsightStatus.Open),
                    UrgentCount = g.Count(x => x.Priority == 1),
                    HighCount = g.Count(x => x.Priority == 2),
                    MediumCount = g.Count(x => x.Priority == 3),
                    LowCount = g.Count(x => x.Priority == 4)
                })
                .ToListAsync();

            // Filter out siteIds that are not already present in insightStatisticsList
            var siteIdsToAdd = siteIds.Except(insightStatisticsList.Select(x => x.Id));

            // Add new InsightStatisticsByStatus objects for the remaining siteIds
            insightStatisticsList.AddRange(siteIdsToAdd.Select(siteId => new InsightStatisticsByPriority { Id = siteId }));
            return insightStatisticsList;
        }

        public async Task<List<TwinInsightStatisticsDto>> GetInsightStatisticsByTwinIds(
            IList<string> twinIds,
            string includeRuleId,
            string excludeRuleId,
            bool includePriorityCounts = false)
        {
            var query = _insightDbContext.Insights
                .Where(x =>
                    twinIds.Contains(x.TwinId)
                    && x.Type != InsightType.Diagnostic
                    && (
                        x.Status == InsightStatus.New
                        || x.Status == InsightStatus.Open
                        || x.Status == InsightStatus.InProgress
                        || x.Status == InsightStatus.ReadyToResolve));

            if (!string.IsNullOrWhiteSpace(includeRuleId))
            {
                query = query.Where(c => c.RuleId == includeRuleId);
            }
            else if (!string.IsNullOrWhiteSpace(excludeRuleId))
            {
                query = query.Where(c => c.RuleId != excludeRuleId);
            }

            return await query.GroupBy(x => x.TwinId)
                .Select(g => new TwinInsightStatisticsDto
                {
                    TwinId = g.Key,
                    InsightCount = g.Count(),
                    HighestPriority = g.Min(x => x.Priority),
                    RuleIds = g.Select(c => c.RuleId).Distinct().ToList(),
                    PriorityCounts = includePriorityCounts ? new()
                    {
                        OpenCount = g.Count(x => x.Status == InsightStatus.New || x.Status == InsightStatus.Open),
                        UrgentCount = g.Count(x => x.Priority == 1),
                        HighCount = g.Count(x => x.Priority == 2),
                        MediumCount = g.Count(x => x.Priority == 3),
                        LowCount = g.Count(x => x.Priority == 4)
                    } : null
                })
                .ToListAsync();
        }

        public async Task<List<InsightStatisticsByStatus>> GetInsightStatisticsByStatusList(IList<Guid> siteIds)
        {
            var insightStatisticsList = await _insightDbContext.Insights.AsNoTracking()
                .Where(x => siteIds.Contains(x.SiteId) && x.Type != InsightType.Diagnostic)
                .GroupBy(x => x.SiteId)
                .Select(g => new InsightStatisticsByStatus
                {
                    Id = g.Key,
                    InProgressCount = g.Count(x => x.Status == InsightStatus.InProgress),
                    ReadyToResolveCount = g.Count(x => x.Status == InsightStatus.ReadyToResolve),
                    NewCount = g.Count(x => x.Status == InsightStatus.New),
                    OpenCount = g.Count(x => x.Status == InsightStatus.Open),
                    IgnoredCount = g.Count(x => x.Status == InsightStatus.Ignored),
                    ResolvedCount = g.Count(x => x.Status == InsightStatus.Resolved),
                    AutoResolvedCount = g.Count(x => x.Status == InsightStatus.Resolved && x.SourceType == SourceType.App)
                })
                .ToListAsync();

            // Filter out siteIds that are not already present in insightStatisticsList
            var siteIdsToAdd = siteIds.Except(insightStatisticsList.Select(x => x.Id));

            // Add new InsightStatisticsByStatus objects for the remaining siteIds
            insightStatisticsList.AddRange(siteIdsToAdd.Select(siteId => new InsightStatisticsByStatus { Id = siteId }));
            return insightStatisticsList;
        }

        public async Task<List<InsightSnackbarByStatus>> GetInsightSnackbarByStatus(IEnumerable<FilterSpecificationDto> filters)
        {
            return await _insightDbContext.Insights.AsNoTracking()
                .FilterBy(filters)
                .GroupBy(x => x.Status)
                .Select(g => new InsightSnackbarByStatus
                {
                    Id = g.Count() == 1 ? g.Max(x => x.Id) : null,
                    Status = g.Key,
                    Count = g.Count(),
                    SourceType = g.GroupBy(x => x.SourceType).Count() == 1 ? g.Max(x => x.SourceType) : null,
                    SourceId = g.GroupBy(x => x.SourceId).Count() == 1 ? g.Max(x => x.SourceId) : null,
                })
                .ToListAsync();
        }

        public async Task<List<StatusLog>> GetInsightStatusLog(Guid insightId, Guid siteId)
		{
			var statusLog = await _insightDbContext.Insights
															.Include(x => x.StatusLogs)
															.Where(x => x.Id == insightId & x.SiteId == siteId)
															.Select(x=>x.StatusLogs)
															.FirstOrDefaultAsync();

			return StatusLogEntity.MapToList(statusLog?.ToList());
		}
        /// <summary>
        /// Insight points represents the points associate with an insight and impact scores with external id
        /// </summary>
        /// <param name="insightId"></param>
        /// <returns></returns>
        public async Task<InsightPoints> GetInsightPointsAsync(Guid insightId)
        {
            var insightPoints = await _insightDbContext.Insights
                                                    .Include(x => x.ImpactScores)
                                                    .Where(x => x.Id == insightId)
                                                    .Select(x => new InsightPoints
                                                    {
                                                        PointsJson = x.PointsJson,
                                                        ImpactScores = ImpactScoreEntity.MapTo(x.ImpactScores)
                                                    })
                                                    .FirstOrDefaultAsync();
            return insightPoints;
        }

        public async Task<InsightActivityData> GetInsightActivities(Guid siteId, Guid insightId)
        {
            var insightActivities = await _insightDbContext.Insights
                                                            .Include(x => x.StatusLogs)
                                                            .Include(x => x.InsightOccurrences)
                                                            .Where(x => x.Id == insightId & x.SiteId == siteId)
                                                            .Select(x => new InsightActivityData(x.InsightOccurrences, x.StatusLogs))
                                                            .FirstOrDefaultAsync();


            return insightActivities;
        }

        public async Task<List<DiagnosticsSnapshotDto>> GetChildrenDiagnostics(Guid insightId, DateTime started, DateTime? ended)
        {
            var childrenDiagnostics = await _insightDbContext.Dependencies.AsNoTracking()
            .Join(_insightDbContext.Insights, d => d.ToInsightId, i => i.Id, (d, i) => new
            {
                i.RuleName,
                i.Name,
                d.ToInsightId,
                d.FromInsightId
            })
            .Join(_insightDbContext.InsightOccurrences, j => j.ToInsightId, o => o.InsightId, (j, o) => new
            {
                j.RuleName,
                j.Name,
                j.ToInsightId,
                j.FromInsightId,
                o.Started,
                o.Ended,
                o.IsFaulted
            })
            .Where(x => x.FromInsightId == insightId && (x.Ended == DateTime.MinValue || (x.Started >= started && x.Ended <= ended)))
            .GroupBy(x => new { x.ToInsightId, x.Name, x.RuleName, x.IsFaulted })
            .Select(x => new DiagnosticsSnapshotDto()
            {
                Id = x.Key.ToInsightId,
                Name = x.Key.Name,
                RuleName = x.Key.RuleName,
                Check = !x.Key.IsFaulted,
                Started = x.Max(y => y.Started),
                Ended = x.Max(y => y.Ended)
            })
            .ToListAsync();

            return childrenDiagnostics.GroupBy(x => x.Id).Select(x => x.OrderByDescending(y => y.Started).FirstOrDefault()).ToList();
        }

        public async Task<Insight> GetSimpleInsightAsync(Guid insightId, bool ignoreQueryFilters = false)
        {
            var query = _insightDbContext.Insights
                .Where(i => i.Id == insightId);

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            var insight = await query.FirstOrDefaultAsync();

            return InsightEntity.MapTo(insight);
        }

        /// <summary>
        /// Send notification for the insight to the notification service
        /// </summary>
        /// <param name="insight"></param>
        /// <returns></returns>
        private async Task SendNotificationAsync(InsightEntity insight)
        {
            if (_appSettings.IsNotificationEnabled)
            {
                await _notificationService.SendNotificationAsync(new NotificationMessage
                {
                    NotificationSource = NotificationSource.Insight,
                    SourceId = insight.Id.ToString(),
                    Title = insight.Name,
                    PropertyBagJson = new
                    {
                        TwinId = insight.TwinId,
                        TwinName = insight.TwinName,
                        ModelId = insight.PrimaryModelId,
                        SkillId = insight.RuleId,
                        SkillCategoryId = (int) insight.Type,
                        Priority = insight.Priority,
                        Locations = insight.Locations?.Select(x => x.LocationId)?.ToList(),

                    }
                });
            }
        }
    }
}
