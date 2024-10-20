using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NotificationCore.Controllers.Requests;
using NotificationCore.Entities;
using NotificationCore.Models;
using Willow.Batch;
using NotificationCore.TriggerFilterRules;

namespace NotificationCore.Repositories;
public interface INotificationTriggerRepository
{
    Task<NotificationTrigger> CreateNotificationTriggerAsync(NotificationTrigger notificationTrigger);
    Task<NotificationTrigger> GetNotificationTriggerByIdAsync(Guid id);
    Task<BatchDto<NotificationTrigger>> GetNotificationTriggers(BatchRequestDto request);
    Task<NotificationTrigger> GetNotificationTriggerDetailByIdAsync(Guid id);
    Task<NotificationTrigger> UpdateNotificationTriggerAsync(NotificationTrigger trigger, NotificationFocus currentFocus, NotificationType currentType,bool allLocation, bool? isEnabledForUser);
    Task<List<NotificationTriggerEntity>> GetFilteredTriggersAsync(NotificationMessage notificationMessage);
    Task DeleteNotificationTriggerAsync(NotificationTrigger trigger);
    Task BatchToggleNotificationTriggerAsync(BatchNotificationTriggerToggleRequest request);
}
public class NotificationTriggerRepository : INotificationTriggerRepository
{
    private readonly NotificationDbContext _notificationDbContext;
    // Trigger filter rules
    private readonly IEnumerable<ITriggerFilterRule> _triggerFilterRules;

    public NotificationTriggerRepository(NotificationDbContext notificationDbContext, IEnumerable<ITriggerFilterRule> triggerFilterRules)
    {
        _notificationDbContext = notificationDbContext;
        _triggerFilterRules = triggerFilterRules;
    }

    public async Task<NotificationTrigger> CreateNotificationTriggerAsync(NotificationTrigger notificationTrigger)
    {
        var entity = NotificationTriggerEntity.MapFrom(notificationTrigger);
        await _notificationDbContext.NotificationTriggers.AddAsync(entity);
        await _notificationDbContext.SaveChangesAsync();
        return NotificationTriggerEntity.MapTo(entity);

    }
    public async Task<NotificationTrigger> GetNotificationTriggerDetailByIdAsync(Guid id)
    {
        var entity =await _notificationDbContext.NotificationTriggers
            .Include(c => c.Locations)
            .FirstOrDefaultAsync(c=>c.Id==id);

        if(entity==null)
            return null;

        if(entity.Type==NotificationType.Workgroup)
            entity.WorkgroupSubscriptions=await _notificationDbContext.WorkgroupSubscriptions.Where(c => c.NotificationTriggerId == id).ToListAsync();
        switch (entity.Focus)
        {
            case NotificationFocus.Skill:
                entity.Skills = await _notificationDbContext.NotificationTriggerSkills.Where(c => c.NotificationTriggerId == id).ToListAsync();
                break;
            case NotificationFocus.Twin:
                entity.Twins = await _notificationDbContext.NotificationTriggerTwins.Where(c => c.NotificationTriggerId == id).ToListAsync();
                break;
            case NotificationFocus.TwinCategory:
                entity.TwinCategories = await _notificationDbContext.NotificationTriggerTwinCategories.Where(c => c.NotificationTriggerId == id).ToListAsync();
                break;
            case NotificationFocus.SkillCategory:
                entity.SkillCategories = await _notificationDbContext.NotificationTriggerSkillCategories.Where(c => c.NotificationTriggerId == id).ToListAsync();
                break;
        }
        return NotificationTriggerEntity.MapTo(entity);
    }
    public async Task<NotificationTrigger> GetNotificationTriggerByIdAsync(Guid id)
    {
        var entity= await _notificationDbContext.NotificationTriggers.FindAsync(id);
        return NotificationTriggerEntity.MapTo(entity);
    }

