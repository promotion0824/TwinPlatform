using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Dto;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Services.MappedIntegration.Dtos;
using WorkflowCore.Services.MappedIntegration.Interfaces;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services.Background;

/// <summary>
/// Background service to process ticket events from service bus Mapped subscription
/// this service will process the ticket event and send it to Mapped API
/// it will only be enabled if MappedIntegrationConfiguration is enabled in appsettings
/// </summary>
public class MappedTicketProcessorHostedService : BaseMessageProcessorHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;

    public MappedTicketProcessorHostedService(ILogger<MappedTicketProcessorHostedService> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        : base($"{DataConstants.MAPPED_PREFIX}{configuration.Get<AppSettings>().MappedIntegrationConfiguration?.CustomerId}", logger, configuration)
    {
        _appSettings = configuration.Get<AppSettings>();
        _serviceProvider = serviceProvider;
    }
    protected override async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            _logger.LogInformation("Mapped Ticket Received message: SequenceNumber:{SequenceNumber} Body:{Body}",
                                    args.Message.SequenceNumber, args.Message.Body);
            using var scope = _serviceProvider.CreateScope();
            var mappedApiService = scope.ServiceProvider.GetRequiredService<IMappedApiService>();
            var msgBody = Encoding.UTF8.GetString(args.Message.Body.ToArray());
            var ticketEvent = JsonConvert.DeserializeObject<TicketEventDto>(msgBody);
            var mappedTicketEvent = MappedTicketEventDto.MapFrom(ticketEvent);
            await mappedApiService.SendTicketDataAsync(_appSettings.MappedIntegrationConfiguration, mappedTicketEvent);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing enqueued ticket message for {CustomerName}",
                                 _appSettings?.MappedIntegrationConfiguration?.CustomerName);
        }

    }
}

