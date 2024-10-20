using Microsoft.EntityFrameworkCore;
using NotificationCore.Controllers.Requests;
using NotificationCore.Entities;
using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Batch;

namespace NotificationCore.Repositories;
public interface INotificationsRepository
{
    Task CreateNotificationAsync(Notification notification);
    Task<NotificationStatesStats> UpdateNotificationState(UpdateNotificationStateRequest request);
    Task<BatchDto<NotificationUser>> GetNotifications(BatchRequestDto request);

    Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request);
}
public class NotificationsRepository : INotificationsRepository
{
    private readonly NotificationDbContext _notificationDbContext;


    public NotificationsRepository(NotificationDbContext notificationDbContext)
    {
        _notificationDbContext = notificationDbContext;
    }

    public async Task<BatchDto<NotificationUser>> GetNotifications(BatchRequestDto request)
    {
        var query = await _notificationDbContext.NotificationsUsers
            .Include(x => x.Notification)
            .FilterBy(request.FilterSpecifications)
            .SortBy(request.SortSpecifications)
            .Paginate(request.Page, request.PageSize);

        return new BatchDto<NotificationUser>()
        {
            After = query.After,
            Before = query.Before,
            Items = [.. NotificationUser.MapFrom(query.Items)],
            Total = query.Total
        };
    }

    public Task<List<NotificationStatesStats>> GetNotificationsStatesStats(IEnumerable<FilterSpecificationDto> request)
    {
        return _notificationDbContext.NotificationsUsers.AsNoTracking()
            .FilterBy(request)
            .GroupBy(x => x.State)
            .Select(g => new NotificationStatesStats
            {
                State = g.Key,
                Count = g.Count(),
            }).ToListAsync();
    }

    public async Task<NotificationStatesStats> UpdateNotificationState(UpdateNotificationStateRequest request)
    {
        var hasFilter = (request.NotificationIds?.Count ?? 0) > 0;

        var userNotificationsToUpdate = _notificationDbContext.NotificationsUsers
                .Where(x => x.UserId == request.UserId
                    && (!hasFilter || request.NotificationIds.Contains(x.NotificationId))
                    && (x.State != request.State || request.State == NotificationUserState.Cleared));

        foreach (var userNotification in userNotificationsToUpdate)
        {
            userNotification.State = request.State;

            if (request.State == NotificationUserState.Cleared)
            {
                userNotification.ClearedDateTime = DateTime.UtcNow;
            }

            _notificationDbContext.Update(userNotification);
        }

        await _notificationDbContext.SaveChangesAsync();

        return new NotificationStatesStats()
        {
            State = request.State,
            Count = userNotificationsToUpdate.Count()
        };
    }

    public async Task CreateNotificationAsync(Notification notification)
    {
        // create notification
        var notificationEntity = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Source = notification.Source,
            Title = notification.Title,
            PropertyBagJson = notification.PropertyBagJson,
            TriggerIdsJson = notification.TriggerIds.Distinct().ToList(),
            SourceId = notification.SourceId
        };
        var notificationsUsers = notification.UserIds.Distinct().Select(x => new NotificationUserEntity
        {
            NotificationId = notificationEntity.Id,
            UserId = x,
            State = NotificationUserState.New,
        }).ToList();
        _notificationDbContext.Add(notificationEntity);
        _notificationDbContext.AddRange(notificationsUsers);
        await _notificationDbContext.SaveChangesAsync();
    }
}

