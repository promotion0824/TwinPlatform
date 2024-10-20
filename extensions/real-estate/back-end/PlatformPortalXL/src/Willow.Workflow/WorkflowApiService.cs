using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Willow.Api.Client;
using Willow.Platform.Users;
using Willow.Workflow.Models;
using Willow.Workflow.Requests;

namespace Willow.Workflow
{
    public interface IWorkflowApiService
    {
        Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task<List<TicketCategory>> GetTicketCategories(Guid siteId);
        Task<TicketCategory> CreateTicketCategory(Guid siteId, CreateTicketCategoryRequest createTicketCategoryRequest);
        Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId);
        Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest updateTicketCategoryRequest);
        Task<Ticket> GetTicket(Guid ticketId, bool includeComments);
        Task<Ticket> GetTicket(Guid siteId, Guid ticketId, bool includeComments);
        Task<List<Ticket>> GetTickets(Guid siteId, IEnumerable<int> statuses, TicketIssueType? issueType, Guid? issueId, Guid? insightId, bool? scheduled, string floorId=null, string orderBy = null, int page = 0, int pageSize = 0);
        Task<int> GetTotalTicketsCount(Guid siteId, IEnumerable<int> statuses, bool scheduled);
        Task<Ticket> CreateTicket(Guid siteId, WorkflowCreateTicketRequest createTicketRequest);
        Task<Ticket> UpdateTicket(Guid siteId, Guid ticketId, WorkflowUpdateTicketRequest updateTicketRequest);

		Task<Attachment> CreateAttachment(CreateStreamAttachmentDto createStreamAttachmentDto);

		Task<Attachment> CreateAttachment(CreateByteAttachmentDto createByteAttachmentDto);

		Task DeleteAttachment(Guid siteId, Guid ticketId, Guid attachmentId);

        Task<Comment> CreateComment(Guid siteId, Guid ticketId, WorkflowCreateCommentRequest createCommentRequest);
        Task DeleteComment(Guid siteId, Guid ticketId, Guid commentId);

        Task<List<Reporter>> GetReporters(Guid siteId);
        Task<Reporter> CreateReporter(Guid siteId, WorkflowCreateReporterRequest createReporterRequet);
        Task<Reporter> UpdateReporter(Guid siteId, Guid reporterId, UpdateReporterRequest updateReporterRequet);
        Task DeleteReporter(Guid siteId, Guid reporterId);

        Task<List<Workgroup>> GetWorkgroups(Guid siteId);
        Task<List<Workgroup>> GetWorkgroups(string siteName);
        Task<Workgroup> CreateWorkgroup(Guid siteId, CreateWorkgroupRequest createWorkgroupRequest);
        Task<Workgroup> UpdateWorkgroup(Guid siteId, Guid workgroupId, UpdateWorkgroupRequest updateWorkgroupRequest);
        Task DeleteWorkgroup(Guid siteId, Guid workgroupId);

        Task<InspectionZone> CreateInspectionZone(Guid siteId, CreateInspectionZoneRequest createInspectionZoneRequest);
        Task<List<InspectionZone>> GetInspectionZones(Guid siteId, bool? includeStatistics);
        Task<List<Inspection>> GetInspectionsByZone(Guid siteId, Guid inspectionZoneId);
        Task UpdateInspectionZone(Guid siteId, Guid inspectionZoneId, UpdateInspectionZoneRequest updateInspectionZoneRequest);

        Task<Inspection> CreateInspection(Guid siteId, CreateInspectionRequest createInspectionRequest);
        Task<List<Inspection>> GetSiteInspections(Guid siteId);
        Task<Inspection> UpdateInspection(Guid siteId, Guid inspectionId, UpdateInspectionRequest updateInspectionRequest);
        Task<Inspection> GetInspection(Guid siteId, Guid inspectionId);
        Task<InspectionUsage> GetInspectionUsageBySiteId(Guid siteId, InspectionUsagePeriod period);
        Task<List<CheckRecordReport>> GetCheckHistory(Guid siteId, Guid inspectionId, Guid customerId, Guid? checkId,
            DateTime startDate, DateTime endDate);
        Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived);
        Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived);
        Task UpdateInspectionSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request);
        Task<Inspection> GetInspection(Guid inspectionId);
        Task<SiteSettings> GetSiteSettings(Guid siteId);
        Task<SiteSettings> UpsertSiteSettings(Guid siteId, WorkflowApiUpsertSiteSettingsRequest request);
        Task<List<SiteTicketStatistics>> GetSiteStatistics(List<Guid> siteIds);
		Task<List<InsightTicketStatistics>> GetInsightStatistics(List<Guid> insightIds, List<int>statuses = null, bool? scheduled = null);
        Task<List<InsightTicketStatistics>> GetSiteInsightStatistics(List<Guid> siteIds, List<int> statuses = null, bool? scheduled = null);

        Task<List<TicketTemplate>> GetTicketTemplates(Guid siteId, bool? archived);
        Task<TicketTemplate> GetTicketTemplate(Guid siteId, Guid ticketTemplateId);
        Task<TicketTemplate> CreateTicketTemplate(Guid siteId, WorkflowCreateTicketTemplateRequest request);
        Task<TicketTemplate> UpdateTicketTemplate(Guid siteId, Guid ticketTemplateId, WorkflowUpdateTicketTemplateRequest request);
        Task<List<CustomerTicketStatus>> GetCustomerTicketStatus(Guid customerId);
        Task<List<CustomerTicketStatus>> CreateOrUpdateTicketStatus(Guid customerId, WorkflowCreateTicketStatusRequest request);
		Task<List<Inspection>> CreateInspections(Guid siteId, CreateInspectionsRequest createInspectionsRequest);
		Task<List<InsightTicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId);
        Task<List<SimpleInspectionZone>> GetInspectionZones(List<Guid> siteIds);
        Task<TicketCategoricalData> GetTicketCategoricalData();
        Task<TicketStatisticsResponse> GetTicketStatisticsBySiteIdsAsync(List<Guid> siteIds);
        Task<List<TicketSubStatus>> GetTicketSubStatus();
        Task<List<TwinTicketStatisticsDto>> GetTwinsTicketStatistics(TwinTicketStatisticApiRequest request);
        Task<TicketAssigneesData> GetTicketPossibleAssignees(Guid siteId);
        Task<List<TwinTicketStatisticsByStatus>> GetTwinsTicketStatisticsByStatus(TwinTicketStatisticApiRequest request);
        /// <summary>
        /// Get the ticket category counts in order descending for a given space twin Id based the limit number of categories
        /// and sum of the rest of the categories as other
        /// </summary>
        /// <param name="twinId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<TicketCategoryCountResponse> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId, int? limit);
        /// <summary>
        /// Get the count of created ticket of each day within a specified date range for a given space twin ID.
        /// </summary>
        /// <param name="spaceTwinId">The ID of the space twin.</param>
        /// <param name="startDate">The start date of the date range.</param>
        /// <param name="endDate">The end date of the date range.</param>
        /// <returns>The count of created tickets of each day within a specified date range..</returns>
        Task<TicketCountsByDateResponse> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Get the ticket status transitions.
        /// </summary>
        /// <returns></returns>
        Task<TicketStatusTransitionsResponse> GetTicketStatusTransitionsAsync();
    }

    public class WorkflowApiService : IWorkflowApiService
    {
        private readonly IRestApi _workflowApi;

        public WorkflowApiService(IRestApi workflowApi)
        {
            _workflowApi = workflowApi;
        }

        #region TicketCategory

        public Task<List<TicketCategory>> GetTicketCategories(Guid siteId)
        {
            return _workflowApi.Get<List<TicketCategory>>($"sites/{siteId}/tickets/categories");
        }

        public Task<TicketCategory> GetTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            return _workflowApi.Get<TicketCategory>($"sites/{siteId}/tickets/categories/{ticketCategoryId}");
        }

        public Task<TicketCategory> CreateTicketCategory(Guid siteId, CreateTicketCategoryRequest createTicketCategoryRequest)
        {
            return _workflowApi.Post<CreateTicketCategoryRequest, TicketCategory>($"sites/{siteId}/tickets/categories", createTicketCategoryRequest);
        }

        public Task DeleteTicketCategory(Guid siteId, Guid ticketCategoryId)
        {
            return _workflowApi.Delete($"sites/{siteId}/tickets/categories/{ticketCategoryId}");
        }

        public Task UpdateTicketCategory(Guid siteId, Guid ticketCategoryId, UpdateTicketCategoryRequest updateTicketCategoryRequest)
        {
            return _workflowApi.PutCommand<UpdateTicketCategoryRequest>($"sites/{siteId}/tickets/categories/{ticketCategoryId}", updateTicketCategoryRequest);
        }

        #endregion

        #region Ticket

        public Task<Ticket> GetTicket(Guid siteId, Guid ticketId, bool includeComments)
        {
            return _workflowApi.Get<Ticket>($"sites/{siteId}/tickets/{ticketId}?includeAttachments=True&includeComments={includeComments}");
        }
        public Task<Ticket> GetTicket(Guid ticketId, bool includeComments)
        {
            return _workflowApi.Get<Ticket>($"tickets/{ticketId}?includeAttachments=True&includeComments={includeComments}");
        }
        public Task<List<Ticket>> GetTickets(Guid siteId, IEnumerable<int> statuses, TicketIssueType? issueType, Guid? issueId, Guid? insightId, bool? scheduled, string floorId=null, string orderBy = null, int page = 0, int pageSize = 0)
        {
            var url = $"sites/{siteId}/tickets";
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    url = QueryHelpers.AddQueryString(url, "statuses", status.ToString());
                }
            }
            if (issueType.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "issueType", issueType.Value.ToString());
                url = QueryHelpers.AddQueryString(url, "issueId", issueId.Value.ToString());
            }
            if (insightId.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "insightId", insightId.Value.ToString());
            }
            if (scheduled.HasValue && scheduled.Value)
            {
                url = QueryHelpers.AddQueryString(url, "scheduled", scheduled.Value.ToString());
            }
            if (!string.IsNullOrEmpty(floorId))
            {
                url = QueryHelpers.AddQueryString(url, "floorId", floorId);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                url = QueryHelpers.AddQueryString(url, "orderBy", orderBy);
            }
            if (page > 0)
            {
                url = QueryHelpers.AddQueryString(url, "page", page.ToString());
            }
            if (pageSize > 0)
            {
                url = QueryHelpers.AddQueryString(url, "pageSize", pageSize.ToString());
            }

            return _workflowApi.Get<List<Ticket>>(url);
        }

        public Task<int> GetTotalTicketsCount(Guid siteId, IEnumerable<int> statuses, bool scheduled)
        {
            var url = $"sites/{siteId}/tickets/count";
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    url = QueryHelpers.AddQueryString(url, "statuses", status.ToString());
                }
            }

            if (scheduled)
            {
                url = QueryHelpers.AddQueryString(url, "scheduled", scheduled.ToString());
            }

            return _workflowApi.Get<int>(url);
        }

        public Task<Ticket> CreateTicket(Guid siteId, WorkflowCreateTicketRequest createTicketRequest)
        {
            return _workflowApi.Post<WorkflowCreateTicketRequest, Ticket>($"sites/{siteId}/tickets", createTicketRequest);
        }

        public Task<Ticket> UpdateTicket(Guid siteId, Guid ticketId, WorkflowUpdateTicketRequest updateTicketRequest)
        {
            return _workflowApi.Put<WorkflowUpdateTicketRequest, Ticket>($"sites/{siteId}/tickets/{ticketId}", updateTicketRequest);
        }

        public Task<TicketCategoricalData> GetTicketCategoricalData()
        {
            var url = $"api/mapped/categoricalData";
            return _workflowApi.Get<TicketCategoricalData>(url);
        }
        #endregion

        #region Attachment

        public async Task<Attachment> CreateAttachment(CreateStreamAttachmentDto createStreamAttachmentDto)
        {
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
				createStreamAttachmentDto.FileStream.CopyTo(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
			var attachmentDto = CreateByteAttachmentDto.MapFrom(createStreamAttachmentDto, fileBytes);

			return await CreateAttachment(attachmentDto);
        }

        public Task<Attachment> CreateAttachment(CreateByteAttachmentDto createByteAttachmentDto)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(createByteAttachmentDto.FileBytes)
            {
                Headers = { ContentLength = createByteAttachmentDto.FileBytes.Length }
            };
            dataContent.Add(fileContent, "attachmentFile", createByteAttachmentDto.FileName);
			dataContent.Add(new StringContent(createByteAttachmentDto.SourceId.ToString()), nameof(createByteAttachmentDto.SourceId));
			dataContent.Add(new StringContent(createByteAttachmentDto.SourceType.ToString()), nameof(createByteAttachmentDto.SourceType));
			return _workflowApi.Post<MultipartFormDataContent, Attachment>($"sites/{createByteAttachmentDto.SiteId}/tickets/{createByteAttachmentDto.TicketId}/attachments", dataContent);
        }

        public Task DeleteAttachment(Guid siteId, Guid ticketId, Guid attachmentId)
        {
            return _workflowApi.Delete($"sites/{siteId}/tickets/{ticketId}/attachments/{attachmentId}");
        }

        #endregion

        #region Comment

        public Task<Comment> CreateComment(Guid siteId, Guid ticketId, WorkflowCreateCommentRequest createCommentRequest)
        {
            return _workflowApi.Post<WorkflowCreateCommentRequest, Comment>($"sites/{siteId}/tickets/{ticketId}/comments", createCommentRequest);
        }

        public Task DeleteComment(Guid siteId, Guid ticketId, Guid commentId)
        {
            return _workflowApi.Delete($"sites/{siteId}/tickets/{ticketId}/comments/{commentId}");
        }

        #endregion

        #region Reporter

        public Task<List<Reporter>> GetReporters(Guid siteId)
        {
            return _workflowApi.Get<List<Reporter>>($"sites/{siteId}/reporters");
        }

        public Task<Reporter> CreateReporter(Guid siteId, WorkflowCreateReporterRequest createReporterRequet)
        {
            return _workflowApi.Post<WorkflowCreateReporterRequest, Reporter>($"sites/{siteId}/reporters", createReporterRequet);
        }

        public Task<Reporter> UpdateReporter(Guid siteId, Guid reporterId, UpdateReporterRequest updateReporterRequet)
        {
            return _workflowApi.Put<UpdateReporterRequest, Reporter>($"sites/{siteId}/reporters/{reporterId}", updateReporterRequet);
        }

        public Task DeleteReporter(Guid siteId, Guid reporterId)
        {
            return _workflowApi.Delete($"sites/{siteId}/reporters/{reporterId}");
        }

        #endregion

        #region Workgroup

        public Task<List<Workgroup>> GetWorkgroups(Guid siteId)
        {
            return _workflowApi.Get<List<Workgroup>>($"sites/{siteId}/workgroups");
        }

        public Task<List<Workgroup>> GetWorkgroups(string siteName)
        {
            return _workflowApi.Get<List<Workgroup>>($"workgroups/all/{siteName}");
        }

        public Task<Workgroup> CreateWorkgroup(Guid siteId, CreateWorkgroupRequest createWorkgroupRequest)
        {
            return _workflowApi.Post<CreateWorkgroupRequest, Workgroup>($"sites/{siteId}/workgroups", createWorkgroupRequest);
        }

        public Task<Workgroup> UpdateWorkgroup(Guid siteId, Guid workgroupId, UpdateWorkgroupRequest updateWorkgroupRequest)
        {
            return _workflowApi.Put<UpdateWorkgroupRequest, Workgroup>($"sites/{siteId}/workgroups/{workgroupId}", updateWorkgroupRequest);
        }

        public Task DeleteWorkgroup(Guid siteId, Guid workgroupId)
        {
            return _workflowApi.Delete($"sites/{siteId}/workgroups/{workgroupId}");
        }

        #endregion

        #region Inspection

        public Task<InspectionZone> CreateInspectionZone(Guid siteId, CreateInspectionZoneRequest createInspectionZoneRequest)
        {
            return _workflowApi.Post<CreateInspectionZoneRequest, InspectionZone>($"sites/{siteId}/zones", createInspectionZoneRequest);
        }

        public Task UpdateInspectionZone(Guid siteId, Guid inspectionZoneId, UpdateInspectionZoneRequest updateInspectionZoneRequest)
        {
            return _workflowApi.PutCommand<UpdateInspectionZoneRequest>($"sites/{siteId}/zones/{inspectionZoneId}", updateInspectionZoneRequest);
        }

        public Task<List<InspectionZone>> GetInspectionZones(Guid siteId, bool? includeStatistics)
        {
            var url = $"sites/{siteId}/zones";
            if (includeStatistics.HasValue)
            {
                url = QueryHelpers.AddQueryString(url, "includeStatistics", includeStatistics.Value.ToString(CultureInfo.InvariantCulture));
            }

            return _workflowApi.Get<List<InspectionZone>>(url);
        }

        public Task<List<SimpleInspectionZone>> GetInspectionZones(List<Guid> siteIds)
        {
            return _workflowApi.Post<List<Guid>,List <SimpleInspectionZone>>("zones/bySiteIds",siteIds);
        }

        public Task<List<Inspection>> GetInspectionsByZone(Guid siteId, Guid inspectionZoneId)
        {
            return _workflowApi.Get<List<Inspection>>($"sites/{siteId}/zones/{inspectionZoneId}/inspections");
        }

        public Task<Inspection> CreateInspection(Guid siteId, CreateInspectionRequest createInspectionRequest)
        {
            return _workflowApi.Post<CreateInspectionRequest, Inspection>($"sites/{siteId}/inspections", createInspectionRequest);
        }
		/// <summary>
		/// create bulk inspections
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="createInspectionsRequest"></param>
		/// <returns></returns>
		public Task<List<Inspection>> CreateInspections(Guid siteId, CreateInspectionsRequest createInspectionsRequest)
		{
			return _workflowApi.Post<CreateInspectionsRequest, List<Inspection>>($"sites/{siteId}/inspections/batch-create", createInspectionsRequest);
		}

		public Task<List<Inspection>> GetSiteInspections(Guid siteId)
        {
            return _workflowApi.Get<List<Inspection>>($"sites/{siteId}/inspections");
        }

        public Task<Inspection> UpdateInspection(Guid siteId, Guid inspectionId, UpdateInspectionRequest updateInspectionRequest)
        {
            return _workflowApi.Put<UpdateInspectionRequest, Inspection>($"sites/{siteId}/inspections/{inspectionId}", updateInspectionRequest);
        }

        public Task<Inspection> GetInspection(Guid siteId, Guid inspectionId)
        {
            return _workflowApi.Get<Inspection>($"sites/{siteId}/inspections/{inspectionId}");
        }

        public Task<Inspection> GetInspection(Guid inspectionId)
        {
            return _workflowApi.Get<Inspection>($"inspections/{inspectionId}");
        }

        public Task<InspectionUsage> GetInspectionUsageBySiteId(Guid siteId, InspectionUsagePeriod period)
        {
            return _workflowApi.Get<InspectionUsage>($"sites/{siteId}/inspectionUsage?inspectionUsagePeriod={period}");
        }

        public Task<List<CheckRecordReport>> GetCheckHistory(Guid siteId, Guid inspectionId,Guid customerId, Guid? checkId, DateTime startDate, DateTime endDate)
        {
            var startString = HttpUtility.UrlEncode(startDate.ToString("O", CultureInfo.InvariantCulture));
            var endString = HttpUtility.UrlEncode(endDate.ToString("O", CultureInfo.InvariantCulture));
            var url =
                $"inspections/{inspectionId}/checks/history?siteId={siteId}&customerId={customerId}&startDate={startString}&endDate={endString}";

            return _workflowApi.Get<List<CheckRecordReport>>(checkId.HasValue?$"{url}&checkId={checkId.Value}":url);
        }

        public Task ArchiveInspection(Guid siteId, Guid inspectionId, bool isArchived)
        {
            return _workflowApi.PostCommand<string>($"sites/{siteId}/inspections/{inspectionId}/archive?isArchived={isArchived}", null);
        }

        public Task ArchiveZone(Guid siteId, Guid zoneId, bool isArchived)
        {
            return _workflowApi.PostCommand<string>($"sites/{siteId}/zones/{zoneId}/archive?isArchived={isArchived}", null);
        }

        public Task UpdateInspectionSortOrder(Guid siteId, Guid zoneId, UpdateInspectionSortOrderRequest request)
        {
            return _workflowApi.PutCommand<UpdateInspectionSortOrderRequest>($"sites/{siteId}/zones/{zoneId}/inspections/sortOrder", request);
        }

        #endregion

        public Task<SiteSettings> GetSiteSettings(Guid siteId)
        {
            return _workflowApi.Get<SiteSettings>($"sites/{siteId}/settings");
        }

        public Task<SiteSettings> UpsertSiteSettings(Guid siteId, WorkflowApiUpsertSiteSettingsRequest request)
        {
            return _workflowApi.Put<WorkflowApiUpsertSiteSettingsRequest, SiteSettings>($"sites/{siteId}/settings", request);
        }

        public Task<List<SiteTicketStatistics>> GetSiteStatistics(List<Guid> siteIds)
        {
            var url = $"siteStatistics";
            foreach(var siteId in siteIds)
            {
                url = QueryHelpers.AddQueryString(url, "siteIds", siteId.ToString());
            }

            return _workflowApi.Get<List<SiteTicketStatistics>>(url);
        }

		public Task<List<InsightTicketStatistics>> GetInsightStatistics(List<Guid> insightIds, List<int> statuses = null, bool? scheduled = null)
		{
			return _workflowApi.Post<dynamic, List<InsightTicketStatistics>>("insightStatistics", new
			{
				InsightIds = insightIds,
				Statuses = statuses,
				Scheduled = scheduled
			});
		}

        public Task<List<InsightTicketStatistics>> GetSiteInsightStatistics(List<Guid> siteIds, List<int> statuses = null, bool? scheduled = null)
        {
            return _workflowApi.Post<dynamic, List<InsightTicketStatistics>>("siteinsightStatistics", new
            {
                SiteIds = siteIds,
                Statuses = statuses,
                Scheduled = scheduled
            });
        }

        public async Task<List<InsightTicketActivity>> GetInsightTicketActivitiesAsync(Guid insightId)
		{
			return await _workflowApi.Get<List<InsightTicketActivity>>($"insights/{insightId}/tickets/activities");
		}

        public Task<TicketStatisticsResponse> GetTicketStatisticsBySiteIdsAsync(List<Guid> siteIds)
        {
            return _workflowApi.Post<List<Guid>, TicketStatisticsResponse>("tickets/statistics", siteIds);
        }
		#region TicketTemplate

		public Task<List<TicketTemplate>> GetTicketTemplates(Guid siteId, bool? archived)
        {
            var url = archived.HasValue ? $"sites/{siteId}/tickettemplate?archived={archived}" : $"sites/{siteId}/tickettemplate";

            return _workflowApi.Get<List<TicketTemplate>>(url);
        }

        public Task<TicketTemplate> GetTicketTemplate(Guid siteId, Guid ticketTemplateId)
        {
            return _workflowApi.Get<TicketTemplate>($"sites/{siteId}/tickettemplate/{ticketTemplateId}");
        }

        public Task<TicketTemplate> CreateTicketTemplate(Guid siteId, WorkflowCreateTicketTemplateRequest request)
        {
            return _workflowApi.Post<WorkflowCreateTicketTemplateRequest, TicketTemplate>($"sites/{siteId}/tickettemplate", request);
        }

        public Task<TicketTemplate> UpdateTicketTemplate(Guid siteId, Guid ticketTemplateId, WorkflowUpdateTicketTemplateRequest request)
        {
            return _workflowApi.Put<WorkflowUpdateTicketTemplateRequest, TicketTemplate>($"sites/{siteId}/tickettemplate/{ticketTemplateId}", request);
        }
        #endregion

        #region TicketStatus

        public Task<List<CustomerTicketStatus>> GetCustomerTicketStatus(Guid customerId)
        {
            return _workflowApi.Get<List<CustomerTicketStatus>>($"customers/{customerId}/ticketstatus");
        }

        public Task<List<CustomerTicketStatus>> CreateOrUpdateTicketStatus(Guid customerId, WorkflowCreateTicketStatusRequest request)
        {
            return _workflowApi.Post<WorkflowCreateTicketStatusRequest,List<CustomerTicketStatus>>($"customers/{customerId}/ticketstatus", request);
        }

        public Task<List<TicketSubStatus>> GetTicketSubStatus()
        {
            return _workflowApi.Get<List<TicketSubStatus>>("ticketsSubStatus");
        }
        #endregion

        public Task<List<TwinTicketStatisticsDto>> GetTwinsTicketStatistics(TwinTicketStatisticApiRequest request)
        {
            return _workflowApi.Post<TwinTicketStatisticApiRequest, List<TwinTicketStatisticsDto>>("tickets/twins/statistics", request);
        }

        public Task<TicketAssigneesData> GetTicketPossibleAssignees(Guid siteId)
        {
            return _workflowApi.Get<TicketAssigneesData>($"sites/{siteId}/possibleTicketAssignees");
        }


        /// <summary>
        /// Returns ticket status statistics by twinIds
        /// </summary>
        /// <param name="request">Twins and source types</param>
        /// <returns>Ticket status statistics by twinIds</returns>
        public Task<List<TwinTicketStatisticsByStatus>> GetTwinsTicketStatisticsByStatus(TwinTicketStatisticApiRequest request)
        {
            return _workflowApi.Post<TwinTicketStatisticApiRequest, List<TwinTicketStatisticsByStatus>>("tickets/twins/statistics/status", request);
        }

        public Task<TicketCategoryCountResponse> GetTicketCategoryCountBySpaceTwinId(string spaceTwinId, int? limit)
        {
            var url = $"tickets/twins/{spaceTwinId}/ticketCountsByCategory";
            if (limit.HasValue)
            {
                url = $"{url}?limit={limit}";
            }

            return _workflowApi.Get<TicketCategoryCountResponse>(url);
           
        }

        public Task<TicketCountsByDateResponse> GetTicketsCountsByCreatedDate(string spaceTwinId, DateTime startDate, DateTime endDate)
        {
            var url = $"tickets/twins/{spaceTwinId}/ticketCountsByDate";
            url = $"{url}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            return _workflowApi.Get<TicketCountsByDateResponse>(url);

        }

        public Task<TicketStatusTransitionsResponse> GetTicketStatusTransitionsAsync()
        {
            var url = "tickets/statusTransitions";
            return _workflowApi.Get<TicketStatusTransitionsResponse>(url);
        }
        private class NotificationReceiver
        {
            public Guid UserId { get; set; }
        }
    }
}
