using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Notification.Requests;
using PlatformPortalXL.Models.Notification;
using PlatformPortalXL.Models.NotificationTrigger;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi.Request;
using Willow.Batch;
using Willow.ExceptionHandling.Exceptions;
using Willow.Platform.Users;

namespace PlatformPortalXL.Services;

public interface INotificationTriggerService
{
    Task<NotificationTriggerDto> GetAsync(Guid userId, string email, Guid id);
    Task<BatchDto<NotificationTriggerDto>> GetAllAsync(Guid currentUserId, string currentUserEmail, BatchRequestDto request);
    Task<NotificationTriggerDto> CreateAsync(Guid currentUserId, string currentUserEmail, CreateNotificationTriggerRequest request);
    Task UpdateAsync(Guid currentUserId, string currentUserEmail, Guid triggerId, UpdateNotificationTriggerRequest request);
    Task DeleteAsync(Guid currentUserId, string currentUserEmail, Guid triggerId);
    Task BatchToggleAsync(Guid currentUserId, string currentUserEmail, BatchNotificationTriggerToggleRequest request);
}

public class NotificationTriggerService : INotificationTriggerService
{
    private readonly INotificationTriggerApiService _notificationTriggerApiService;
    private readonly IAccessControlService _accessControl;
    private readonly ILogger<NotificationTriggerService> _logger;

    public NotificationTriggerService(INotificationTriggerApiService notificationTriggerApiService,
        ILogger<NotificationTriggerService> logger,
        IAccessControlService accessControl)
    {
        _notificationTriggerApiService = notificationTriggerApiService;
        _logger = logger;
        _accessControl = accessControl;
    }

    public async Task BatchToggleAsync(Guid currentUserId,string currentUserEmail, BatchNotificationTriggerToggleRequest request)
    {
        var isAdmin = await _accessControl.IsAdminUser(currentUserId, currentUserEmail);

        await _notificationTriggerApiService.BatchToggleAsync(new BatchNotificationTriggerToggleApiRequest
        {
            Source = request.Source,
            UserId = currentUserId,
            IsAdmin = isAdmin,
            WorkgroupIds = !isAdmin?await _accessControl.GetAuthorizedWorkgroupIds(currentUserId):null
        });
    }
    public async Task<NotificationTriggerDto> GetAsync(Guid userId, string email, Guid id)
    {
        var trigger = await _notificationTriggerApiService.GetAsync(id);

        if (trigger == null)
        {
            throw new NotFoundException($"Notification trigger with id {id} not found");
        }

        if (!await HasAccess(trigger.Type, trigger.CreatedBy, userId, email))
        {
            throw new ForbiddenException("user does not have access");
        }

        return NotificationTriggerDto.MapFrom(trigger);
    }

    public async Task<BatchDto<NotificationTriggerDto>> GetAllAsync(Guid currentUserId, string currentUserEmail, BatchRequestDto request)
    {
        var subscriberIds = await _accessControl.GetAuthorizedWorkgroupIds(currentUserId);
        subscriberIds.Add(currentUserId);
        request.FilterSpecifications.Upsert("WorkgroupSubscriptions[WorkgroupId], CreatedBy", FilterOperators.ContainedIn, subscriberIds);

        var triggers = await _notificationTriggerApiService.GetAllAsync(request);

        return new BatchDto<NotificationTriggerDto>
        {
            After = triggers.After,
            Before = triggers.Before,
            Items = NotificationTriggerDto.MapFrom(triggers.Items.ToList()).ToArray(),
            Total = triggers.Total
        };
    }

    public async Task<NotificationTriggerDto> CreateAsync(Guid currentUserId,string currentUserEmail,  CreateNotificationTriggerRequest request)
    {
        ValidateNotificationTriggerRequest(request, request.Type, request.Focus);

        if (!await HasAccess(request.Type, null, currentUserId, currentUserEmail))
        {
            throw new ForbiddenException("User does not have access to create a workgroup notification trigger");
        }
        var apiRequest = new CreateNotificationTriggerApiRequest
        {
            Type = request.Type,
            Source = request.Source,
            Focus = request.Focus,
            Locations = request.Locations,
            IsEnabled = request.IsEnabled,
            CreatedBy = currentUserId,
            CanUserDisableNotification = request.CanUserDisableNotification,
            WorkGroupIds = request.WorkGroupIds,
            SkillCategoryIds = request.SkillCategories,
            TwinCategoryIds = request.TwinCategoryIds,
            Twins = request.Twins,
            SkillIds = request.SkillIds,
            PriorityIds = request.Priorities,
            Channels = request.Channels
        };
        var notificationTrigger=await  _notificationTriggerApiService.CreateAsync(apiRequest);
        return NotificationTriggerDto.MapFrom(notificationTrigger);
    }


