using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.ZendeskApi;
using Willow.Batch;
using Willow.Platform.Models;
using PlatformPortalXL.Features.ContactUs;
using PlatformPortalXL.Infrastructure.AppSettingOptions;

namespace PlatformPortalXL.Services.ContactUs;
public interface IContactUsService
{
    Task CreateSupportTicket(CreateSupportTicketRequest request, IFormFileCollection attachmentFiles,Site site, Guid userId);
}
public class ContactUsService : IContactUsService
{
    private readonly IDirectoryApiService _directoryApiService;
    private readonly IInsightApiService _insightApiService;
    private readonly IZendeskApiService _zendeskApiService;
    private readonly IOptions<AppSettings> _options;
    private readonly ILogger<ContactUsService> _logger;
    public ContactUsService(IInsightApiService insightApiService, IZendeskApiService zendeskApiService, IDirectoryApiService directoryApiService, IOptions<AppSettings> options, ILogger<ContactUsService> logger)
    {
        _zendeskApiService = zendeskApiService;
        _logger = logger;
        _insightApiService = insightApiService;
        _options = options;
        _directoryApiService = directoryApiService;
    }
    public async Task CreateSupportTicket(CreateSupportTicketRequest request, IFormFileCollection attachmentFiles, Site site, Guid userId)
    {
        var customer = await _directoryApiService.GetCustomer(site.CustomerId);
        Insight[] requestedInsights = null;
        if (request.InsightIds != null && request.InsightIds.Any())
        {
            var insightBatchRequest = new BatchRequestDto
            {
                FilterSpecifications = (new List<FilterSpecificationDto>()
                    .Upsert(nameof(Insight.Id), request.InsightIds)).ToArray()
            };
              requestedInsights = (await _insightApiService.GetInsights(insightBatchRequest))?.Items;
        }
        try
        {
            var uploadResponse = await _zendeskApiService.UploadAttachmentsAsync(attachmentFiles);
            var zendeskTicketRequest = MapToCreateTicketRequest(request, site, customer, requestedInsights, uploadResponse);
            await _zendeskApiService.CreateTicket(zendeskTicketRequest);
            await SetInsightsToReported(requestedInsights, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError("unable to create Zendesk ticket", ex);
            throw;
        }
    }

    private async Task SetInsightsToReported(Insight[] requestedInsights, Guid currentUserId)
    {
        if (requestedInsights == null || !requestedInsights.Any())
            return;
        var updateRequest = new UpdateInsightRequest
        {
            Reported = true,
            UpdatedByUserId = currentUserId
        };

        var insightUpdateTasks =
            requestedInsights.Select(x => _insightApiService.UpdateInsightAsync(x.SiteId, x.Id, updateRequest));
        await Task.WhenAll(insightUpdateTasks);
    }


    private ZendeskCreateTicketRequest MapToCreateTicketRequest(CreateSupportTicketRequest request, Site site, Customer customer, Insight[] requestedInsights, List<ZendeskUpload> uploadResponse)
    {
        var zendeskRequest = new ZendeskCreateTicketRequest
        {
            Comment = new ZendeskTicketComment
            {
                Body = request.CommentBody,
                Uploads = uploadResponse != null && uploadResponse.Any() ? uploadResponse.Select(c=>c.Token).ToList(): null
            },
            Priority = ZendeskTicketPriority.Normal,
            Subject = request.Subject,
            Status = ZendeskTicketStatus.Open,
            CustomFields = GetZendeskCustomFields(request, site, customer),
            GroupId = _options.Value.ZendeskOptions.GroupId,
            Requester = new ZendeskTicketRequester
            {
                Email = _options.Value.ZendeskOptions.AuthUsername,
                Name = _options.Value.ZendeskOptions.RequesterName
            }
        };
        if (requestedInsights != null && requestedInsights.Any())
        {
            zendeskRequest.Comment.Body += GetInsightsBody(requestedInsights);
        }
        return zendeskRequest;
    }

    private string GetInsightsBody(Insight[] requestedInsights)
    {
        string insightBody = null;
        foreach (var insight in requestedInsights)
        {
            insightBody ??= "\n\rInsight List:";
            insightBody += $"\n\r Id: {insight.Id} - Name: {insight.Name}\n\r url: {_options.Value.CommandPortalBaseUrl}/insights/{insight.Id}\n\r";
        }
        return insightBody;
    }

    private List<ZendeskCustomField> GetZendeskCustomFields(CreateSupportTicketRequest request, Site site, Customer customer)
    {

        var customFields = new List<ZendeskCustomField>
        {
            new()
            {
                Id = 360029547271,
                Value = customer.Name.Replace(' ', '_')
            },
            new()
            {
                Id = 4413864107161,
                Value = $"{customer.Name.Replace(' ', '_')}__{site.Name.Replace(' ', '_')}"
            },
            new()
            {
                Id = 360029506971,
                Value = request.Category.GetDescription()
            },
            new()
            {
                Id = 360029506771,
                Value = ToProductArea(request.Category).GetDescription()
            },
            new()
            {
                Id = 7231972707215,
                Value = ZendeskTicketSeverityLevel.Informational.GetDescription()
            },
            new()
            {
                Id = 900007384566,
                Value = ZendeskTicketPriority.Normal.ToString()
            }
        };
       
        return customFields;
    }

    static ContactUsProductArea ToProductArea( ContactUsCategory category)
    {
        switch (category)
        {
            case ContactUsCategory.Copilot:
                return ContactUsProductArea.CoreServices;
            case ContactUsCategory.Insights:
                return ContactUsProductArea.RulesAndInsights;
            case ContactUsCategory.Marketplace:
                return ContactUsProductArea.Connectors;
            case ContactUsCategory.TimeSeries:
                return ContactUsProductArea.LiveDate;
            case ContactUsCategory.Admin:
                return ContactUsProductArea.CoreServices;
            case ContactUsCategory.Dashboards:
                return ContactUsProductArea.Dashboards;
            case ContactUsCategory.Inspections:
                return ContactUsProductArea.Inspections;
            case ContactUsCategory.Tickets:
                return ContactUsProductArea.Workflows;
            case ContactUsCategory.SearchAndExplore:
                return ContactUsProductArea.SearchAndExplore;
            default:
                return ContactUsProductArea.RulesAndInsights;
        }
    }
}
