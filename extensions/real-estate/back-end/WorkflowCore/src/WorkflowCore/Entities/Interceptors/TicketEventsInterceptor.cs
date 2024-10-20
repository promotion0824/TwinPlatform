using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.Common;
using WorkflowCore.Dto;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Entities.Interceptors;

/// <summary>
/// EfCore interceptor to track changes in ticket entity
/// and publish these changes to service bus topic
/// </summary>
public class TicketEventsInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<TicketEventsInterceptor> _logger;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly MappedIntegrationConfiguration _mappedIntegrationConfiguration;
    private readonly string _topicName;
    private readonly ISessionService _sessionService;
    private readonly ISiteApiService _siteApiService;


    public TicketEventsInterceptor(ILogger<TicketEventsInterceptor> logger,
                                   IConfiguration configuration,
                                   ServiceBusClient serviceBusClient,
                                   ISessionService sessionService,
                                   ISiteApiService siteApiService)
    {
        var appSettings = configuration.Get<AppSettings>();

        ArgumentNullException.ThrowIfNull(appSettings?.MappedIntegrationConfiguration);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrEmpty(appSettings?.TicketEventsTopicName);

        _logger = logger;
        _topicName = appSettings?.TicketEventsTopicName;
        _mappedIntegrationConfiguration = appSettings?.MappedIntegrationConfiguration;
        _serviceBusSender = serviceBusClient.CreateSender(_topicName);
        _sessionService = sessionService;
        _siteApiService = siteApiService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var customerId = _mappedIntegrationConfiguration?.CustomerId;
        if (customerId is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        var entries = eventData.Context.ChangeTracker.Entries<TicketEntity>()
                                                     .Where(x => (x.State == EntityState.Added
                                                                     || x.State == EntityState.Modified)
                                                                     && x.Entity.SourceType == SourceType.Platform
                                                                     && customerId == x.Entity.CustomerId);

        if(entries is null || entries.Count() == 0 )
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        var siteId = entries.FirstOrDefault()?.Entity?.SiteId;
        

        if (!siteId.HasValue)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var site = await _siteApiService.GetCachedExtendedSite(siteId.Value);

        _sessionService.SetMappedSiteSetting(new MappedSiteSetting(siteId.Value, site.Features?.IsTicketMappedIntegrationEnabled ?? false));

        if (!_sessionService.MappedSiteSetting?.IsTicketMappedIntegrationEnabled ?? true)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

       
       
        // save the entity status to be used after entity saved in db
        foreach (var entry in entries)
        {
            _logger.LogInformation("Intercept SavingChangesAsync for ticket Id {EntityId}", entry.Entity.Id);
            entry.Entity.EntityLifeCycleState = entry.State;
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_sessionService.MappedSiteSetting?.IsTicketMappedIntegrationEnabled ?? false)
        {
            await PublishChangesAsync(eventData);
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }


    private async Task PublishChangesAsync(SaveChangesCompletedEventData eventData)
    {
        var sendMessageTasks = new List<Task>();
        try
        {
            var customerId = _mappedIntegrationConfiguration?.CustomerId;

            if (customerId is null)
            {
                return;
            }
            // only enqueue ticket data created in willow command ui
            // and if the ticket has customer and sites subscription 
            var entries = eventData.Context.ChangeTracker.Entries<TicketEntity>()
                                                         .Where(x => (x.Entity.EntityLifeCycleState == EntityState.Added
                                                                 || x.Entity.EntityLifeCycleState == EntityState.Modified)
                                                                 && x.Entity.SourceType == SourceType.Platform
                                                                 && customerId == x.Entity.CustomerId
                                                                 && x.Entity.SiteId == _sessionService.MappedSiteSetting.siteId);

            foreach (var entry in entries)
            {
                var msg = TicketEventDto.MapFromTicketEntity(entry.Entity);
                var msgJson = JsonSerializer.Serialize(msg);
                ServiceBusMessage serviceBusMessage = new(Encoding.UTF8.GetBytes(msgJson));
                _logger.LogInformation("Enqueue ticket data to service bus topic {TopicName} with ticketId {DataId}", _topicName, msg.Data.Id);
                var messageTask = _serviceBusSender.SendMessageAsync(serviceBusMessage);
                sendMessageTasks.Add(messageTask);

            }
            await Task.WhenAll(sendMessageTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enqueue ticket data failed");
        }
        finally
        {
            await _serviceBusSender.CloseAsync();
        }
    }

}


