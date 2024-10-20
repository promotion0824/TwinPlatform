using System;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using Willow.Api.Client;
using Willow.Batch;

namespace PlatformPortalXL.ServicesApi.NotificationTriggerApi;

public interface INotificationTriggerApiService
{
    Task<NotificationTrigger> CreateAsync(CreateNotificationTriggerApiRequest request);
    Task<NotificationTrigger> GetAsync(Guid id);
    Task<BatchDto<NotificationTrigger>> GetAllAsync(BatchRequestDto request);
    Task UpdateAsync(Guid triggerId, UpdateNotificationTriggerApiRequest request);
    Task DeleteAsync(Guid triggerId);
    Task BatchToggleAsync(BatchNotificationTriggerToggleApiRequest request);
}

public class NotificationTriggerApiService : INotificationTriggerApiService
{
    private readonly IRestApi _notificationTriggerApi;

    public NotificationTriggerApiService(IRestApi notificationTriggerApi)
    {
        _notificationTriggerApi = notificationTriggerApi;
    }

    public Task BatchToggleAsync(BatchNotificationTriggerToggleApiRequest request)
    {
        return _notificationTriggerApi.PostCommand($"notifications/triggers/toggle", request);
    }

    public Task<NotificationTrigger> GetAsync(Guid id)
    {
        return _notificationTriggerApi.Get<NotificationTrigger>($"notifications/triggers/{id}");
    }

    public Task<BatchDto<NotificationTrigger>> GetAllAsync(BatchRequestDto request)
    {
        return _notificationTriggerApi.Post<BatchRequestDto, BatchDto<NotificationTrigger>>($"notifications/triggers/all", request);
    }

    public Task<NotificationTrigger> CreateAsync(CreateNotificationTriggerApiRequest request)
    {
        return _notificationTriggerApi.Post<CreateNotificationTriggerApiRequest, NotificationTrigger>($"notifications/triggers", request);
    }

    public Task UpdateAsync(Guid triggerId,  UpdateNotificationTriggerApiRequest request)
    {
        return _notificationTriggerApi.PatchCommand($"notifications/triggers/{triggerId}", request);
    }

    public Task DeleteAsync(Guid triggerId)
    {
        return _notificationTriggerApi.Delete($"notifications/triggers/{triggerId}");
    }
}
