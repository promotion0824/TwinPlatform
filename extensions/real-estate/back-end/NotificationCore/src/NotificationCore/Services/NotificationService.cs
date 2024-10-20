using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using Newtonsoft.Json;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using NotificationCore.Models;
using NotificationCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;

namespace NotificationCore.Services;

/// <summary>
/// Represents a service for creating notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a notification asynchronously.
    /// </summary>
    /// <param name="notificationMessage">The notification message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateNotificationAsync(NotificationMessage notificationMessage);
    Task<NotificationStatesStats> UpdateNotificationState(UpdateNotificationStateRequest request);
    Task<BatchDto<NotificationUserDto>> GetNotifications(BatchRequestDto request);
    Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request);
}

public class NotificationService : INotificationService
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IUserAuthorizationService _userAuthorizationService;
    private readonly INotificationTriggerRepository _notificationTriggerRepository;


    public NotificationService(INotificationsRepository notificationsRepository, IUserAuthorizationService userAuthorizationService, INotificationTriggerRepository notificationTriggerRepository)
    {
        _userAuthorizationService = userAuthorizationService;
        _notificationsRepository = notificationsRepository;
        _notificationTriggerRepository = notificationTriggerRepository;
    }

    /// <summary>
    /// Return requested notifications
    /// </summary>
    public async Task<BatchDto<NotificationUserDto>> GetNotifications(BatchRequestDto request)
    {
        var query = await _notificationsRepository.GetNotifications(request);

        return new BatchDto<NotificationUserDto>()
        {
            After = query.After,
            Before = query.Before,
            Items = [.. NotificationUserDto.MapFrom(query.Items)],
            Total = query.Total
        };
    }

    /// <summary>
    /// Return the count of notifications per user grouped by state
    /// </summary>
    public Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request)
    {
        return _notificationsRepository.GetNotificationsStatesStats(request);
    }

    /// <summary>
    /// Update requested notifications
    /// </summary>
    public Task<NotificationStatesStats> UpdateNotificationState(UpdateNotificationStateRequest request)
    {
        return _notificationsRepository.UpdateNotificationState(request);
    }

    /// <summary>
    /// Record a notification
    /// </summary>
    public async Task CreateNotificationAsync(NotificationMessage notificationMessage)
    {

        // get selectedTriggers based on the filters applied to the notification message
        var filteredTriggers = await _notificationTriggerRepository.GetFilteredTriggersAsync(notificationMessage);
        if (filteredTriggers.Count == 0)
        {
            return;
        }

        // get all users ids related to the triggers
        var userIds = new List<Guid>();

        // get personal user ids
        userIds = filteredTriggers
                            .Where(x => x.Type == NotificationType.Personal)
                            .Select(x => x.CreatedBy).ToList();

        // get work group ids-trigger ids mapping
        var workgroupIdTriggerIds = filteredTriggers.Where(x => x.Type == NotificationType.Workgroup)
                                               .SelectMany(t => t.WorkgroupSubscriptions.Select(x => new { x.WorkgroupId, TriggerId = t.Id }))
                                               .ToList();

        var notificationSubscriptionOverrides = filteredTriggers.SelectMany(x => x.NotificationSubscriptionOverrides)
                                                                .ToList();


        // get all users ids related to the trigger workgroups except the ones that are overridden
        if (workgroupIdTriggerIds.Count > 0)
        {
            var workgroupIds = workgroupIdTriggerIds.Select(x => x.WorkgroupId).Distinct().ToList();
            var filterModel = new BatchRequestDto
            {
                FilterSpecifications = (new List<FilterSpecificationDto>()
                             .Upsert(nameof(GroupModel.Id), FilterOperators.ContainedIn, workgroupIds))
                             .ToArray()
            };

            var workGroups = await _userAuthorizationService.GetApplicationGroupsAsync(filterModel);

            foreach (var workgroupTrigger in workgroupIdTriggerIds)
            {
                var workgroupUserIds = workGroups?.Items?
                    .Where(x => x.Id == workgroupTrigger.WorkgroupId)
                    .SelectMany(x => x.Users.Select(x => x.Id))
                    .ToList();

                var overrideUsers = notificationSubscriptionOverrides
                    .Where(x => x.NotificationTriggerId == workgroupTrigger.TriggerId)
                    .ToList();

                var filteredWorkgroupUserIds = workgroupUserIds
                    .Where(x => !overrideUsers.Select(x => x.UserId).Contains(x))
                    .ToList();

                userIds.AddRange(filteredWorkgroupUserIds);

            }
        }
        var notification = new Notification
        {
            Source = notificationMessage.Source,
            Title = notificationMessage.Title,
            PropertyBagJson = JsonConvert.SerializeObject(notificationMessage),
            TriggerIds = filteredTriggers.Select(x => x.Id).ToList(),
            UserIds = userIds.ToList(),
            SourceId = notificationMessage.SourceId
        };

        // create notification
        await _notificationsRepository.CreateNotificationAsync(notification);
    }
}

