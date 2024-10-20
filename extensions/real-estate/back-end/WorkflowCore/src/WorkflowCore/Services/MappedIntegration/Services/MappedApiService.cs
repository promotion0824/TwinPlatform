using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using WorkflowCore.Services.MappedIntegration.Interfaces;

namespace WorkflowCore.Services.MappedIntegration.Services;

public class MappedApiService : IMappedApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MappedService> _logger;
    private readonly IMappedIdentityService _mappedIdentityService;
    private readonly WorkflowContext _workflowContext;

    public MappedApiService(IHttpClientFactory httpClientFactory,
                            ILogger<MappedService> logger,
                            IMappedIdentityService mappedIdentityService,
                            WorkflowContext workflowContext)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _mappedIdentityService = mappedIdentityService;
        _workflowContext = workflowContext;
    }

    public async Task SendTicketDataAsync(MappedIntegrationConfiguration mappedConfiguration, MappedTicketEventDto ticketEvent)
    {
        ValidateConfigurationProperty(mappedConfiguration.WebhookUrl, nameof(mappedConfiguration.WebhookUrl));
        ValidateConfigurationProperty(mappedConfiguration.WebhookAuthHeader, nameof(mappedConfiguration.WebhookAuthHeader));
        ValidateConfigurationProperty(mappedConfiguration.WebhookAuthKey, nameof(mappedConfiguration.WebhookAuthKey));

        if (ticketEvent is null)
        {
            _logger.LogError("Mapped ticket event is null. Skipping sending ticket event to Mapped");
            throw new Exception("Mapped ticket event is null");
        }
        await _mappedIdentityService.SetIdentitiesAsync(ticketEvent);
        await SetTicketFields(ticketEvent);
        var ticketEventJson = JsonConvert.SerializeObject(ticketEvent);
        _logger.LogInformation("Sending ticket event to Mapped, Event data: {TicketEventJson}", ticketEventJson);
       
        _httpClient.DefaultRequestHeaders.Add(mappedConfiguration.WebhookAuthHeader, mappedConfiguration.WebhookAuthKey);
        var response = await _httpClient.PostAsJsonAsync(mappedConfiguration.WebhookUrl, ticketEvent);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Successfully sent ticket event to Mapped. Status code: {StatusCode}, TicketId: {TicketId}",
                                                   response.StatusCode,
                                                   ticketEvent.Data.Id);
        }
        else
        {
            _logger.LogError("Failed to sent ticket event to Mapped. Status code: {StatusCode}, TicketId: {TicketId}",
                                response.StatusCode,
                                ticketEvent.Data.Id);
            throw new Exception($"Failed to sent ticket event to Mapped. Status code: {response.StatusCode}, TicketId: {ticketEvent.Data.Id}");
        }
    }

    public async Task<TicketMetadataResponse> GetTicketMetaDataAsync(MappedIntegrationConfiguration mappedConfiguration)
    {
        var baseMtiUrl = mappedConfiguration?.MtiBaseUrl;
        var connectorId = mappedConfiguration?.TicketMetaDataConnectorId;

        if (string.IsNullOrWhiteSpace(baseMtiUrl))
        {
            _logger.LogError("Ticket Metadata Sync : Mti BaseMtiUrl is null or empty. Skipping sync ticket metadata");
            return null;
        }

        if (string.IsNullOrWhiteSpace(connectorId))
        {
            _logger.LogError("Ticket Metadata Sync : ConnectorId is null or empty. Skipping sync ticket metadata");
            return null;
        }



        var url = $"{baseMtiUrl}/Sync/GetConnectorMetadata/{connectorId}";
        _logger.LogInformation("Ticket Metadata Sync : Syncing ticket metadata from Mti, Url: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Ticket Metadata Sync : Failed to sync ticket metadata from Mti. Status code: {StatusCode}", response.StatusCode);

            var responseContent = response.Content?.ReadAsStringAsync();
            _logger.LogError("Ticket Metadata Sync : Failed to sync ticket metadata from Mti. Response: {Response}", responseContent);

            return null;
        }

        var result = await response.Content.ReadAsAsync<List<TicketMetadataResponse>>();
        if(result is null || result.Count == 0)
        {
            _logger.LogError("Ticket Metadata Sync : Failed to sync ticket metadata from Mti. Response is null or empty");
            return null;
        }
        return result.FirstOrDefault();
    }

    #region Private Methods
    private async Task SetTicketFields(MappedTicketEventDto dto)
    {
        var ticket = await _workflowContext.Tickets
                     .Include(x=>x.Category)
                     .Include(x=>x.JobType)
                     .Include(x=>x.ServiceNeeded)
                     .Include(x=>x.SubStatus)
                     .FirstOrDefaultAsync(x=>x.Id == dto.Data.Id);
        if (ticket is null)
        {
            _logger.LogError("Ticket not found in database. TicketId: {TicketId}", dto.Data.Id);
             throw new Exception($"Ticket not found in database. TicketId: {dto.Data.Id}");
        }

        dto.Data.RequestType = ticket.Category?.Name;
        dto.Data.JobType = ticket.JobType?.Name;
        dto.Data.ServiceNeeded = ticket.ServiceNeeded?.Name;
        dto.Data.SubStatus = ticket.SubStatus?.Name;
    }

    private void ValidateConfigurationProperty(string configurationValue, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(configurationValue))
        {
            var errorMessage = $"Mapped {propertyName} is not configured, Skip sending ticket event to Mapped";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
    }

    #endregion

}

