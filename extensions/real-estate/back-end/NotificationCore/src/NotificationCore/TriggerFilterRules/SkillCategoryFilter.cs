using Microsoft.EntityFrameworkCore;
using NotificationCore.Entities;
using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationCore.TriggerFilterRules;


/// <summary>
/// Represents a filter rule based on skill category (insight type) for triggering notifications.
/// this filter rule applies to the Insight notification source.
/// </summary>
public class SkillCategoryFilter : ITriggerFilterRule
{
    private readonly NotificationDbContext _notificationDbContext;

    public SkillCategoryFilter(NotificationDbContext notificationDbContext)
    {
        _notificationDbContext = notificationDbContext;
    }

    public NotificationSource[] Source => new[] { NotificationSource.Insight };

    /// <summary>
    /// Applies the skill category (insight type) filter rule to the given notification message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <returns>A list of trigger IDs that match the skill category filter.</returns>
    public async Task<List<Guid>> Apply(NotificationMessage message)
    {
        var triggerIds = new List<Guid>();
        if (message.SkillCategoryId.HasValue)
        {
            triggerIds = await _notificationDbContext.NotificationTriggerSkillCategories
                .Where(x => x.CategoryId == message.SkillCategoryId)
                .Select(x => x.NotificationTriggerId)
                .ToListAsync();
        }
        return triggerIds;
    }
}
