using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformPortalXL.Models.Notification;
using PlatformPortalXL.ServicesApi.NotificationApi.Request;
using Willow.Api.Client;
using Willow.Batch;

namespace PlatformPortalXL.ServicesApi.NotificationApi;

public interface INotificationApiService
{
    Task<NotificationStatesStats> UpdateNotificationStateAsync(UpdateNotificationStateApiRequest request);
    Task<BatchDto<NotificationUser>> GetNotifications(BatchRequestDto request);
    Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request);
}

public class NotificationApiService(IRestApi notificationApi) : INotificationApiService
{
    public Task<NotificationStatesStats> UpdateNotificationStateAsync(UpdateNotificationStateApiRequest request)
    {
        return notificationApi.Put<UpdateNotificationStateApiRequest, NotificationStatesStats>($"notifications/state", request);
    }

    public Task<BatchDto<NotificationUser>> GetNotifications(BatchRequestDto request)
    {
        return notificationApi.Post<BatchRequestDto, BatchDto<NotificationUser>>($"notifications/all", request);
    }

    public Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request)
    {
        return notificationApi.Post<IEnumerable<FilterSpecificationDto>, List<NotificationStatesStats>>($"notifications/states/stats", request);
    }
}
