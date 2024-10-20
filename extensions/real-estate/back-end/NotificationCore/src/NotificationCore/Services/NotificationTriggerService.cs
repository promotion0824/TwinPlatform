
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NotificationCore.Controllers.Requests;
using NotificationCore.Dto;
using NotificationCore.Models;
using NotificationCore.Repositories;
using Willow.Batch;
using Willow.ExceptionHandling.Exceptions;

namespace NotificationCore.Services;
public interface INotificationTriggerService
{
    Task<NotificationTriggerDto> CreateNotificationTriggerAsync(CreateNotificationTriggerRequest request);
    Task<NotificationTriggerDto> UpdateNotificationTriggerAsync(Guid id, UpdateNotificationTriggerRequest request);
    Task<BatchDto<NotificationTriggerDto>> GetNotificationTriggers(BatchRequestDto request);
    Task<NotificationTriggerDto> GetNotificationTriggerAsync(Guid id);
    Task DeleteNotificationTriggerAsync(Guid id);
    Task BatchToggleNotificationTriggerAsync(BatchNotificationTriggerToggleRequest request);
}
public class NotificationTriggerService: INotificationTriggerService
{
    private readonly ILogger<NotificationTriggerService> _logger;
    private readonly INotificationTriggerRepository _repository;

    public NotificationTriggerService(ILogger<NotificationTriggerService> logger, INotificationTriggerRepository repository)
    {
        _logger=logger;
        _repository = repository;
    }
    public async Task DeleteNotificationTriggerAsync(Guid id)
    {
        var trigger = await _repository.GetNotificationTriggerByIdAsync(id);
        if (trigger == null)
            throw new NotFoundException($"Notification trigger with id {id} not found");

        await _repository.DeleteNotificationTriggerAsync(trigger);
    }
    public async Task<NotificationTriggerDto> GetNotificationTriggerAsync(Guid id)
    {
        var trigger = await _repository.GetNotificationTriggerDetailByIdAsync(id);
        if(trigger==null)
            throw new NotFoundException($"Notification trigger with id {id} not found");
        return NotificationTriggerDto.MapFrom(trigger);
    }
    public async Task<NotificationTriggerDto> CreateNotificationTriggerAsync(CreateNotificationTriggerRequest request)
    {
        ValidateNotificationRequest(request, request.Type, request.Focus);

        var notificationTrigger =await _repository.CreateNotificationTriggerAsync(MapToModel(request));

        return NotificationTriggerDto.MapFrom(notificationTrigger);
    }

    public async Task<NotificationTriggerDto> UpdateNotificationTriggerAsync(Guid id,
        UpdateNotificationTriggerRequest request)
    {
        ValidateNotificationRequest(request, request.Type, request.Focus);

        var trigger = await _repository.GetNotificationTriggerByIdAsync(id);
        if (trigger == null)
        {
            throw new NotFoundException($"Notification trigger with id {id} not found");
        }

        if (trigger.Type == NotificationType.Workgroup && !trigger.CanUserDisableNotification && request.IsEnabledForUser is false)
        {
            throw new BadRequestException("User cannot disable the trigger");
        }

        var currentType=trigger.Type;
        var currentFocus = trigger.Focus;

        trigger.IsEnabled = request.IsEnabled ?? trigger.IsEnabled;
        trigger.Channels = request.Channels ?? trigger.Channels;
        trigger.CanUserDisableNotification = request.CanUserDisableNotification ?? trigger.CanUserDisableNotification;
        trigger.UpdatedBy = request.UpdatedBy;
        trigger.PriorityIds = request.PriorityIds ?? trigger.PriorityIds;
        trigger.Source = request.Source ?? trigger.Source;
        trigger.Type = request.Type ?? trigger.Type;
        trigger.Focus = request.Focus ?? trigger.Focus;
        trigger.WorkgroupIds = request.WorkGroupIds;
        trigger.Twins = NotificationTriggerTwinDto.MapTo(request.Twins);
        trigger.SkillCategoryIds = request.SkillCategoryIds;
        trigger.TwinCategoryIds = request.TwinCategoryIds;
        trigger.SkillIds = request.SkillIds;
        trigger.Locations = request.Locations;
        trigger.UpdatedDate=DateTime.UtcNow;
        var updatedTrigger = await _repository.UpdateNotificationTriggerAsync(trigger, currentFocus, currentType, request.AllLocation??false, request.IsEnabledForUser);
        return NotificationTriggerDto.MapFrom(updatedTrigger);
    }

    public async Task BatchToggleNotificationTriggerAsync(BatchNotificationTriggerToggleRequest request)
    {
        await _repository.BatchToggleNotificationTriggerAsync(request);
    }

    public async Task<BatchDto<NotificationTriggerDto>> GetNotificationTriggers(BatchRequestDto request)
    {
        var query = await _repository.GetNotificationTriggers(request);

        return new BatchDto<NotificationTriggerDto>()
        {
            After = query.After,
            Before = query.Before,
            Items = NotificationTriggerDto.MapFrom(query.Items.ToList()).ToArray(),
            Total = query.Total
        };
    }


    #region private 

    private NotificationTrigger MapToModel(CreateNotificationTriggerRequest request)
    {
        return new NotificationTrigger
        {
            Id = Guid.NewGuid(),
            IsEnabled = request.IsEnabled,
            Channels = request.Channels,
            CanUserDisableNotification = request.CanUserDisableNotification,
            SkillCategoryIds = request.SkillCategoryIds,
            TwinCategoryIds = request.TwinCategoryIds,
            CreatedBy = request.CreatedBy,
            CreatedDate = DateTime.UtcNow,
            Focus = request.Focus,
            Locations = request.Locations,
            PriorityIds = request.PriorityIds,
            SkillIds = request.SkillIds,
            Source = request.Source,
            Twins = NotificationTriggerTwinDto.MapTo(request.Twins),
            Type = request.Type,
            WorkgroupIds = request.Type == NotificationType.Workgroup ? request.WorkGroupIds : null
        };
    }

    private static void ValidateNotificationRequest(NotificationTriggerRequestBase request, NotificationType? type, NotificationFocus? focus)
    {

        if (type!=null &&  type==NotificationType.Workgroup && (request.WorkGroupIds==null || request.WorkGroupIds.Count==0))
            throw new BadRequestException($"workgroupIds is required");

        if(focus!=null)
            switch (focus)
            {
                case NotificationFocus.SkillCategory:
                    if(request.SkillCategoryIds==null || request.SkillCategoryIds.Count == 0)
                        throw new BadRequestException($"Skill categoryId is required");
                    break;
                case NotificationFocus.Skill:
                    if (request.SkillIds == null || request.SkillIds.Count == 0)
                        throw new BadRequestException($"SkillId is required");
                    break;
                case NotificationFocus.Twin:
                    if (request.Twins == null || request.Twins.Count == 0)
                        throw new BadRequestException($"TwinId is required");
                    break;

            }
    }

    #endregion
}