    public async Task<BatchDto<NotificationTrigger>> GetNotificationTriggers(BatchRequestDto request)
    {
        var context = _notificationDbContext.NotificationTriggers
            .Include(x => x.WorkgroupSubscriptions)
            .Include(x => x.Locations);

        var query =  await context
            .FilterBy(request.FilterSpecifications)
            .SortBy(request.SortSpecifications)
            .Paginate(request.Page, request.PageSize);

        var triggerIds = query.Items.Select(x => x.Id).ToList();

        var workroupTriggerIds = query.Items.Where(x => x.Type == NotificationType.Workgroup).Select(x => x.Id).ToList();

        var workgroupSubscribers = _notificationDbContext.WorkgroupSubscriptions
            .Where(c => workroupTriggerIds.Contains(c.NotificationTriggerId))
            .FilterBy(request.FilterSpecifications);

        var disabledNotificationSubscribtionsOverrides = _notificationDbContext.NotificationSubscriptionOverrides
            .Where(x => workroupTriggerIds.Contains(x.NotificationTriggerId) && !x.IsEnabled)
            .FilterBy(request.FilterSpecifications);

        var focusGroups = query.Items.GroupBy(x => x.Focus);

        var skills = new List<NotificationTriggerSkillEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.Skill))
        {
            skills = await _notificationDbContext.NotificationTriggerSkills.Where(c => triggerIds.Contains(c.NotificationTriggerId)).ToListAsync();
        }

        var twins = new List<NotificationTriggerTwinEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.Twin))
        {
            twins = await _notificationDbContext.NotificationTriggerTwins.Where(c => triggerIds.Contains(c.NotificationTriggerId)).ToListAsync();
        }

        var twinCategories = new List<NotificationTriggerTwinCategoryEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.TwinCategory))
        {
            twinCategories = await _notificationDbContext.NotificationTriggerTwinCategories.Where(c => triggerIds.Contains(c.NotificationTriggerId)).ToListAsync();
        }

        var skillCategories = new List<NotificationTriggerSkillCategoryEntity>();
        if (focusGroups.Any(x => x.Key == NotificationFocus.SkillCategory))
        {
            skillCategories = await _notificationDbContext.NotificationTriggerSkillCategories.Where(c => triggerIds.Contains(c.NotificationTriggerId)).ToListAsync();
        }

        foreach (var entity in query.Items)
        {
            if (entity.Type == NotificationType.Workgroup)
            {
                entity.WorkgroupSubscriptions = workgroupSubscribers.Where(c => c.NotificationTriggerId == entity.Id).ToList();
                entity.NotificationSubscriptionOverrides = disabledNotificationSubscribtionsOverrides.Where(x => x.NotificationTriggerId == entity.Id).ToList();
            }

            switch (entity.Focus)
            {
                case NotificationFocus.Skill:
                    entity.Skills = skills.Where(c => c.NotificationTriggerId == entity.Id).ToList();
                    break;
                case NotificationFocus.Twin:
                    entity.Twins = twins.Where(c => c.NotificationTriggerId == entity.Id).ToList();
                    break;
                case NotificationFocus.TwinCategory:
                    entity.TwinCategories = twinCategories.Where(c => c.NotificationTriggerId == entity.Id).ToList();
                    break;
                case NotificationFocus.SkillCategory:
                    entity.SkillCategories = skillCategories.Where(c => c.NotificationTriggerId == entity.Id).ToList();
                    break;
            }
        }

        return new BatchDto<NotificationTrigger>()
        {
            After = query.After,
            Before = query.Before,
            Items = NotificationTriggerEntity.MapTo(query.Items.ToList()).ToArray(),
            Total = query.Total
        };
    }

    public async Task<NotificationTrigger> UpdateNotificationTriggerAsync(NotificationTrigger trigger, NotificationFocus currentFocus, NotificationType currentType, bool allLocation, bool? isEnabledForUser)
    {
        if (allLocation || trigger.Locations is { Count: > 0 })
        {
            _notificationDbContext.Locations.RemoveRange(
                _notificationDbContext.Locations.Where(c =>
                    c.NotificationTriggerId == trigger.Id));
        }

        if(trigger.Type != currentType && trigger.Type == NotificationType.Personal || (trigger.WorkgroupIds != null && trigger.WorkgroupIds.Count > 0))
        {
            _notificationDbContext.WorkgroupSubscriptions.RemoveRange(
                _notificationDbContext.WorkgroupSubscriptions.Where(c =>
                    c.NotificationTriggerId == trigger.Id));
          
        }

        switch (currentFocus)
        {
            case NotificationFocus.Skill:
                _notificationDbContext.NotificationTriggerSkills.RemoveRange(
                    _notificationDbContext.NotificationTriggerSkills.Where(c =>
                        c.NotificationTriggerId == trigger.Id));
                break;
            case NotificationFocus.Twin:
                _notificationDbContext.NotificationTriggerTwins.RemoveRange(
                    _notificationDbContext.NotificationTriggerTwins.Where(c =>
                        c.NotificationTriggerId == trigger.Id));
                break;
            case NotificationFocus.TwinCategory:
                _notificationDbContext.NotificationTriggerTwinCategories.RemoveRange(
                    _notificationDbContext.NotificationTriggerTwinCategories.Where(c =>
                        c.NotificationTriggerId == trigger.Id));
                break;
            case NotificationFocus.SkillCategory:
                _notificationDbContext.NotificationTriggerSkillCategories.RemoveRange(
                    _notificationDbContext.NotificationTriggerSkillCategories.Where(c =>
                        c.NotificationTriggerId == trigger.Id));
                break;
        }

        if (isEnabledForUser.HasValue)
        {
            if (trigger.Type == NotificationType.Personal)
            {
                trigger.IsEnabled = isEnabledForUser.Value;
            }
            else
            {
                var notificationOverride =
                    await _notificationDbContext.NotificationSubscriptionOverrides.FirstOrDefaultAsync(c =>
                        c.NotificationTriggerId == trigger.Id && c.UserId == trigger.UpdatedBy);

                if (notificationOverride != null && !isEnabledForUser.Value)
                {
                    notificationOverride.IsEnabled = isEnabledForUser.Value;
                    _notificationDbContext.Update(notificationOverride);

                }
                else if (!isEnabledForUser.Value)
                {
                    await _notificationDbContext.NotificationSubscriptionOverrides.AddAsync(
                        new NotificationSubscriptionOverrideEntity()
                        {
                            UserId = trigger.UpdatedBy.Value,
                            IsEnabled = isEnabledForUser.Value,
                            NotificationTriggerId = trigger.Id
                        });
                }
            }
        }

        var entity = NotificationTriggerEntity.MapFrom(trigger);
        _notificationDbContext.NotificationTriggers.Update(entity);
        await _notificationDbContext.SaveChangesAsync();
        return NotificationTriggerEntity.MapTo(entity);
    }

    public async Task<List<NotificationTriggerEntity>> GetFilteredTriggersAsync(NotificationMessage notificationMessage)
    {
        var triggerIds = new List<Guid>();
        // Apply trigger filter rules based on source type 
        var triggerFilterRules = _triggerFilterRules.Where(x => x.Source.Contains(notificationMessage.Source));
        foreach (var triggerFilterRule in triggerFilterRules)
        {
            var result = await triggerFilterRule.Apply(notificationMessage);
            triggerIds.AddRange(result);
        }
        // Build query based on notification message filters
        // apply default trigger filter
        var defaultTriggerQuery = _notificationDbContext.NotificationTriggers
                                    .Include(x => x.Locations)
                                    .Where(x => x.Source == notificationMessage.Source && x.IsEnabled && !x.IsMuted);
       
        // filter by priority
        if (notificationMessage.Priority.HasValue)
        {
            defaultTriggerQuery = defaultTriggerQuery.Where(x => x.PriorityJson.Contains(notificationMessage.Priority.Value));
        }
        // filter by locations
        if (notificationMessage.Locations.Any())
        {
            defaultTriggerQuery = defaultTriggerQuery
                .Where(x => x.Locations.Count == 0 || x.Locations.Any(l => notificationMessage.Locations.Contains(l.Id)));
        }
        // execute query to get filtered Triggers
        var filteredTriggers = await defaultTriggerQuery
            .Where(x => triggerIds.Contains(x.Id))
            .Include(x => x.WorkgroupSubscriptions)
            .Include(x => x.NotificationSubscriptionOverrides.Where(x => !x.IsEnabled || x.IsMuted))
            .ToListAsync();

        return filteredTriggers;
    }

    public async Task DeleteNotificationTriggerAsync(NotificationTrigger trigger)
    {
        var entity = NotificationTriggerEntity.MapFrom(trigger);
        _notificationDbContext.Locations.RemoveRange(_notificationDbContext.Locations.Where(c => c.NotificationTriggerId == entity.Id));
        _notificationDbContext.NotificationSubscriptionOverrides.RemoveRange(_notificationDbContext.NotificationSubscriptionOverrides.Where(c => c.NotificationTriggerId == entity.Id));
        if (entity.Type == NotificationType.Workgroup)
            _notificationDbContext.WorkgroupSubscriptions.RemoveRange(_notificationDbContext.WorkgroupSubscriptions.Where(c => c.NotificationTriggerId == entity.Id));
        switch (entity.Focus)
        {
            case NotificationFocus.Skill:
                _notificationDbContext.NotificationTriggerSkills.RemoveRange(_notificationDbContext.NotificationTriggerSkills.Where(c => c.NotificationTriggerId == entity.Id));

                break;
            case NotificationFocus.Twin:
                _notificationDbContext.NotificationTriggerTwins.RemoveRange(_notificationDbContext.NotificationTriggerTwins.Where(c => c.NotificationTriggerId == entity.Id));

                break;
            case NotificationFocus.TwinCategory:
                _notificationDbContext.NotificationTriggerTwinCategories.RemoveRange(_notificationDbContext.NotificationTriggerTwinCategories.Where(c => c.NotificationTriggerId == entity.Id));
                break;
            case NotificationFocus.SkillCategory:
                _notificationDbContext.NotificationTriggerSkillCategories.RemoveRange(_notificationDbContext.NotificationTriggerSkillCategories.Where(c => c.NotificationTriggerId == entity.Id));
                break;
        }
        _notificationDbContext.NotificationTriggers.Remove(entity);
        await _notificationDbContext.SaveChangesAsync();
    }

    public async Task BatchToggleNotificationTriggerAsync(BatchNotificationTriggerToggleRequest request)
    {
        var query = _notificationDbContext.NotificationTriggers.Where(x => !x.IsDefault && x.Source == request.Source);

        query = request.IsAdmin? query.Where(x => x.Type == NotificationType.Workgroup || (x.Type == NotificationType.Personal && x.CreatedBy == request.UserId)):
                                 query.Where(x => x.Type == NotificationType.Personal && x.CreatedBy == request.UserId); 

        var triggers = await query.ToListAsync();
        foreach (var trigger in triggers)
        {
            trigger.IsMuted = !trigger.IsMuted;
            _notificationDbContext.Update(trigger);
        }

        if(!request.IsAdmin)
        {
            var userWorkgroupTriggers = await _notificationDbContext.NotificationTriggers.Where(x =>
                !x.IsDefault && x.Source == request.Source && x.Type == NotificationType.Workgroup && x.CanUserDisableNotification
                && x.WorkgroupSubscriptions.Any(c => request.WorkgroupIds.Contains(c.WorkgroupId)))
                .Include(c=>c.NotificationSubscriptionOverrides.Where(x=>x.UserId==request.UserId)).ToListAsync();

            if(userWorkgroupTriggers!=null && userWorkgroupTriggers.Any())
            {
                foreach (var trigger in userWorkgroupTriggers)
                {
                    var notificationOverride =
                        trigger.NotificationSubscriptionOverrides.FirstOrDefault(x => x.UserId == request.UserId);

                    if (notificationOverride != null)
                    {
                        notificationOverride.IsMuted= !notificationOverride.IsMuted;
                        _notificationDbContext.Update(notificationOverride);
                    }
                    else
                    {
                        _notificationDbContext.NotificationSubscriptionOverrides.Add(new NotificationSubscriptionOverrideEntity()
                        {
                            IsEnabled = false,
                            NotificationTriggerId = trigger.Id,
                            UserId = request.UserId,
                            IsMuted = true
                        });
                    }
                }
            }
         
        }
        await _notificationDbContext.SaveChangesAsync();
   
    }
}
