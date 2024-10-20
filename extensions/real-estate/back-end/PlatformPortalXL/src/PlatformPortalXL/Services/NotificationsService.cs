using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models.Notification;
using PlatformPortalXL.ServicesApi.NotificationApi;
using Willow.Batch;

namespace PlatformPortalXL.Services;

public interface INotificationsService
{
    Task<NotificationStatesStats> UpdateNotifiactionStateAsync(Guid currentUserId, UpdateNotificationStateRequest request);
    Task<BatchDto<NotificationUser>> GetNotifications(Guid currentUserId, BatchRequestDto request);

    Task<List<NotificationStatesStats>> GetNotificationsStatesStats(Guid currentUserId, IEnumerable<FilterSpecificationDto> request);
}

public class NotificationsService : INotificationsService
{
    private readonly INotificationApiService _notificationApiService;
    private readonly ILogger<NotificationTriggerService> _logger;

    public NotificationsService(INotificationApiService notificationApiService,
        ILogger<NotificationTriggerService> logger)
    {
        _notificationApiService = notificationApiService;
        _logger = logger;
    }

    /// <summary>
    /// Given a user id, update the state of their notifications
    /// </summary>
    public Task<NotificationStatesStats> UpdateNotifiactionStateAsync(Guid currentUserId, UpdateNotificationStateRequest request)
    {
        return _notificationApiService.UpdateNotificationStateAsync(new ServicesApi.NotificationApi.Request.UpdateNotificationStateApiRequest
        {
             UserId = currentUserId,
             NotificationIds = request.NotificationIds,
             State = request.State
        });
    }

    /// <summary>
    /// Given a user id, return their notifications
    /// </summary>
    public Task<BatchDto<NotificationUser>> GetNotifications(Guid currentUserId, BatchRequestDto request)
    {
        request.FilterSpecifications.Upsert(nameof(NotificationUser.UserId), currentUserId);

        return _notificationApiService.GetNotifications(request);
    }

    /// <summary>
    /// Given a user id, return the notification counts by state
    /// </summary>
    public Task<List<NotificationStatesStats>> GetNotificationsStatesStats(Guid currentUserId, IEnumerable<FilterSpecificationDto> request)
    {
        request.Upsert(nameof(NotificationUser.UserId), currentUserId);

        return _notificationApiService.GetNotificationsStatesStats(request);
    }
}
