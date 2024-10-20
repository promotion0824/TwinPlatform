using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Repository;
using WorkflowCore.Models;
using Willow.Common;
using Willow.Data;
using System.Linq;
using WorkflowCore.Services.Apis;
using WorkflowCore.Dto;
using System.Threading;
using WorkflowCore.Entities;
using Newtonsoft.Json;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Services
{
    public interface IWorkflowService
    {
        Task<List<TicketCategory>> GetTicketCategories(Guid siteId);
        Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task<TicketCategory> CreateTicketCategory(Guid siteId, CreateTicketCategoryRequest ticketCategoryRequest);
        Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest updateTicketCategoryRequest);
        Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task<List<Ticket>> GetSiteTickets(Guid siteId, GetSiteTicketsRequest siteTicketsRequest);
        Task<int> GetTicketsCount(Guid siteId, IList<int> statuses, bool isScheduled);
        Task<bool> GetTicketExistence(Guid siteId, Guid ticketId, bool isTemplate);
        Task<Ticket> GetTicket(Guid ticketId, bool includeAttachments, bool includeComments);
        Task<Ticket> GetTicketBySequenceNumber(String sequenceNumber);
        Task<Ticket> CreateTicket(Guid siteId, CreateTicketRequest createTicketRequest, int ticketStatus, string language);
        Task<List<Ticket>> CreateTicket(List<Ticket> tickets,Guid siteId, string language);
        Task UpdateTicket(Guid siteId, Guid ticketId, UpdateTicketRequest updateRequest, string language);
        Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence);
        [Obsolete("Instead use Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)")]
        Task<bool> TicketOccurrenceExists(Guid templateId, Guid assetId, int occurrence);
        Task<string> GenerateSequenceNumber(string prefix, string key = "S");
        Task<CheckRecord> GetCheckRecord(Guid checkRecordId);
        Task<List<TicketStatus>> GetTicketStatus(Guid customerId);
        Task<List<TicketStatus>> CreateOrUpdateTicketStatus(Guid customerId, CreateTicketStatusRequest request);
        Task<List<Ticket>> CreateTickets(Guid siteId, CreateTicketsRequest request, string language);
        Task AddTwinIdToTicketAsync(int batchSize, CancellationToken stoppingToken);
        Task AddTwinsToTicketTemplateAsync(int batchSize, CancellationToken stoppingToken);
        Task<List<TwinTicketStatisticsDto>> GetTicketStatisticsByTwinIds(TwinStatisticsRequest request);
        Task<List<TwinTicketStatisticsByStatus>> GetTicketStatusStatisticsByTwinIds(TwinStatisticsRequest request);
        Task<TicketCategoryCountDto> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId, int categoryLimit);
        Task<TicketCountsByDateDto> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IWorkflowRepository _repository;
        private readonly IWorkflowSequenceNumberService _workflowSequenceNumberService;
        private readonly IWorkflowNotificationService _notificationService;
        private readonly IReadRepository<Guid, Site> _siteRepo;
        private readonly ILogger<WorkflowService> _logger;
		private readonly IInsightServiceApi _insightServiceApi;
        private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;

        public WorkflowService(IDateTimeService dateTimeService,
            IWorkflowRepository repository,
            IWorkflowNotificationService notificationService,
            IReadRepository<Guid, Site> siteRepo,
            ILogger<WorkflowService> logger,
            IInsightServiceApi insightServiceApi,
            IDigitalTwinServiceApi digitalTwinServiceApi,
            IWorkflowSequenceNumberService workflowSequenceNumberService)
        {
            _workflowSequenceNumberService = workflowSequenceNumberService;

            _dateTimeService = dateTimeService;
            _repository = repository;
            _notificationService = notificationService;
            _siteRepo = siteRepo ?? throw new ArgumentNullException(nameof(siteRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _insightServiceApi = insightServiceApi ?? throw new ArgumentNullException(nameof(insightServiceApi));
            _digitalTwinServiceApi = digitalTwinServiceApi ?? throw new ArgumentNullException(nameof(digitalTwinServiceApi));
        }

        public async Task<List<TwinTicketStatisticsDto>> GetTicketStatisticsByTwinIds(TwinStatisticsRequest request)
        {
            if (request.TwinIds == null || !request.TwinIds.Any())
            {
                throw new BadRequestException("The twinIds are required");
            }

            return await _repository.GetTicketStatisticsByTwinIds(request.TwinIds.Distinct().ToList(),request.SourceTypes);
        }

        /// <summary>
        /// Ticket Status Statistics
        /// </summary>
        /// <param name="request">Twins and source types</param>
        /// <returns>Ticket Status Statistics</returns>
        /// <exception cref="BadRequestException">No twins in request</exception>
        public async Task<List<TwinTicketStatisticsByStatus>> GetTicketStatusStatisticsByTwinIds(TwinStatisticsRequest request)
        {
            if (request.TwinIds == null)
            {
                throw new BadRequestException("The twinIds are required");
            }

            return await _repository.GetTicketStatusStatisticsByTwinIds(request.TwinIds.Distinct().ToList(), request.SourceTypes);
        }

        public async Task<List<TicketCategory>> GetTicketCategories(Guid siteId)
        {
            return await _repository.GetTicketCategories(siteId);
        }

        public async Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            return await _repository.GetTicketCategory(siteId, ticketCategoryId);
        }

        public async Task<TicketCategory> CreateTicketCategory(Guid siteId, CreateTicketCategoryRequest ticketCategoryRequest)
        {
            var ticketCategory = new TicketCategory
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                Name = ticketCategoryRequest.Name
            };
            await _repository.CreateTicketCategory(siteId, ticketCategory);
            return ticketCategory;
        }

        public async Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest updateTicketCategoryRequest)
        {
            await _repository.UpdateTicketCategory(siteId, ticketCategoryId, updateTicketCategoryRequest);
        }

        public async Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            await _repository.DeleteTicketCategory(siteId, ticketCategoryId);
        }

        public async Task<bool> GetTicketExistence(Guid siteId, Guid ticketId, bool isTemplate)
        {
            return await _repository.GetTicketExistence(siteId, ticketId, isTemplate);
        }

		public async Task<Ticket> GetTicket(Guid ticketId, bool includeAttachments, bool includeComments)
		{
			var ticket = await _repository.GetTicket(ticketId, includeAttachments, includeComments);

			if (ticket is not null && ticket.InsightId.HasValue)
			{
				ticket.CanResolveInsight =!(await _repository.HasInsightOpenTicketsAsync(ticket.InsightId.Value,ticket.Id));
			}
			return ticket;
		}

		public async Task<Ticket> GetTicketBySequenceNumber(string sequenceNumber)
        {
            var ticket = await _repository.GetTicketBySequenceNumber(sequenceNumber);
            return ticket;
        }

        public async Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)
        {
            return await _repository.TicketOccurrenceExists(templateId, twinId, occurrence);
        }

        [Obsolete("Instead use Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)")]
        public async Task<bool> TicketOccurrenceExists(Guid templateId, Guid assetId, int occurrence)
        {
            return await _repository.TicketOccurrenceExists(templateId, assetId, occurrence);
        }

        public async Task<string> GenerateSequenceNumber(string prefix, string key = "S")
        {
            return await _workflowSequenceNumberService.GenerateSequenceNumber(prefix, key);
        }

        public async Task<List<Ticket>> GetSiteTickets(Guid siteId, GetSiteTicketsRequest siteTicketsRequest)
        {
            return await _repository.GetSiteTickets(siteId, siteTicketsRequest);
        }

        public async Task<int> GetTicketsCount(Guid siteId, IList<int> statuses, bool isScheduled)
        {
            return await _repository.GetTicketsCount(siteId, statuses, isScheduled);
        }

        public async Task<List<Ticket>> CreateTickets(Guid siteId, CreateTicketsRequest request,
            string language)
        {
            var issueTwinIds=await GetTwinIdsByUniqueIdsAsync(request.Tickets.Where(c=>string.IsNullOrEmpty(c.TwinId) && c.IssueId.HasValue)?.Select(c=>c.IssueId.Value),siteId);
            var ticketList = new List<Ticket>();

            foreach (var ticketRequest in request.Tickets)
            {
                var createTicketRequest = new CreateTicketRequest
                {
                    CustomerId = request.CustomerId,
                    FloorCode = ticketRequest.FloorCode,
                    SequenceNumberPrefix = request.SequenceNumberPrefix,
                    Priority = ticketRequest.Priority,
                    IssueType = ticketRequest.IssueType,
                    IssueId = ticketRequest.IssueId,
                    IssueName = ticketRequest.IssueName,
                    Summary = ticketRequest.Summary,
                    Description = ticketRequest.Description,
                    Cause = ticketRequest.Cause,
                    ReporterId = null,
                    ReporterName = ticketRequest.ReporterName,
                    ReporterPhone = ticketRequest.ReporterPhone,
                    ReporterEmail = ticketRequest.ReporterEmail,
                    ReporterCompany = ticketRequest.ReporterCompany,
                    AssigneeType = ticketRequest.AssigneeType,
                    AssigneeId = ticketRequest.AssigneeId,
                    CreatorId = ticketRequest.CreatorId,
                    DueDate = ticketRequest.DueDate,
                    SourceType = request.SourceType,
                    SourceId = request.SourceId,
                    ExternalId = ticketRequest.ExternalId,
                    ExternalStatus = ticketRequest.ExternalStatus,
                    ExternalMetadata = ticketRequest.ExternalMetadata,
                    CustomProperties = ticketRequest.CustomProperties,
                    ExtendableSearchablePropertyKeys = ticketRequest.ExtendableSearchablePropertyKeys,
                    ExternalCreatedDate = ticketRequest.ExternalCreatedDate,
                    ExternalUpdatedDate = ticketRequest.ExternalUpdatedDate,
                    LastUpdatedByExternalSource = ticketRequest.LastUpdatedByExternalSource,
                    Latitude = ticketRequest.Latitude,
                    Longitude = ticketRequest.Longitude,
                    CategoryId = ticketRequest.CategoryId,
                    TwinId =string.IsNullOrEmpty(ticketRequest.TwinId)&&ticketRequest.IssueId.HasValue?issueTwinIds?.FirstOrDefault(c=>c.UniqueId==ticketRequest.IssueId.ToString())?.Id:ticketRequest.TwinId
                };
                ticketList.Add(await MapCreateRequestToTicket(createTicketRequest,ticketRequest.Status,siteId));

            }

            return await CreateTicket(ticketList, siteId, language);
        }
        public async Task<Ticket> CreateTicket(Guid siteId, CreateTicketRequest createTicketRequest, int ticketStatus, string language)
        {
            // set site and issue twin ids
            await SetTwinIds(siteId, createTicketRequest);
            var ticket = await MapCreateRequestToTicket(createTicketRequest, ticketStatus, siteId);
            return (await CreateTicket(new List<Ticket>(){ ticket},siteId, language)).FirstOrDefault();
        }

        public async Task<List<Ticket>> CreateTicket(List<Ticket> tickets,Guid siteId, string language)
        {
            var site = await _siteRepo.Get(siteId);
            var createdTickets=new List<Ticket>();
            await _repository.CreateTicketsAsync(tickets);
            foreach (var ticket in tickets)
            {
                try
                {
                    await _notificationService.NotifyAssignees(ticket, site?.Name ?? "", language).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to notify assignees for new ticket {TicketId}", ticket.Id);
                }
                createdTickets.Add(await _repository.GetTicket(ticket.Id,true,true));
            }
            if (tickets.Any(c => c.InsightId.HasValue))
            {
                await _insightServiceApi.UpdateInsightStatusAsync(siteId, new BatchUpdateInsightStatusRequest
                {
                    // All the tickets have same sourceId and CreatorId
                    Ids = tickets.Where(c => c.InsightId.HasValue).Select(c => c.InsightId.Value),
                    SourceId = tickets[0].SourceId,
                    UpdatedByUserId = tickets[0].SourceId.HasValue ? null : tickets[0].CreatorId,
                    Status = InsightStatus.InProgress
                });
            }
            return createdTickets;
        }

        public async Task UpdateTicket(Guid siteId, Guid ticketId, UpdateTicketRequest updateRequest, string language)
        {
            var customerTicketStatuses = await _repository.GetTicketStatuses(updateRequest.CustomerId);

            if (updateRequest.Priority.HasValue && (updateRequest.Priority.Value < 1 || updateRequest.Priority.Value > 4))
            {
                throw new ArgumentException().WithData(new { Priority = updateRequest.Priority.Value });
            }

            if (updateRequest.Status.HasValue &&
                    !customerTicketStatuses.Any(ts => ts.StatusCode == updateRequest.Status) &&
                        !Enum.IsDefined(typeof(TicketStatusEnum), updateRequest.Status))
            {
                throw new ArgumentException("Status is invalid.").WithData(new { updateRequest.Status });
            }

            var originalTicket = await GetTicket(ticketId, false, false);
            var site = await _siteRepo.Get(siteId);

            await _repository.UpdateTicket(ticketId, updateRequest);

	        try
            {
                await _notificationService.NotifyAssignees(updateRequest, originalTicket, site?.Name ?? "", language);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to notify assignees after updating ticket {TicketId}", ticketId);
            }
        }

        public async Task<CheckRecord> GetCheckRecord(Guid checkRecordId)
        {
            return await _repository.GetCheckRecord(checkRecordId);
        }

        public async Task<List<TicketStatus>> GetTicketStatus(Guid customerId)
        {
            return await _repository.GetTicketStatuses(customerId);
        }

        public async Task<List<TicketStatus>> CreateOrUpdateTicketStatus(Guid customerId, CreateTicketStatusRequest request)
        {
            var newTicketStatuses = request.TicketStatuses.Select(s => new TicketStatus
                                                                        {
                                                                            CustomerId = customerId,
                                                                            StatusCode = s.StatusCode,
                                                                            Status = s.Status,
                                                                            Tab = s.Tab,
                                                                            Color = s.Color
                                                                        }).ToList();
			return await _repository.CreateOrUpdateTicketStatuses(customerId, newTicketStatuses);
        }

        public async Task AddTwinsToTicketTemplateAsync(int batchSize, CancellationToken stoppingToken)
        {
            var pageNumber = 1;
            var ticketTemplates = await _repository.GetPagedTicketTemplatesWithAssetsAsync(pageNumber, batchSize);

            while (ticketTemplates != null && ticketTemplates.Any())
            {
                var ticketTemplatesGroupedBySite = ticketTemplates.GroupBy(c => c.SiteId);

                foreach (var siteGroup in ticketTemplatesGroupedBySite)
                {
                    try
                    {
                        var siteTicketTemplates = siteGroup.ToList();

                        var siteGroupAssetIds = siteTicketTemplates
                            .Select(x => x.Assets)
                            .SelectMany(JsonConvert.DeserializeObject<List<TicketAsset>>)
                            .Select(x => x.AssetId).Distinct();

                        var siteGroupTwinIds = await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(siteGroup.Key, siteGroupAssetIds);

                        if (siteGroupTwinIds != null && siteGroupTwinIds.Any())
                        {
                            foreach(var ticketTemplate in siteTicketTemplates)
                            {
                                var ticketTemplateAssets = TicketTemplateEntity.MapToModel(ticketTemplate).Assets;
                                ticketTemplate.Twins = JsonConvert.SerializeObject(ticketTemplateAssets.Select(x => new TicketTwin
                                {
                                    TwinId = siteGroupTwinIds.FirstOrDefault(y => y.UniqueId.Equals(x.AssetId.ToString(), StringComparison.InvariantCultureIgnoreCase))?.Id,
                                    TwinName = x.AssetName
                                }).ToList());

                                ticketTemplate.Assets = null;
                            }
                        }
                        await _repository.UpdateEntitiesAsync(siteTicketTemplates);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to get twinId  in AddTwinIdToTicketsAsync {Message}", ex.Message);
                    }

                    pageNumber += 1;
                    ticketTemplates = await _repository.GetPagedTicketTemplatesWithAssetsAsync(pageNumber, batchSize);
                }
            }
        }

        public async Task AddTwinIdToTicketAsync(int batchSize, CancellationToken stoppingToken)
        {
            var pageNumber = 1;
            var tickets = await _repository.GetPagedTicketsWithIssueIdAndNoTwinIdAsync(pageNumber, batchSize);
            while (tickets != null && tickets.Any())
            {

                var ticketGroupedBySite = tickets.GroupBy(c => c.SiteId);

                foreach (var site in ticketGroupedBySite)
                {
                    try
                    {
                        var siteTickets = site.ToList();
                        var siteTwinIds = await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(site.Key,
                            siteTickets.Select(c => c.IssueId.Value).Distinct());
                        if (siteTwinIds != null && siteTwinIds.Any())
                        {
                            siteTickets.ForEach(inspection =>
                                inspection.TwinId =
                                    siteTwinIds.FirstOrDefault(c => c.UniqueId.Equals(inspection.IssueId.Value.ToString(), StringComparison.InvariantCultureIgnoreCase))?.Id);

                        }
                        await _repository.UpdateEntitiesAsync(siteTickets);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Failed to get twinId  in AddTwinIdToTicketsAsync {Message}", ex.Message);
                    }

                    pageNumber += 1;
                    tickets = await _repository.GetPagedTicketsWithIssueIdAndNoTwinIdAsync(pageNumber, batchSize);
                }

            }
        }

        public async Task<TicketCategoryCountDto> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId, int limit)
        {
            var getCategoryCounts = await _repository.GetTicketCategoryCountBySpaceTwinId(spaceTwinId);
            var allCategoryCounts = getCategoryCounts.Sum(x => x.Count);

            var limitedCategoryCounts = getCategoryCounts
                .Take(limit)
                .ToList();

            var otherCategoryCount = allCategoryCounts - limitedCategoryCounts.Sum(x => x.Count);

            return new TicketCategoryCountDto
            {
                CategoryCounts = limitedCategoryCounts,
                OtherCount = otherCategoryCount
            };
        }

        public Task<TicketCountsByDateDto> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate)
        {
            return _repository.GetTicketsCountsByCreatedDate(spaceTwinId, startDate, endDate);
        }

        private async Task<Ticket> MapCreateRequestToTicket(CreateTicketRequest createTicketRequest, int ticketStatus,Guid siteId)
        {
            var sequenceNumber = await GenerateSequenceNumber(createTicketRequest.SequenceNumberPrefix, "T");
            var utcNow = _dateTimeService.UtcNow;

            return new Ticket
            {
                Id = Guid.NewGuid(),
                CustomerId = createTicketRequest.CustomerId,
                SiteId = siteId,
                FloorCode = createTicketRequest.FloorCode ?? string.Empty,
                SequenceNumber = sequenceNumber,
                Priority = createTicketRequest.Priority,
                Status = ticketStatus,
                IssueType = createTicketRequest.IssueType,
                IssueId = createTicketRequest.IssueId,
                IssueName = createTicketRequest.IssueName,
                InsightId = createTicketRequest.InsightId,
                InsightName = createTicketRequest.InsightName,
                Summary = createTicketRequest.Summary ?? string.Empty,
                Description = createTicketRequest.Description ?? string.Empty,
                Cause = createTicketRequest.Cause ?? string.Empty,
                Solution = string.Empty,
                ReporterId = createTicketRequest.ReporterId,
                ReporterName = createTicketRequest.ReporterName ?? string.Empty,
                ReporterPhone = createTicketRequest.ReporterPhone ?? string.Empty,
                ReporterEmail = createTicketRequest.ReporterEmail ?? string.Empty,
                ReporterCompany = createTicketRequest.ReporterCompany ?? string.Empty,
                AssigneeType = createTicketRequest.AssigneeId.HasValue ? createTicketRequest.AssigneeType : AssigneeType.NoAssignee,
                AssigneeId = createTicketRequest.AssigneeId,
                CreatorId = createTicketRequest.CreatorId,
                DueDate = createTicketRequest.DueDate,
                CreatedDate = GetTicketCreatedDate(createTicketRequest),
                UpdatedDate = utcNow,
                ResolvedDate = null,
                ClosedDate = null,
                SourceType = createTicketRequest.SourceType,
                SourceId = createTicketRequest.SourceId,
                ExternalId = createTicketRequest.ExternalId ?? string.Empty,
                ExternalStatus = createTicketRequest.ExternalStatus ?? string.Empty,
                ExternalMetadata = createTicketRequest.ExternalMetadata ?? string.Empty,
                CustomProperties = createTicketRequest.CustomProperties,
                ExtendableSearchablePropertyKeys = createTicketRequest.ExtendableSearchablePropertyKeys,
                ExternalCreatedDate = createTicketRequest.ExternalCreatedDate,
                ExternalUpdatedDate = createTicketRequest.ExternalUpdatedDate,
                LastUpdatedByExternalSource = createTicketRequest.LastUpdatedByExternalSource,
                Latitude = createTicketRequest.Latitude,
                Longitude = createTicketRequest.Longitude,
                CategoryId = createTicketRequest.CategoryId,
                Tasks = createTicketRequest.Tasks,
                Notes = createTicketRequest.Notes ?? string.Empty,
                TwinId = createTicketRequest.TwinId,
                SpaceTwinId = createTicketRequest.SpaceTwinId,
                JobTypeId = createTicketRequest.JobTypeId,
                ServiceNeededId = createTicketRequest.ServiceNeededId,
                Diagnostics = createTicketRequest.Diagnostics
            };
        }
        private DateTime GetTicketCreatedDate(CreateTicketRequest createTicketRequest)
        {
            if (createTicketRequest.LastUpdatedByExternalSource && createTicketRequest.ExternalCreatedDate != null)
            {
                return (DateTime)createTicketRequest.ExternalCreatedDate;
            }

            return _dateTimeService.UtcNow;
        }
        private async Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(IEnumerable<Guid> uniqueIds, Guid siteId)
        {   // Due to the misconfigured DT setting for some sites, and also invalid uniqueIds on production, we skip the exception.
            try
            {
                return uniqueIds != null && uniqueIds.Any() ?
                        await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(siteId,
                           uniqueIds) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"'adding TwinId to the ticket failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");

            }
            return null;
        }
        /// <summary>
        /// Set twin ids for ticket using unique ids
        /// set space twin id for the ticket site
        /// set twin id for the ticket issue
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task SetTwinIds(Guid siteId, CreateTicketRequest request)
        {

            var uniqueIds = new List<Guid>();
            if (string.IsNullOrEmpty(request.SpaceTwinId))
            {
                uniqueIds.Add(siteId);
            }
            if (string.IsNullOrEmpty(request.TwinId) && request.IssueId.HasValue)
            {
                uniqueIds.Add(request.IssueId.Value);
            }
            if (uniqueIds.Any())
            {
                var twins = await GetTwinIdsByUniqueIdsAsync(uniqueIds, siteId);
                if (twins is not null && twins.Any())
                {
                    if (string.IsNullOrEmpty(request.SpaceTwinId))
                    {
                        request.SpaceTwinId = twins.FirstOrDefault(c => c.UniqueId == siteId.ToString())?.Id;
                    }
                    if (string.IsNullOrEmpty(request.TwinId) && request.IssueId.HasValue)
                    {
                        request.TwinId = twins.FirstOrDefault(c => c.UniqueId == request.IssueId.ToString())?.Id;
                    }
                }
            }



        }
        
    }
}
