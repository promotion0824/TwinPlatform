using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Willow.Api.Client;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure;
using WorkflowCore.Infrastructure.Json;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
namespace WorkflowCore.Repository
{
    public interface IWorkflowRepository
    {
        Task<List<TwinTicketStatisticsByStatus>> GetTicketStatusStatisticsByTwinIds(IList<string> twinIds, List<SourceType> sourceTypes);
        Task<List<TwinTicketStatisticsDto>> GetTicketStatisticsByTwinIds(IList<string> twinIds, List<SourceType> sourceTypes);
        Task<List<TicketCategory>> GetTicketCategories(Guid siteId);
        Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task CreateTicketCategory(Guid siteId, TicketCategory ticketCategory);
        Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest request);
        Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task<List<Ticket>> GetSiteTickets(Guid siteId, GetSiteTicketsRequest request);
        Task<int> GetTicketsCount(Guid siteId, IList<int> statuses, bool isScheduled);
        Task<bool> GetTicketExistence(Guid siteId, Guid ticketId, bool isTemplate);
        Task<Ticket> GetTicket(Guid ticketId, bool includeAttachments, bool includeComments);
        Task<Ticket> GetTicketBySequenceNumber(String sequenceNumber);
        Task<long> GenerateSequenceNumber(string sequenceNumberPrefix);
        Task<long> GenerateSequenceNumber(string sequenceNumberPrefix, string suffix);      
        Task UpdateTicket(Guid ticketId, UpdateTicketRequest updateRequest);

        Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence);
        [Obsolete("Instead use Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)")]
        Task<bool> TicketOccurrenceExists(Guid templateId, Guid assetId, int occurrence);

        Task<TicketAttachment> CreateAttachment(TicketAttachment attachment);
        Task<bool> DeleteAttachment(Guid ticketId, Guid attachmentId);

        Task<Comment> CreateComment(Comment comment);
        Task<bool> DeleteComment(Guid siteId, Guid ticketId, Guid commentId);

        Task<List<Reporter>> GetReporters(Guid siteId);
        Task<Reporter> GetReporter(Guid siteId, Guid reporterId);
        Task CreateReporter(Reporter reporter);
        Task UpdateReporter(Reporter reporter);
        Task<bool> DeleteReporter(Guid siteId, Guid reporterId);

        Task<List<NotificationReceiver>> GetNotificationReceivers(Guid siteId);

        Task<List<Workgroup>> GetWorkgroups(Guid siteId, bool includeMemberIds);
        Task<List<Workgroup>> GetWorkgroups(string siteName, bool includeMemberIds);
        Task<Workgroup> GetWorkgroup(Guid siteId, Guid workgroupId, bool includeMemberIds);
        Task CreateWorkgroup(Workgroup workgroup);
        Task UpdateWorkgroup(Workgroup workgroup);
        Task<bool> DeleteWorkgroup(Guid siteId, Guid workgroupId);
        Task UpdateWorkgroupMembers(Guid workgroupId, IList<Guid> memberIds);

        Task<List<SiteStatistics>> GetSiteStatisticsList(IList<Guid> siteIds);
        Task<SiteStatistics> GetSiteStatistics(Guid siteId, string floorId);

		Task<List<InsightStatistics>> GetInsightStatisticsList(IList<Guid> insightIds, IList<int> statuses, bool? isScheduled);
        Task<List<InsightStatistics>> GetSiteInsightStatisticsList(IList<Guid> siteIds, IList<int> statuses, bool? isScheduled);

        Task<CheckRecord> GetCheckRecord(Guid checkRecordId);
        Task<List<TicketStatus>> GetTicketStatuses(Guid customerId);
        Task<List<TicketStatus>> CreateOrUpdateTicketStatuses(Guid customerId, List<TicketStatus> ticketStatuses);

        Task<bool> HasInsightOpenTicketsAsync(Guid insightId, Guid? ticketId = null);
		Task<List<TicketActivity>> GetInsightTicketCommentsAsync(Guid insightId);
        Task CreateTicketsAsync(List<Ticket> ticketList);
        Task<List<TicketEntity>> GetPagedTicketsWithIssueIdAndNoTwinIdAsync(int pageNumber, int batchSize);
        Task<List<TicketTemplateEntity>> GetPagedTicketTemplatesWithAssetsAsync(int pageNumber, int batchSize);
        Task UpdateEntitiesAsync<T>(IEnumerable<T> entities);
        Task<List<SiteTicketStatisticsByStatus>> GetSiteTicketStatisticsByStatus(List<Guid> siteIds);
        Task<List<SiteStatistics>> GetSiteStatistics(List<Guid> siteIds);
        Task<List<CategoryCountDto>> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId);
        Task<TicketCountsByDateDto> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate);
    }

    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDirectoryApiService _directoryApiService;
        private readonly IMarketPlaceApiService _marketplaceApiService;
        private readonly ITicketStatusTransitionsService _ticketStatusTransitionsService;
        private readonly ITicketStatusService _ticketStatusService;
        private readonly IAppCache _appCache;
        public WorkflowRepository(WorkflowContext context,
                                  IDateTimeService dateTimeService,
                                  IDirectoryApiService directoryApiService,
                                  IMarketPlaceApiService marketplaceApiService,
                                  ITicketStatusTransitionsService ticketStatusTransitionsService,
                                  ITicketStatusService ticketStatusService,
                                  IAppCache appCache)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _directoryApiService = directoryApiService;
            _marketplaceApiService = marketplaceApiService;
            _ticketStatusTransitionsService = ticketStatusTransitionsService;
            _ticketStatusService = ticketStatusService;
            _appCache = appCache;
        }

        /// <summary>
        /// For the provided twins, and sourceTypes, only consider the OPEN tickets, take the ticket count and include the highest priority ticket
        /// </summary>
        /// <param name="twinIds">Twin to query</param>
        /// <param name="sourceTypes">The source of the ticket</param>
        /// <returns>Ticket count and highest priority ticket</returns>
        public async Task<List<TwinTicketStatisticsDto>> GetTicketStatisticsByTwinIds(IList<string> twinIds, List<SourceType> sourceTypes)
        {
            var openTicketStatuses = await _ticketStatusService.GetOpenedStatus();

            var query = _context.Tickets
                .Where(x => twinIds.Contains(x.TwinId)
                && (openTicketStatuses.Contains(x.Status)) && x.Occurrence == 0);

           if(sourceTypes!=null && sourceTypes.Any())
               query=query.Where(x=>sourceTypes.Contains(x.SourceType));

            return await  query.GroupBy(x => x.TwinId)
                .Select(g => new TwinTicketStatisticsDto
                {
                    TwinId = g.Key,
                    TicketCount = g.Count(),
                    HighestPriority = g.Min(x => x.Priority)
                })
                .ToListAsync();
        }

        /// <summary>
        /// For the supplied twins, group the OPEN tickets (not all tickets) by status.
        /// </summary>
        /// <param name="twinIds">Twin to query</param>
        /// <param name="sourceTypes">The source of the ticket</param>
        /// <returns>The number of ticket in each status</returns>
        /// <remarks>Statistics generation is likely to change soon in terms of TicketTabs and Ticket.Status (dynamic) </remarks>
        public async Task<List<TwinTicketStatisticsByStatus>> GetTicketStatusStatisticsByTwinIds(IList<string> twinIds, List<SourceType> sourceTypes)
        {
            var closedStatus = await _ticketStatusService.GetClosedStatus();
            var resolvedStatus = await _ticketStatusService.GetResolvedStatus();
            var openStatus = await _ticketStatusService.GetOpenedStatus();

            var query = _context.Tickets
                .Where(x => twinIds.Contains(x.TwinId));

            if (sourceTypes != null && sourceTypes.Count != 0)
            {
                query = query.Where(x => sourceTypes.Contains(x.SourceType));
            }

            return await query.GroupBy(x => x.TwinId)
                .Select(g => new TwinTicketStatisticsByStatus
                {
                    TwinId = g.Key,
                    OpenCount = openStatus.Count > 0 ? g.Count(x => openStatus.Contains(x.Status)) : 0,
                    ResolvedCount = resolvedStatus.Count > 0 ? g.Count(x => resolvedStatus.Contains(x.Status)) : 0,
                    ClosedCount = closedStatus.Count > 0 ? g.Count(x => closedStatus.Contains(x.Status)) : 0
                })
                .ToListAsync();
        }

        public async Task<List<TicketCategory>> GetTicketCategories(Guid siteId)
        {
            var ticketCategories = await _context.TicketCategories
                                                .Where(x => x.SiteId == siteId || x.SiteId == null)
                                                .OrderBy(x => x.Name)
                                                .ToListAsync();
            return TicketCategoryEntity.MapToModels(ticketCategories);
        }

        public async Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            var ticketCategory = await _context.TicketCategories.Where(x => x.Id == ticketCategoryId).FirstOrDefaultAsync();
            if (ticketCategory == null)
            {
                throw new NotFoundException(new { TicketCategoryId = ticketCategoryId });
            }
            return TicketCategoryEntity.MapToModel(ticketCategory);
        }

        public async Task CreateTicketCategory(Guid siteId, TicketCategory ticketCategory)
        {
            var entity = new TicketCategoryEntity
            {
                Id = ticketCategory.Id,
                SiteId = siteId,
                Name = ticketCategory.Name
            };
            _context.TicketCategories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest request)
        {
            var ticketCategory = await _context.TicketCategories
                                            .AsTracking()
                                            .Where(x => x.Id == ticketCategoryId && x.SiteId == siteId)
                                            .FirstOrDefaultAsync();
            if (ticketCategory == null)
            {
                throw new NotFoundException(new { TicketCategoryId = ticketCategoryId });
            }

            ticketCategory.Name = request.Name;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            var ticketCategory = await _context.TicketCategories
                                            .AsTracking()
                                            .Where(x => x.Id == ticketCategoryId && x.SiteId == siteId)
                                            .FirstOrDefaultAsync();
            if (ticketCategory == null)
            {
                throw new NotFoundException(new { TicketCategoryId = ticketCategoryId });
            }
            _context.TicketCategories.Remove(ticketCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<Ticket> GetTicket(Guid ticketId, bool includeAttachments, bool includeComments)
        {
            var query = _context.Tickets.Where(x => x.Id == ticketId);
            if (includeAttachments)
            {
                query = query.Include(x => x.Attachments);
            }
            if (includeComments)
            {
                query = query.Include(x => x.Comments);
            }
            var ticketEntity = await query.Include(x => x.Category)
                                          .Include(x => x.Tasks)
                                          .Include(x => x.Diagnostics)
                                          .FirstOrDefaultAsync();

            if (ticketEntity == null)
                return null;

            if (ticketEntity.AssigneeName == null)
            {
                ticketEntity.AssigneeName = await GetTicketAssigneeName(ticketEntity.SiteId, ticketEntity.AssigneeType, ticketEntity.AssigneeId, useDefault: true);
            }

            if (ticketEntity.SourceName == null)
            {
                ticketEntity.SourceName = await GetTicketSourceName(ticketEntity.SourceType, ticketEntity.SourceId);
            }

            ticketEntity.Tasks = ticketEntity.Tasks.OrderBy(t => t.Order).ToList();
            var ticket = TicketEntity.MapToModel(ticketEntity);
            ticket.NextValidStatus = await _ticketStatusTransitionsService.GetNextValidStatusAsync(ticket.Status);
            return ticket;
        }

        public async Task<Ticket> GetTicketBySequenceNumber(String sequenceNumber)
        {
            var query = _context.Tickets.Where(x => x.SequenceNumber == sequenceNumber);
            var ticketEntity = await query.Include(x => x.Category).FirstOrDefaultAsync();

            if (ticketEntity == null)
                return null;

            if (ticketEntity.AssigneeName == null)
            {
                ticketEntity.AssigneeName = await GetTicketAssigneeName(ticketEntity.SiteId, ticketEntity.AssigneeType, ticketEntity.AssigneeId, useDefault: true);
            }

            if (ticketEntity.SourceName == null)
            {
                ticketEntity.SourceName = await GetTicketSourceName(ticketEntity.SourceType, ticketEntity.SourceId);
            }

            return TicketEntity.MapToModel(ticketEntity);
        }

        public async Task<bool> GetTicketExistence(Guid siteId, Guid ticketId, bool isTemplate)
        {
            return await _context.Tickets.AnyAsync(t => t.Id == ticketId && t.SiteId == siteId);
        }

        public async Task<List<Ticket>> GetSiteTickets(Guid siteId, GetSiteTicketsRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.OrderBy))
            {
                // This is needed only temporary until all AssigneeNames and SourceNames are set
                await UpdateTicketsAssigneeName(siteId);
                await UpdateTicketsSourceName(siteId);
            }

            var site = await _directoryApiService.GetSite(siteId);

            var query = _context.Tickets.Where(x => x.SiteId == siteId);

            if (request.IsScheduled)
            {
                if (site.Features.IsScheduledTicketsEnabled)
                {
                    query = query.Where(x => x.Occurrence != 0);
                }
                else
                {
                    return new List<Ticket>();
                }  
            }
            else
            {
                query = query.Where(x => x.Occurrence == 0);
            }

            if (request.Statuses != null && request.Statuses.Count > 0)
            {
                query = query.Where(x => request.Statuses.Select(s => (int)s).Contains(x.Status));
            }
            if (request.IssueType.HasValue)
            {
                query = query.Where(x => x.IssueId == request.IssueId);
            }
            if (request.InsightId.HasValue)
            {
                query = query.Where(x => x.InsightId == request.InsightId);
            }
            if (request.AssigneeId.HasValue)
            {
                query = query.Where(x => x.AssigneeId.HasValue && x.AssigneeId.Value == request.AssigneeId.Value);
            }
            if (request.Unassigned.HasValue)
            {
                if (request.Unassigned.Value)
                {
                    query = query.Where(x => x.AssigneeType == AssigneeType.NoAssignee);
                }
                else
                {
                    query = query.Where(x => x.AssigneeType != AssigneeType.NoAssignee);
                }
            }
            if (!string.IsNullOrEmpty(request.ExternalId))
            {
                query = query.Where(x => x.ExternalId == request.ExternalId);
            }
            if (!string.IsNullOrWhiteSpace(request.FloorId))
            {
                query = query.Where(x => x.FloorCode == request.FloorId);
            }
            if (request.SourceId.HasValue)
            {
                query = query.Where(x => x.SourceId.HasValue && x.SourceId.Value == request.SourceId.Value);
            }
            if (request.SourceType.HasValue)
            {
                query = query.Where(x => x.SourceType == request.SourceType.Value);
            }
            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(x => request.CreatedAfter.Value < x.CreatedDate);
            }
            if (request.IsScheduled && site.Features.IsScheduledTicketsEnabled)
            {
                query = query.Include(x => x.Tasks);
            }

            query = query.Include(x => x.Category);

            if (!string.IsNullOrWhiteSpace(request.OrderBy))
            {
                query = query.OrderBy(request.OrderBy
                    .Replace("CreatedDate", "ComputedCreatedDate")
                    .Replace("UpdatedDate", "ComputedUpdatedDate")
                    .Replace("AssignedTo", "AssigneeName")
                    .Replace("Category", "Category.Name"));
            }

            if (request.Page > 0)
            {
                query = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);
            }

            var tickets = await query.ToListAsync();
            return TicketEntity.MapToModels(tickets);
        }

        public async Task<int> GetTicketsCount(Guid siteId, IList<int> statuses, bool isScheduled)
        {
            var query = _context.Tickets.Where(x => x.SiteId == siteId && (isScheduled ? x.Occurrence != 0 : x.Occurrence == 0));
            if (statuses != null && statuses.Count > 0)
            {
                query = query.Where(x => statuses.Contains(x.Status));
            }

            return await query.CountAsync();
        }

        public async Task<List<InsightStatistics>> GetSiteInsightStatisticsList(IList<Guid> siteIds, IList<int> statuses, bool? isScheduled)
        {
            var query = _context.Tickets.Where(x => siteIds.Contains(x.SiteId) && x.InsightId.HasValue);

            if (isScheduled.HasValue)
            {
                query = query.Where(x => isScheduled.Value ? x.Occurrence != 0 : x.Occurrence == 0);
            }

            if (statuses != null && statuses.Count > 0)
            {
                query = query.Where(x => statuses.Contains(x.Status));
            }

            return await query.GroupBy(x => x.InsightId)
                .Select(g => new InsightStatistics
                {
                    Id = g.Key.Value,
                    OverdueCount = g.Sum(x => x.DueDate.HasValue && x.DueDate.Value < _dateTimeService.UtcNow.Date ? 1 : 0),
                    ScheduledCount = g.Sum(x => x.Occurrence != 0 ? 1 : 0),
                    TotalCount = g.Sum(x => 1),
                })
                .ToListAsync();
        }

        public async Task<List<InsightStatistics>> GetInsightStatisticsList(IList<Guid> insightIds, IList<int> statuses, bool? isScheduled)
		{
			var query = _context.Tickets.Where(x => x.InsightId.HasValue && insightIds.Contains(x.InsightId.Value));

			if (isScheduled.HasValue)
			{
				query = query.Where(x => isScheduled.Value ? x.Occurrence != 0 : x.Occurrence == 0);
			}

			if (statuses != null && statuses.Count > 0)
			{
				query = query.Where(x => statuses.Contains(x.Status));
			}

			var insightStatisticsList = await query.GroupBy(x => x.InsightId)
										.Select(g => new InsightStatistics
										{
											Id = g.Key.Value,
											OverdueCount = g.Sum(x => x.DueDate.HasValue && x.DueDate.Value < _dateTimeService.UtcNow.Date ? 1 : 0),
											ScheduledCount = g.Sum(x => x.Occurrence != 0 ? 1 : 0),
											TotalCount = g.Sum(x => 1),
										})
										.ToListAsync();

			foreach (var insightId in insightIds)
			{
				if (!insightStatisticsList.Any(x => x.Id == insightId))
				{
					insightStatisticsList.Add(new InsightStatistics { Id = insightId });
				}
			}
			return insightStatisticsList;
		}

        public async Task CreateTicketsAsync(List<Ticket> ticketList)
        {
            foreach (var ticket in ticketList)
            {
                var ticketEntity = await EnrichTheTicket(ticket);
                _context.Tickets.Add(ticketEntity);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<long> GenerateSequenceNumber(string sequenceNumberPrefix)
        {
            var ticketSequenceNumber = await _context.TicketNextNumbers.AsTracking().FirstOrDefaultAsync(x => x.Prefix == sequenceNumberPrefix);
            if (ticketSequenceNumber == null)
            {
                ticketSequenceNumber = new TicketNextNumberEntity
                {
                    Prefix = sequenceNumberPrefix,
                    NextNumber = 1
                };
                _context.TicketNextNumbers.Add(ticketSequenceNumber);
            }
            var sequenceNumber = ticketSequenceNumber.NextNumber;
            ticketSequenceNumber.NextNumber++;
            await _context.SaveChangesAsync();
            return sequenceNumber;
        }

        public async Task<long> GenerateSequenceNumber(string sequenceNumberPrefix, string suffix)
        {
            var ticketSequenceNumber = (await _context.TicketSequenceNumbers.FromSqlRaw("exec dbo.GetTicketSequenceNumber {0}, {1}", sequenceNumberPrefix, suffix).ToListAsync()).SingleOrDefault();
            
            return ticketSequenceNumber?.NextNumber ?? 1;
        }

        public async Task<List<TicketEntity>> GetPagedTicketsWithIssueIdAndNoTwinIdAsync(int pageNumber, int batchSize)
        {
            return await _context.Tickets.OrderBy(c => c.Id).Where(c => c.TwinId == null && c.IssueId.HasValue).Skip((pageNumber - 1) * batchSize)
                .Take(batchSize).ToListAsync();
        }

        public async Task<List<TicketTemplateEntity>> GetPagedTicketTemplatesWithAssetsAsync(int pageNumber, int batchSize)
        {
            return await _context.TicketTemplates.OrderBy(c => c.Id).Where(c => c.Assets != null).Skip((pageNumber - 1) * batchSize)
                .Take(batchSize).ToListAsync();
        }

        public async Task UpdateEntitiesAsync<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        private async Task<TicketEntity> EnrichTheTicket(Ticket ticket)
        {
            var ticketEntity = TicketEntity.MapFromModel(ticket);

            ticketEntity.AssigneeName = await GetTicketAssigneeName(ticketEntity.SiteId, ticketEntity.AssigneeType, ticketEntity.AssigneeId, throwOnNull: true);
            ticketEntity.SourceName = await GetTicketSourceName(ticketEntity.SourceType, ticketEntity.SourceId);
            return ticketEntity;
        }
        private class TicketNextNumberError
        {
			public long   ErrorNumber      { get; set; }
		  	public long   ErrorSeverity    { get; set; } 
		  	public long   ErrorState       { get; set; }
		  	public string ErrorProcedure   { get; set; }
		  	public long   ErrorLine        { get; set; }
		  	public string ErrorMessage     { get; set; }			  
        }

        public async Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)
        {
            return await _context.Tickets.AnyAsync(t => t.TemplateId == templateId && t.TwinId == twinId && t.IssueType == IssueType.Asset && t.Occurrence == occurrence);
        }

        [Obsolete("Instead use Task<bool> TicketOccurrenceExists(Guid templateId, string twinId, int occurrence)")]
        public async Task<bool> TicketOccurrenceExists(Guid templateId, Guid assetId, int occurrence)
        {
            return await _context.Tickets.AnyAsync(t => t.TemplateId == templateId && t.IssueId == assetId && t.IssueType == IssueType.Asset && t.Occurrence == occurrence);
        }

        public async Task UpdateTicket(Guid ticketId, UpdateTicketRequest updateRequest)
        {
            var ticket = await _context.Tickets
                .AsTracking()
                .Include(x => x.Tasks)
                .FirstAsync(t => t.Id == ticketId);

            if (updateRequest.Priority.HasValue)
            {
                ticket.Priority = updateRequest.Priority.Value;
            }

            if (updateRequest.Status.HasValue && updateRequest.Status.Value != ticket.Status)
            {
                var closedStatus = await _ticketStatusService.GetClosedStatus();

                var isValidTransition = await _ticketStatusTransitionsService.IsValidStatusTransitionAsync(ticket.Status, updateRequest.Status.Value);
                if (!isValidTransition)
                {
                    throw new ArgumentException($"Invalid status transition from {(TicketStatusEnum)ticket.Status} to {(TicketStatusEnum)updateRequest.Status.Value}");
                }
                ticket.Status = updateRequest.Status.Value;               
                if (updateRequest.Status == (int)TicketStatusEnum.InProgress)
                {
                    ticket.StartedDate = _dateTimeService.UtcNow;
                }
                else if (updateRequest.Status == (int)TicketStatusEnum.Resolved)
                {
                    ticket.ResolvedDate = _dateTimeService.UtcNow;
                }
                else if (closedStatus.Contains((int)updateRequest.Status))
                {
                    ticket.ClosedDate = _dateTimeService.UtcNow;
                }
            }

            if (updateRequest.FloorCode != null)
            {
                ticket.FloorCode = updateRequest.FloorCode;
            }

            if (updateRequest.IssueType.HasValue)
            {
                ticket.IssueType = updateRequest.IssueType.Value;
                ticket.IssueId = updateRequest.IssueId;
                ticket.IssueName = updateRequest.IssueName;
            }

            if (updateRequest.Summary != null)
            {
                ticket.Summary = updateRequest.Summary;
            }

            if (updateRequest.Description != null)
            {
                ticket.Description = updateRequest.Description;
            }

            if (updateRequest.Cause != null)
            {
                ticket.Cause = updateRequest.Cause;
            }

            if (updateRequest.Solution != null)
            {
                ticket.Solution = updateRequest.Solution;
            }

            if (updateRequest.ShouldUpdateReporterId)
            {
                ticket.ReporterId = updateRequest.ReporterId;
            }

            if (updateRequest.ReporterName != null)
            {
                ticket.ReporterName = updateRequest.ReporterName;
            }

            if (updateRequest.ReporterPhone != null)
            {
                ticket.ReporterPhone = updateRequest.ReporterPhone;
            }

            if (updateRequest.ReporterEmail != null)
            {
                ticket.ReporterEmail = updateRequest.ReporterEmail;
            }

            if (updateRequest.ReporterCompany != null)
            {
                ticket.ReporterCompany = updateRequest.ReporterCompany;
            }

            if (updateRequest.AssigneeType.HasValue)
            {
                ticket.AssigneeType = updateRequest.AssigneeType.Value;
                ticket.AssigneeId = updateRequest.AssigneeId;
                ticket.AssigneeName = await GetTicketAssigneeName(ticket.SiteId, updateRequest.AssigneeType.Value, updateRequest.AssigneeId, throwOnNull: true);
            }

            if (ticket.TemplateId == null && updateRequest.DueDate.HasValue)
            {
                ticket.DueDate = updateRequest.DueDate.Value;
            }

            ticket.ExternalCreatedDate = updateRequest.ExternalCreatedDate;
            ticket.ExternalUpdatedDate = updateRequest.ExternalUpdatedDate;
            ticket.LastUpdatedByExternalSource = updateRequest.LastUpdatedByExternalSource;

            if (updateRequest.ExternalMetadata != null)
            {
                ticket.ExternalMetadata = updateRequest.ExternalMetadata;
            }

            if (updateRequest.CustomProperties != null)
            {
                ticket.CustomProperties = JsonConvert.SerializeObject(updateRequest.CustomProperties, JsonSettings.CaseSensitive);
            }

            if (updateRequest.ExtendableSearchablePropertyKeys != null)
            {
                ticket.ExtendableSearchablePropertyKeys = JsonConvert.SerializeObject(updateRequest.ExtendableSearchablePropertyKeys, JsonSettings.CaseSensitive);
            }

            if (updateRequest.Latitude.HasValue)
            {
                ticket.Latitude = updateRequest.Latitude;
            }

            if (updateRequest.Longitude.HasValue)
            {
                ticket.Longitude = updateRequest.Longitude;
            }

            if (updateRequest.Tasks != null && updateRequest.Tasks.Any())
            {
                var taskOrder = 0;
                var tasksToBeRemoved = ticket.Tasks.Where(x => !updateRequest.Tasks.Any(y => y.Id == x.Id)).ToList();
                var tasksToBeAdded = updateRequest.Tasks.Where(x => !ticket.Tasks.Any(y => y.Id == x.Id)).ToList();
                var tasksToBeUpdated = ticket.Tasks.Where(x => updateRequest.Tasks.Any(y => y.Id == x.Id)).OrderBy(y => y.Order).ToList();
                tasksToBeUpdated.ForEach(x =>
                {
                    var task = updateRequest.Tasks.Find(t => t.Id == x.Id);
                    x.IsCompleted = task.IsCompleted;
                    x.TaskName = task.TaskName;
                    x.Order = ++taskOrder;
                    x.Type = task.Type;
                    x.DecimalPlaces = task.DecimalPlaces;
                    x.MinValue = task.MinValue;
                    x.MaxValue = task.MaxValue;
                    x.Unit = task.Unit;
                    x.NumberValue = task.NumberValue;
                });
                var taskEntitiesToBeAdded = TicketTaskEntity.MapFromModels(tasksToBeAdded);
                taskEntitiesToBeAdded.ForEach(x =>
                {
                    x.Id = Guid.NewGuid();
                    x.TicketId = ticketId;
                    x.Order = ++taskOrder;
                });
                await _context.TicketTasks.AddRangeAsync(taskEntitiesToBeAdded);
                _context.TicketTasks.RemoveRange(tasksToBeRemoved);
                _context.UpdateRange(tasksToBeUpdated);
            }

            if (!string.IsNullOrEmpty(updateRequest.Notes))
            {
                ticket.Notes = updateRequest.Notes;
            }

            if (updateRequest.TwinId is not null)
            {
                ticket.TwinId = updateRequest.TwinId;
            }
            if (updateRequest.SubStatusId.HasValue)
            {
                ticket.SubStatusId = updateRequest.SubStatusId.Value;
            }
            if (updateRequest.SpaceTwinId is not null)
            {
                ticket.SpaceTwinId = updateRequest.SpaceTwinId;
            }
            if (updateRequest.JobTypeId is not null)
            {
                ticket.JobTypeId = updateRequest.JobTypeId;
            }
            if (updateRequest.ServiceNeededId is not null)
            {
                ticket.ServiceNeededId = updateRequest.ServiceNeededId;
            }

            ticket.CategoryId = updateRequest.CategoryId;
            ticket.UpdatedDate = _dateTimeService.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<TicketAttachment> CreateAttachment(TicketAttachment attachment)
        {
            var attachmentEntity = AttachmentEntity.MapFromModel(attachment);
            _context.Attachments.Add(attachmentEntity);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<bool> DeleteAttachment(Guid ticketId, Guid attachmentId)
        {
            var attachment = await _context.Attachments.SingleOrDefaultAsync(x => x.Id == attachmentId && x.TicketId == ticketId);
            if (attachment == null)
            {
                return false;
            }
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Comment> CreateComment(Comment comment)
        {
            var commentEntity = CommentEntity.MapFromModel(comment);
            _context.Comments.Add(commentEntity);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteComment(Guid siteId, Guid ticketId, Guid commentId)
        {
            var comment = await _context.Comments.SingleOrDefaultAsync(x => x.Id == commentId && x.TicketId == ticketId);
            if (comment == null)
            {
                return false;
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Reporter>> GetReporters(Guid siteId)
        {
            var reporterEntities = await _context.Reporters.Where(x => x.SiteId == siteId).ToListAsync();
            return ReporterEntity.MapToModels(reporterEntities);
        }

        public async Task<Reporter> GetReporter(Guid siteId, Guid reporterId)
        {
            var reporterEntity = await _context.Reporters.FirstOrDefaultAsync(x => x.Id == reporterId && x.SiteId == siteId);
            if (reporterEntity == null)
            {
                return null;
            }
            return ReporterEntity.MapToModel(reporterEntity);
        }

        public async Task CreateReporter(Reporter reporter)
        {
            var entity = ReporterEntity.MapFromModel(reporter);
            _context.Reporters.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReporter(Reporter reporter)
        {
            var entity = ReporterEntity.MapFromModel(reporter);
            _context.Reporters.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteReporter(Guid siteId, Guid reporterId)
        {
            var reporterEntity = await _context.Reporters.FirstOrDefaultAsync(x => x.SiteId == siteId && x.Id == reporterId);
            if (reporterEntity == null)
            {
                return false;
            }
            _context.Reporters.Remove(reporterEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<NotificationReceiver>> GetNotificationReceivers(Guid siteId)
        {
            var receiverEntities = await _context.NotificationReceivers.Where(x => x.SiteId == siteId).ToListAsync();
            return NotificationReceiverEntity.MapToModels(receiverEntities);
        }

        public async Task CreateWorkgroup(Workgroup workgroup)
        {
            var entity = WorkgroupEntity.MapFromModel(workgroup);
            _context.Workgroups.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateWorkgroup(Workgroup workgroup)
        {
            var entity = WorkgroupEntity.MapFromModel(workgroup);
            _context.Workgroups.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Workgroup>> GetWorkgroups(Guid siteId, bool includeMemberIds)
        {
            var workgroupEntities = await _context.Workgroups.Where(x => x.SiteId == siteId).ToListAsync();
            var workgroups = WorkgroupEntity.MapToModels(workgroupEntities);
            if (includeMemberIds)
            {
                var workgroupIds = workgroups.Select(x => x.Id).ToList();
                var members = await _context.WorkgroupMembers.Where(x => workgroupIds.Contains(x.WorkgroupId)).ToListAsync();
                foreach (var workgroup in workgroups)
                {
                    workgroup.MemberIds = members.Where(x => x.WorkgroupId == workgroup.Id).Select(x => x.MemberId).ToList();
                }
            }
            return workgroups;
        }

        public async Task<List<Workgroup>> GetWorkgroups(string siteName, bool includeMemberIds)
        {
            var query = _context.Workgroups.AsNoTracking();

            if (!string.IsNullOrEmpty(siteName))
            {
                query = _context.Workgroups.Where(x => x.Name.StartsWith(siteName));
            }

            var workgroupEntities = await query.ToListAsync();
            var workgroups = WorkgroupEntity.MapToModels(workgroupEntities);
            if (includeMemberIds)
            {
                var workgroupIds = workgroups.Select(x => x.Id).ToList();
                var members = await _context.WorkgroupMembers.Where(x => workgroupIds.Contains(x.WorkgroupId)).ToListAsync();
                foreach (var workgroup in workgroups)
                {
                    workgroup.MemberIds = members.Where(x => x.WorkgroupId == workgroup.Id).Select(x => x.MemberId).ToList();
                }
            }
            return workgroups;
        }

        public async Task<Workgroup> GetWorkgroup(Guid siteId, Guid workgroupId, bool includeMemberIds)
       {
            var workgroupEntity = await _context.Workgroups.FirstOrDefaultAsync(x => x.Id == workgroupId && (siteId == Guid.Empty || x.SiteId == siteId));
            if (workgroupEntity == null)
            {
                return null;
            }
            var workgroup = WorkgroupEntity.MapToModel(workgroupEntity);
            if (includeMemberIds)
            {
                workgroup.MemberIds = await _context.WorkgroupMembers.Where(x => x.WorkgroupId == workgroupId).Select(x => x.MemberId).ToListAsync();
            }
            return workgroup;
        }

        public async Task<bool> DeleteWorkgroup(Guid siteId, Guid workgroupId)
        {
            var workgroupEntity = await _context.Workgroups.FirstOrDefaultAsync(x => (siteId == Guid.Empty || x.SiteId == siteId) && x.Id == workgroupId);
            if (workgroupEntity == null)
            {
                return false;
            }
            var workgroupMembers = _context.WorkgroupMembers.Where(x => x.WorkgroupId == workgroupId);
            _context.WorkgroupMembers.RemoveRange(workgroupMembers);
            _context.Workgroups.Remove(workgroupEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateWorkgroupMembers(Guid workgroupId, IList<Guid> memberIds)
        {
            var existingMembers = _context.WorkgroupMembers.Where(x => x.WorkgroupId == workgroupId).ToList();
            var membersToBeDeleted = existingMembers.Where(x => !memberIds.Contains(x.MemberId)).ToList();
            var memberIdsToBeAdded = memberIds.Where(x => !existingMembers.Any(m => m.MemberId == x));

            _context.WorkgroupMembers.RemoveRange(membersToBeDeleted);
            _context.WorkgroupMembers.AddRange(memberIdsToBeAdded.Select(id => new WorkgroupMemberEntity { WorkgroupId = workgroupId, MemberId = id }));
            await _context.SaveChangesAsync();
        }

        public async Task<List<SiteStatistics>> GetSiteStatisticsList(IList<Guid> siteIds)
        {
            var openStatus = await _ticketStatusService.GetOpenedStatus();

            var siteStatisticsList = await _context.Tickets.Where(x => siteIds.Contains(x.SiteId) &&
                                            x.Occurrence == 0 &&
                                            openStatus.Contains(x.Status))
                                        .GroupBy(x => x.SiteId)
                                        .Select(g => new SiteStatistics
                                        {
                                            Id = g.Key,
                                            OverdueCount = g.Sum(x => x.DueDate.HasValue && x.DueDate.Value < _dateTimeService.UtcNow.Date ? 1 : 0),
                                            UrgentCount = g.Sum(x => x.Priority == 1 ? 1 : 0),
                                            HighCount = g.Sum(x => x.Priority == 2 ? 1 : 0),
                                            MediumCount = g.Sum(x => x.Priority == 3 ? 1 : 0),
                                            LowCount = g.Sum(x => x.Priority == 4 ? 1 : 0),
                                            OpenCount = g.Sum(x => 1),
                                        })
                                        .ToListAsync();
            foreach (var siteId in siteIds)
            {
                if(!siteStatisticsList.Any(x => x.Id == siteId))
                {
                    siteStatisticsList.Add(new SiteStatistics { Id = siteId });
                }
            }
            return siteStatisticsList;
        }

        public async Task<SiteStatistics> GetSiteStatistics(Guid siteId, string floorId)
        {
            var openStatus = await _ticketStatusService.GetOpenedStatus();
           
            if (string.IsNullOrWhiteSpace(floorId))
            { 
                floorId = null;
            }

            var siteStatistics = _context.Tickets.Where(x => x.SiteId == siteId &&
                                                        x.Occurrence == 0 &&
                                                        openStatus.Contains(x.Status) &&
                                                        (floorId == null || x.FloorCode == floorId))
                                                 .GroupBy(x => x.SiteId)
                                                 .Select(g => new SiteStatistics
                                                 {
                                                     Id = g.Key,
                                                     OverdueCount = g.Sum(x => x.DueDate.HasValue && x.DueDate.Value < _dateTimeService.UtcNow.Date ? 1 : 0),
                                                     UrgentCount = g.Sum(x => x.Priority == 1 ? 1 : 0),
                                                     HighCount = g.Sum(x => x.Priority == 2 ? 1 : 0),
                                                     MediumCount = g.Sum(x => x.Priority == 3 ? 1 : 0),
                                                     LowCount = g.Sum(x => x.Priority == 4 ? 1 : 0),
                                                     OpenCount = g.Sum(x => 1),
                                                 });

            var result = await siteStatistics.SingleOrDefaultAsync();

            return result ?? new SiteStatistics { Id = siteId };
        }

        public async Task<CheckRecord> GetCheckRecord(Guid checkRecordId)
        {
            var checkRecord = await _context.CheckRecords.FindAsync(checkRecordId);
            return CheckRecordEntity.MapToModel(checkRecord);
        }

        private readonly IDictionary<Guid?, string> _cachedAssigneeNames = new Dictionary<Guid?, string>();

        private async Task<string> GetTicketAssigneeName(Guid siteId, AssigneeType type, Guid? assigneeId, bool useDefault = false, bool throwOnNull = false)
        {
            var defaultAssigneeName = useDefault ? "Unassigned" : null;
            var nullAssigneeName = !throwOnNull ? "Unknown" : null;

            if (assigneeId == null)
            {
                return defaultAssigneeName;
            }

            if (_cachedAssigneeNames.ContainsKey(assigneeId))
            {
                return _cachedAssigneeNames[assigneeId];
            }

            string assigneeName = defaultAssigneeName;

            try
            {
                if (type == AssigneeType.CustomerUser)
                {
                    var user = await _directoryApiService.GetUser(assigneeId.Value); 
                    assigneeName = ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim();
                }
                else if (type == AssigneeType.WorkGroup)
                {
                    var workgroup = await GetWorkgroup(siteId, assigneeId.Value, false); 
                    assigneeName = workgroup?.Name ?? nullAssigneeName;
                }
            } 
            catch
            {
                assigneeName = nullAssigneeName;
            }

            if (string.IsNullOrWhiteSpace(assigneeName))
            {
                throw new NotFoundException(new { SiteId = siteId, AssigneeId = assigneeId, AssigneeType = type });
            }

            _cachedAssigneeNames.Add(assigneeId, assigneeName);

            return assigneeName;
        }

        private IDictionary<Guid?, string> _cachedSourceNames = new Dictionary<Guid?, string>();

        private async Task<string> GetTicketSourceName(SourceType sourceType, Guid? sourceId)
        {
            var sourceName = $"{sourceType}";
            if (sourceType == SourceType.App)
            {
                if (sourceId.HasValue && sourceId != Guid.Empty)
                {
                    var appId = sourceId ?? default;

                    if (_cachedSourceNames.ContainsKey(appId))
                    {
                        sourceName = _cachedSourceNames[appId];
                    }
                    else
                    {

                        //var app = await _memoryCache.GetOrCreateAsync(
                        //    $"Apps_${appId}",
                        //    async (entry) =>
                        //    {
                        //        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120);
                        //        return await GetApp(appId);
                        //    }
                        //);

                        var app = await GetApp(appId);
                        sourceName = app?.Name ?? string.Empty;

                        _cachedSourceNames.Add(appId, sourceName);
                    }
                }
                else
                {
                    sourceName = string.Empty;
                }
            }
            return sourceName;
        }

        private async Task<App> GetApp(Guid appId)
        {
            try
            {
                var marketPlaceApp = await _marketplaceApiService.GetApp(appId);
                return marketPlaceApp;
            }
            catch (RestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // The app does not exist any more
                return new App
                {
                    Name = appId.ToString()
                };
            }
        }

        public async Task UpdateTicketsAssigneeName(Guid siteId)
        {
            if (_context.Tickets.Any(x => x.SiteId == siteId && x.AssigneeType > 0 && x.AssigneeId != null && string.IsNullOrWhiteSpace(x.AssigneeName)))
            {
                var assignees = await _context.Tickets.Where(x => x.SiteId == siteId && x.AssigneeType > 0 && x.AssigneeId != null && string.IsNullOrWhiteSpace(x.AssigneeName)).Select(x => new { x.AssigneeId, x.AssigneeType }).Distinct().ToListAsync();

                foreach (var assignee in assignees)
                {
                    var assigneeName = await GetTicketAssigneeName(siteId, assignee.AssigneeType, assignee.AssigneeId);

                    var pageSize = 100;
                    while (_context.Tickets.Any(x => x.SiteId == siteId && x.AssigneeType > 0 && x.AssigneeId == assignee.AssigneeId && string.IsNullOrWhiteSpace(x.AssigneeName)))
                    {
                        var tickets = await _context.Tickets.AsTracking().Where(x => x.SiteId == siteId && x.AssigneeType > 0 && x.AssigneeId == assignee.AssigneeId && string.IsNullOrWhiteSpace(x.AssigneeName)).Take(pageSize).ToListAsync();
                        tickets.ForEach(x => x.AssigneeName = assigneeName);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task UpdateTicketsSourceName(Guid siteId)
        {
            if (_context.Tickets.Any(x => x.SiteId == siteId && x.SourceId != null && string.IsNullOrWhiteSpace(x.SourceName)))
            {
                var sources = await _context.Tickets.Where(x => x.SiteId == siteId && x.SourceId != null && string.IsNullOrWhiteSpace(x.SourceName)).Select(x => new { x.SourceId, x.SourceType }).Distinct().ToListAsync();

                foreach (var source in sources)
                {
                    var sourceName = await GetTicketSourceName(source.SourceType, source.SourceId);

                    var pageSize = 100;
                    while (_context.Tickets.Any(x => x.SiteId == siteId && x.SourceId == source.SourceId && string.IsNullOrWhiteSpace(x.SourceName)))
                    {
                        var tickets = await _context.Tickets.AsTracking().Where(x => x.SiteId == siteId && x.SourceId == source.SourceId && string.IsNullOrWhiteSpace(x.SourceName)).Take(pageSize).ToListAsync();
                        tickets.ForEach(x => x.SourceName = sourceName);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatuses(Guid customerId)
        {
            var ticketStatuses = await _context.TicketStatuses.Where(s => s.CustomerId == customerId).ToListAsync();
            return TicketStatusEntity.MapToModels(ticketStatuses);
        }

        public async Task<List<TicketStatus>> CreateOrUpdateTicketStatuses(Guid customerId, List<TicketStatus> ticketStatuses)
        {
            var existingTicketStatuses = await _context.TicketStatuses.AsTracking()
                                                .Where(s => s.CustomerId == customerId)
                                                .ToListAsync();

            foreach (var ticketStatus in ticketStatuses)
            {
                var toBeUpdatedTicketStatus = existingTicketStatuses.Find(s => s.StatusCode == ticketStatus.StatusCode || 
                                                                                s.Status.Equals(ticketStatus.Status, StringComparison.InvariantCultureIgnoreCase));
                if (toBeUpdatedTicketStatus != null)
                {
					toBeUpdatedTicketStatus.Status = ticketStatus.Status;
                    toBeUpdatedTicketStatus.Tab = ticketStatus.Tab;
                    toBeUpdatedTicketStatus.Color = ticketStatus.Color;
                }
                else
                {
                    _context.TicketStatuses.Add(TicketStatusEntity.MapFromModel(ticketStatus));
                }
            }

            await _context.SaveChangesAsync();
            // invalidate ticket status cache
            _appCache.Remove(CacheKeys.TicketStatusList);
            return _context.TicketStatuses.Where(s => s.CustomerId == customerId).Select(TicketStatusEntity.MapToModel).ToList();
        }

		public async Task<bool> HasInsightOpenTicketsAsync(Guid insightId,Guid? ticketId=null)
		{
            var closedStatus = await _ticketStatusService.GetClosedStatus();
            return await _context.Tickets.AnyAsync(c =>
				c.InsightId != null && c.InsightId.Value == insightId 
				&& (ticketId == null || ticketId.Value!=c.Id)
				&& !closedStatus.Contains(c.Status));
		}

		public async Task<List<TicketActivity>> GetInsightTicketCommentsAsync(Guid insightId)
		{

			var comments = await _context.Tickets.Where(x => x.InsightId.HasValue && x.InsightId == insightId)
												.Include(x => x.Comments)
												.Select(x => x.Comments.Select(c => new 
												{
													c.TicketId,
													c.CreatedDate,
													c.CreatorId,
													c.Text,
                                                    x.Summary
												})).ToListAsync();

			// query separated because of EF Core bug that cause error in unit test
			var insightTicketComments = comments
										.SelectMany(x => x.Select(c => new TicketActivity
										{
											TicketId = c.TicketId,
											ActivityType = TicketActivityType.TicketComment,
											ActivityDate = c.CreatedDate,
											SourceType = SourceType.Platform,
											SourceId = c.CreatorId,
                                            TicketSummary = c.Summary,
											Activities = new()
											{
												new KeyValuePair<string, string>(nameof(Ticket.Comments), c.Text)
											}

										}))
										.ToList();

			return insightTicketComments;
		}

        public async Task<List<SiteTicketStatisticsByStatus>> GetSiteTicketStatisticsByStatus(List<Guid> siteIds)
        {
            var closedStatus = await _ticketStatusService.GetClosedStatus();
            var resolvedStatus = await _ticketStatusService.GetResolvedStatus();
            var openStatus = await _ticketStatusService.GetOpenedStatus();
            var ticketStatistics = await _context.Tickets.Where(x => siteIds.Contains(x.SiteId) &&
                                                        x.Occurrence == 0)
                                                 .GroupBy(x => x.SiteId)
                                                 .Select(g => new SiteTicketStatisticsByStatus
                                                 {
                                                     Id = g.Key,
                                                     OpenCount = openStatus.Count > 0 ? g.Count(x => openStatus.Contains(x.Status)) : 0,
                                                     ResolvedCount = resolvedStatus.Count > 0 ? g.Count(x => resolvedStatus.Contains(x.Status)) : 0,
                                                     ClosedCount = closedStatus.Count > 0 ? g.Count(x => closedStatus.Contains(x.Status)) : 0
                                                 }).ToListAsync();

            // Filter out siteIds that are not already present in ticketStatistics
            var siteIdsToAdd = siteIds.Except(ticketStatistics.Select(x => x.Id));

            // Add new ticketStatistics objects for the remaining siteIds
            ticketStatistics.AddRange(siteIdsToAdd.Select(siteId => new SiteTicketStatisticsByStatus { Id = siteId }));
            return ticketStatistics;
        }

        public async Task<List<SiteStatistics>> GetSiteStatistics(List<Guid> siteIds)
        {
            var openStatus = await _ticketStatusService.GetOpenedStatus();

            var siteStatistics =await _context.Tickets.Where(x =>siteIds.Contains(x.SiteId)&&
                                                             x.Occurrence == 0 &&
                                                             openStatus.Contains(x.Status))
                .GroupBy(x => x.SiteId)
                .Select(g => new SiteStatistics
                {
                    Id = g.Key,
                    OverdueCount = g.Count(x => x.DueDate.HasValue && x.DueDate.Value < _dateTimeService.UtcNow.Date ),
                    UrgentCount = g.Count(x => x.Priority == 1 ),
                    HighCount = g.Count(x => x.Priority == 2 ),
                    MediumCount = g.Count(x => x.Priority == 3),
                    LowCount = g.Count(x => x.Priority == 4),
                    OpenCount = g.Count()
                }).ToListAsync();

            // Filter out siteIds that are not already present in siteStatistics
            var siteIdsToAdd = siteIds.Except(siteStatistics.Select(x => x.Id));

            // Add new siteStatistics objects for the remaining siteIds
            siteStatistics.AddRange(siteIdsToAdd.Select(siteId => new SiteStatistics { Id = siteId }));
            return siteStatistics;
        }

        public async Task<List<CategoryCountDto>> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId)
        {
            var ticketsCategoriesCount = await _context.Tickets
                .Where(x => x.SpaceTwinId == spaceTwinId)
                .GroupBy(x => x.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var ticketCategories = await _context.TicketCategories.ToListAsync();

            var ticketCountsPerCategoryName = ticketsCategoriesCount
                .Select(x => new CategoryCountDto(
                    ticketCategories.FirstOrDefault(c => c.Id == x.CategoryId)?.Name ?? "Unknown",
                    x.Count
                ))
            .ToList();
            return ticketCountsPerCategoryName;
        }

        public async Task<TicketCountsByDateDto> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate)
        {
            var ticketCounts = await _context.Tickets
                .Where(x => x.SpaceTwinId == spaceTwinId && x.CreatedDate >= startDate && x.CreatedDate <= endDate)
                .GroupBy(x => x.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            return new TicketCountsByDateDto { Counts = ticketCounts };
        }
    }
}
