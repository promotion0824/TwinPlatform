using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using MobileXL.Models;
using System.Globalization;
using MobileXL.Services.Apis.WorkflowApi.Requests;
using MobileXL.Services.Apis.WorkflowApi.Responses;
using MobileXL.Features.Inspections.Requests;
using MobileXL.Features.Inspections.Response;
using Willow.Api.Client;
using MobileXL.Dto;
using MobileXL.Services.Apis.DigitalTwinApi;
using MobileXL.Services.Apis.InsightApi;
using MobileXL.Services.Apis.DirectoryApi;
using Willow.Common;

namespace MobileXL.Services.Apis.WorkflowApi
{
	public interface IWorkflowApiService
    {
        Task<Ticket> GetTicket(Guid siteId, Guid ticketId, bool includeComments);
        Task<Ticket> UpdateTicket(Guid siteId, Guid ticketId, WorkflowUpdateTicketRequest updateTicketRequest);
        Task<List<Ticket>> GetSiteTickets(Guid siteId, Guid assigneeId, IEnumerable<int> statuses, bool scheduled, bool isCustomerAdmin);
        Task<List<Ticket>> GetSiteUnassignedTickets(Guid siteId, IEnumerable<int> statuses, bool scheduled);

        Task<Attachment> CreateAttachment(Guid siteId, Guid resourceId, string fileName, Stream fileStream, string resourceType);
        Task DeleteAttachment(Guid siteId, Guid resourceId, Guid attachmentId, string resourceType);

        Task<Comment> CreateComment(Guid siteId, Guid ticketId, WorkflowCreateCommentRequest createCommentRequest);
        Task DeleteComment(Guid siteId, Guid ticketId, Guid commentId);

        Task<List<Guid>> GetNotificationReceiverIds(Guid siteId);