    public async Task UpdateAsync(Guid currentUserId, string currentUserEmail, Guid triggerId, UpdateNotificationTriggerRequest request)
    {
        ValidateNotificationTriggerRequest(request, request.Type, request.Focus);

        var trigger = await _notificationTriggerApiService.GetAsync(triggerId);

        if (trigger == null)
        {
            throw new NotFoundException($"Notification trigger with id {triggerId} not found");
        }
        if (!await HasAccess(trigger.Type, trigger.CreatedBy, currentUserId, currentUserEmail))
        {
            throw new ForbiddenException("User does not have access to edit the notification trigger");
        }
        if (trigger.Type == NotificationTriggerType.Workgroup && !trigger.CanUserDisableNotification && request.IsEnabledForUser is false)
        {
            throw new BadRequestException("User cannot disable the trigger");
        }

        var apiRequest = new UpdateNotificationTriggerApiRequest()
        {
            Type = request.Type,
            Source = request.Source,
            Focus = request.Focus,
            Locations = request.Locations,
            IsEnabled = request.IsEnabled,
            UpdatedBy = currentUserId,
            CanUserDisableNotification = request.CanUserDisableNotification,
            WorkGroupIds = request.WorkGroupIds,
            SkillCategoryIds = request.SkillCategories?.Select(c => (int)c).ToList(),
            TwinCategoryIds = request.TwinCategoryIds,
            Twins = request.Twins,
            SkillIds = request.SkillIds,
            PriorityIds = request.Priorities?.Select(c => (int)c).ToList(),
            Channels = request.Channels,
            IsEnabledForUser = request.IsEnabledForUser
        };
        await _notificationTriggerApiService.UpdateAsync(triggerId, apiRequest);
    }

    public async Task DeleteAsync(Guid currentUserId, string currentUserEmail, Guid triggerId)
    {
        var trigger = await _notificationTriggerApiService.GetAsync(triggerId);
        if (trigger == null)
        {
            throw new NotFoundException($"Notification trigger with id {triggerId} not found");
        }

        if (!await HasAccess(trigger.Type, trigger.CreatedBy, currentUserId, currentUserEmail))
        {
            throw new ForbiddenException($"User  {currentUserId} does not have access to delete the notification trigger {triggerId} ");
        }
      
        await _notificationTriggerApiService.DeleteAsync(triggerId);
    }

    private void ValidateNotificationTriggerRequest(NotificationTriggerRequestBase request, NotificationTriggerType? type, NotificationTriggerFocus? focus)
    {

        if (type is NotificationTriggerType.Workgroup)
        {
            if (request.WorkGroupIds == null || request.WorkGroupIds.Count == 0)
                throw new BadRequestException($"workgroupIds is required");
        }

        if (!focus.HasValue)
            return;

        switch (focus.Value)
        {
            case NotificationTriggerFocus.TwinCategory: break;
            case NotificationTriggerFocus.SkillCategory:
                if (request.SkillCategories == null || request.SkillCategories.Count == 0)
                    throw new BadRequestException($"skill categoryId is required");
                break;
            case NotificationTriggerFocus.Skill:
                if (request.SkillIds == null || request.SkillIds.Count == 0)
                    throw new BadRequestException($"SkillId is required");
                break;
            case NotificationTriggerFocus.Twin:
                if (request.Twins == null || request.Twins.Count == 0 || request.Twins.Any(x => string.IsNullOrWhiteSpace(x.TwinId) || string.IsNullOrWhiteSpace(x.TwinName)))
                    throw new BadRequestException($"TwinId and TwinName are required");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<bool> HasAdminAccess(NotificationTriggerType triggerType, Guid userId, string email = null)
    {
        return triggerType == NotificationTriggerType.Workgroup && await _accessControl.IsAdminUser(userId, email);
    }

    private bool HasUserAccess(NotificationTriggerType triggerType, Guid userId, Guid? createdBy = null)
    {
        return triggerType == NotificationTriggerType.Personal && (!createdBy.HasValue || createdBy == userId);
    }

    private async Task<bool> HasAccess(NotificationTriggerType triggerType, Guid? createdBy, Guid userId, string email = null)
    {
        return await HasAdminAccess(triggerType, userId, email) || HasUserAccess(triggerType, userId, createdBy);
    }
}