        Task<List<InspectionZone>> GetInspectionZones(Guid siteId, Guid userId);
        Task<InspectionZone> GetInspectionZone(Guid siteId, Guid userId, Guid inspectionZoneId);
        Task<List<Inspection>> GetInspectionsByZoneId(Guid siteId, Guid userId, Guid inspectionZoneId);
        Task<InspectionRecord> GetInspectionLastRecord(Guid siteId, Guid inspectionId);
        Task<WorkflowSubmitCheckRecordResponse> SubmitCheckRecord(Guid siteId, Guid inspectionId, Guid checkRecordId, WorkflowSubmitCheckRecordRequest request);
        Task UpdateCheckRecordInsight(Guid siteId, Guid inspectionId, Guid checkRecordId, Guid insightId);
        Task<List<Workgroup>> GetWorkgroups(Guid siteId);
        Task<InspectionRecordsResponse> SyncInspectionRecords(Models.Site site, Guid userId,string userType, InspectionRecordsRequest request);
        Task<InspectionsDto> GetInspections(Guid userId);
		Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count);
		Task<List<CustomerTicketStatus>> GetCustomerTicketStatus(Guid customerId);
		Task<string> GetUserFullname(Guid userId, string userType, Guid customerId);
    }

    public class WorkflowApiService : IWorkflowApiService
    {
        private readonly HttpClient _client;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IInsightApiService _insightApi;
        private readonly IDigitalTwinApiService _digitalTwinService;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly IUserCache _userCache;
public WorkflowApiService(IHttpClientFactory httpClientFactory,
                                  IDateTimeService dateTimeService,
                                  IDirectoryApiService directoryApi,
                                  IInsightApiService insightApi,
                                  IDigitalTwinApiService digitalTwinService,
                                  IImageUrlHelper imageUrlHelper,
                                  IUserCache userCache)
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.WorkflowCore);
            _dateTimeService = dateTimeService;
            _directoryApi = directoryApi;
            _insightApi = insightApi;
            _digitalTwinService = digitalTwinService;
            _imageUrlHelper = imageUrlHelper;
            _userCache = userCache ;
		}

        public async Task<Ticket> GetTicket(Guid siteId, Guid ticketId, bool includeComments)
        {
            var response = await _client.GetAsync($"sites/{siteId}/tickets/{ticketId}?includeAttachments=True&includeComments={includeComments}");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<Ticket>();
        }

        public async Task<List<Workgroup>> GetWorkgroups(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/workgroups");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<List<Workgroup>>();
        }

        public async Task<Ticket> UpdateTicket(Guid siteId, Guid ticketId, WorkflowUpdateTicketRequest updateTicketRequest)
        {
            var response = await _client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", updateTicketRequest);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<Ticket>();
        }

        public async Task<List<Ticket>> GetSiteTickets(Guid siteId, Guid assigneeId, IEnumerable<int> statuses, bool scheduled, bool isCustomerAdmin)
        {
            var url = $"sites/{siteId}/tickets";
            url = QueryHelpers.AddQueryString(url, "scheduled", scheduled.ToString());
            if (!isCustomerAdmin)
            {
                url = QueryHelpers.AddQueryString(url, "assigneeId", assigneeId.ToString());
            }
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    url = QueryHelpers.AddQueryString(url, "statuses", status.ToString());
                }
            }

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<List<Ticket>>();
        }

        public async Task<List<Ticket>> GetSiteUnassignedTickets(Guid siteId, IEnumerable<int> statuses, bool scheduled)
        {
            var url = $"sites/{siteId}/tickets";
            url = QueryHelpers.AddQueryString(url, "scheduled", scheduled.ToString());
            url = QueryHelpers.AddQueryString(url, "unassigned", true.ToString(CultureInfo.InvariantCulture));
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    url = QueryHelpers.AddQueryString(url, "statuses", status.ToString());
                }
            }

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<List<Ticket>>();
        }

        public async Task<Attachment> CreateAttachment(Guid siteId, Guid resourceId, string fileName, Stream fileStream, string resourceType)
        {
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
            return await CreateAttachment(siteId, resourceId, fileName, fileBytes, resourceType);
        }
        public async Task<Attachment> CreateAttachment(Guid siteId, Guid resourceId, string fileName, byte[] fileBytes, string resourceType)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes)
            {
                Headers = { ContentLength = fileBytes.Length }
            };
            dataContent.Add(fileContent, "attachmentFile", fileName);
            var url = $"sites/{siteId}/";
            if (resourceType == "ticket")
            {
                url += $"tickets/{resourceId}/attachments";
            }
            if (resourceType == "checkRecord")
            {
                url += $"checkRecords/{resourceId}/attachments";
            }

            var response = await _client.PostAsync(url, dataContent);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<Attachment>();
        }
        public async Task DeleteAttachment(Guid siteId, Guid resourceId, Guid attachmentId, string resourceType)
        {
            var url = $"sites/{siteId}/";
            if (resourceType == "ticket")
            {
                url += $"tickets/{resourceId}/attachments/{attachmentId}";
            }
            if (resourceType == "checkRecord")
            {
                url += $"checkRecords/{resourceId}/attachments/{attachmentId}";
            }
            var response = await _client.DeleteAsync(url);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
        }

        public async Task<Comment> CreateComment(Guid siteId, Guid ticketId, WorkflowCreateCommentRequest createCommentRequest)
        {
            var response = await _client.PostAsJsonAsync($"sites/{siteId}/tickets/{ticketId}/comments", createCommentRequest);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<Comment>();
        }

        public async Task DeleteComment(Guid siteId, Guid ticketId, Guid commentId)
        {
            var response = await _client.DeleteAsync($"sites/{siteId}/tickets/{ticketId}/comments/{commentId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
        }

        public async Task<List<InspectionZone>> GetInspectionZones(Guid siteId, Guid userId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/users/{userId}/zones?includeStatistics=true");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<List<InspectionZone>>();
        }

        public async Task<InspectionZone> GetInspectionZone(Guid siteId, Guid userId, Guid inspectionZoneId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/users/{userId}/zones/{inspectionZoneId}?includeStatistics=true");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<InspectionZone>();
        }

        public async Task<List<Inspection>> GetInspectionsByZoneId(Guid siteId, Guid userId, Guid inspectionZoneId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/users/{userId}/zones/{inspectionZoneId}/inspections");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<List<Inspection>>();
        }

        public async Task<InspectionRecord> GetInspectionLastRecord(Guid siteId, Guid inspectionId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            var inspectionRecord = await response.Content.ReadAsAsync<InspectionRecord>();
            var pausedCheckIds = inspectionRecord.Inspection.Checks
                                                            .Where(x => IsCheckPaused(x.PauseStartDate, x.PauseEndDate))
                                                            .Select(x => x.Id)
                                                            .ToList();
            inspectionRecord.Inspection.Checks.RemoveAll(x => pausedCheckIds.Contains(x.Id));
            inspectionRecord.CheckRecords.RemoveAll(x => pausedCheckIds.Contains(x.CheckId));
            return inspectionRecord;
        }

        public async Task<List<Guid>> GetNotificationReceiverIds(Guid siteId)
        {
            var response = await _client.GetAsync($"sites/{siteId}/notificationReceivers");
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            var receivers = await response.Content.ReadAsAsync<List<NotificationReceiver>>();
            return receivers.Select(x => x.UserId).ToList();
        }

        public async Task<WorkflowSubmitCheckRecordResponse> SubmitCheckRecord(Guid siteId, Guid inspectionId, Guid checkRecordId, WorkflowSubmitCheckRecordRequest request)
        {
            var response = await _client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}", request);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<WorkflowSubmitCheckRecordResponse>();
        }

        public async Task UpdateCheckRecordInsight(Guid siteId, Guid inspectionId, Guid checkRecordId, Guid insightId)
        {
            var response = await _client.PutAsJsonAsync(
                $"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}/insight",
                new UpdateCheckRecordInsightRequest { InsightId = insightId });
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
        }

        public async Task<WorkflowSubmitCheckRecordResponse> UpdateCheckRecord(Guid siteId, Guid inspectionId, Guid inspectionRecordId,
            Guid checkRecordId, WorkflowSubmitCheckRecordRequest request)
        {
            var response = await _client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}/{inspectionRecordId}/checkRecords/{checkRecordId}", request);
            response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
            return await response.Content.ReadAsAsync<WorkflowSubmitCheckRecordResponse>();
        }

        public async Task<InspectionRecordsResponse> SyncInspectionRecords(Models.Site site, Guid userId,string userType, InspectionRecordsRequest request)
        {
            var inspectionRecords = new List<InspectionRecordResponse>();
			var userFullname=await GetUserFullname(userId,userType,site.CustomerId);
			
			foreach (var inspectionRecordRequest in request.InspectionRecords)
            {
                var inspectionRecordResponse = new InspectionRecordResponse();
                inspectionRecordResponse.Id = inspectionRecordRequest.Id;
                var checkRecords = new List<CheckRecordResponse>();

                foreach (var checkRecordRequest in inspectionRecordRequest.CheckRecords)
                {
                    var checkRecordResponse = new CheckRecordResponse();
                    checkRecordResponse.Id = checkRecordRequest.Id;

                    try
                    {
                        var response = await UpdateCheckRecord(site.Id, inspectionRecordRequest.InspectionId,
                                                                inspectionRecordRequest.Id, checkRecordRequest.Id,
                            new WorkflowSubmitCheckRecordRequest
                            {
                                Notes = checkRecordRequest.Notes,
                                NumberValue = checkRecordRequest.NumberValue,
                                StringValue = checkRecordRequest.StringValue,
                                DateValue = checkRecordRequest.DateValue,
                                SubmittedUserId = userId,
								SubmittedUserFullname = userFullname,
                                TimeZoneId = site.TimeZoneId,
                                EnteredAt = checkRecordRequest.EnteredAt
                            });

                        if (response.RequiredInsight != null && !string.IsNullOrEmpty(response.RequiredInsight.TwinId))
                        {

                            var customer = await _directoryApi.GetCustomer(site.CustomerId);
                            var asset = await _digitalTwinService.GetAssetAsync(site.Id, response.RequiredInsight.TwinId);
                            var createInsightRequest = new CreateInsightCoreRequest
                            {
                                CustomerId = site.CustomerId,
                                SequenceNumberPrefix = site.Code,
                                TwinId = response.RequiredInsight.TwinId,
                                Type = response.RequiredInsight.Type,
                                Name = response.RequiredInsight.Name,
                                Description = (response.RequiredInsight.Description ?? string.Empty) + $"\r\nAsset: {asset?.Name}",
                                Priority = response.RequiredInsight.Priority,
                                State = InsightState.Active,
                                OccurredDate = _dateTimeService.UtcNow,
                                DetectedDate = _dateTimeService.UtcNow,
                                SourceType = InsightSourceType.Inspection,
                                SourceId = null,
                                ExternalId = string.Empty,
                                ExternalStatus = string.Empty,
                                ExternalMetadata = string.Empty,
                                OccurrenceCount = 1,
                                AnalyticsProperties = new Dictionary<string, string>
                                {
                                    { "Site", site.Name },
                                    { "Company", customer.Name }
                                },
								CreatedUserId = userId
                            };
                            var createdInsight = await _insightApi.CreateInsight(site.Id, createInsightRequest);
                            await UpdateCheckRecordInsight(site.Id, inspectionRecordRequest.Id, checkRecordRequest.Id, createdInsight.Id);
                        }

                        checkRecordResponse.Result = "Success";
                    }
                    catch (RestException ex)
                    {
                        checkRecordResponse.Result = "Error";
                        checkRecordResponse.Message = ex.Message;
                    }

                    checkRecords.Add(checkRecordResponse);
                }

                inspectionRecordResponse.CheckRecords = checkRecords;
                inspectionRecords.Add(inspectionRecordResponse);
            }

            return new InspectionRecordsResponse { InspectionRecords = inspectionRecords };
        }

		public async Task<List<CustomerTicketStatus>> GetCustomerTicketStatus(Guid customerId)
		{
			var response = await _client.GetAsync($"customers/{customerId}/ticketstatus");
			response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
			return await response.Content.ReadAsAsync<List<CustomerTicketStatus>>();
		}

		public async Task<string> GetUserFullname(Guid userId, string userType,Guid customerId)
		{
			if (userType.Equals(UserTypeNames.CustomerUser, StringComparison.InvariantCultureIgnoreCase))
			{
				var customerUser = await _userCache.GetCustomerUser(customerId, userId,returnNullIfNotFound:true);
				return customerUser != null ? $"{customerUser.FirstName} {customerUser.LastName}" : null;
			}
			return null;
		}
		private class NotificationReceiver
        {
            public Guid UserId { get; set; }
        }

        public class UpdateCheckRecordInsightRequest
        {
            public Guid InsightId { get; set; }
        }

        public async Task<InspectionsDto> GetInspections(Guid userId)
        {
            var inspectionsDto = new InspectionsDto();
            var siteDtos = new List<SiteDto>();
            var sites = await _directoryApi.GetUserSites(userId, Permissions.ViewSites);
            foreach (var site in sites)
            {
                var inspectionZones = await GetInspectionZones(site.Id, userId);
                var siteDto = SiteDto.Map(site);
                siteDto.InspectionZones = ZoneDto.Map(inspectionZones);
                foreach (var inspectionZone in siteDto.InspectionZones)
                {
                    var inspections = await GetInspectionsByZoneId(site.Id, userId, inspectionZone.Id);
                    inspectionZone.Inspections = InspectionDto.Map(inspections, _imageUrlHelper).ToList();
                    foreach (var inspection in inspectionZone.Inspections)
                    {
                        var inspectionRecord = InspectionRecordDto.Map(await GetInspectionLastRecord(site.Id, inspection.Id), _imageUrlHelper);
                        inspectionRecord.Inspection = null;
                        inspection.InspectionRecords = new List<InspectionRecordDto>() { inspectionRecord };
                    }
                }
                siteDtos.Add(siteDto);
            }
            inspectionsDto.Sites = siteDtos;
            return inspectionsDto;
        }

        private bool IsCheckPaused(DateTime? startDate, DateTime? endDate)
        {
            var utcNow = _dateTimeService.UtcNow;
            switch (startDate.HasValue, endDate.HasValue)
            {
                case (false, false):
                    return false;
                case (true, true):
                    return utcNow.CompareTo(startDate) >= 0 && utcNow.CompareTo(endDate) <= 0;
                case (true, false):
                    return utcNow.CompareTo(startDate) >= 0;
                default: // false, true
                    return false;
            }
        }

		public async Task<List<CheckRecord>> GetCheckSubmittedHistory(Guid siteId, Guid inspectionId, Guid checkId, int count)
		{
			var response = await _client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory/{count}");
			response.EnsureSuccessStatusCode(ApiServiceNames.WorkflowCore);
			return await response.Content.ReadAsAsync<List<CheckRecord>>();
		}
	}
}
