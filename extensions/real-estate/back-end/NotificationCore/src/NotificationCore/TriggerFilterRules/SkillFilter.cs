using Microsoft.EntityFrameworkCore;
using NotificationCore.Entities;
using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationCore.TriggerFilterRules;


/// <summary>
/// Represents a filter rule based on skill (Insight Rule Id).
/// this filter rule applies to the Insight notification source.
/// </summary>
public class SkillFilter : ITriggerFilterRule
{
    private readonly NotificationDbContext _notificationDbContext;

    public SkillFilter(NotificationDbContext notificationDbContext)
    {
        _notificationDbContext = notificationDbContext;
    }

    public NotificationSource[] Source => [NotificationSource.Insight];

    /// <summary>
    /// Applies the skill filter rule (insight rule id) to the given notification message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <returns>A list of trigger IDs that match the skill filter rule.</returns>
    public async Task<List<Guid>> Apply(NotificationMessage message)
    {
        var triggerIds = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(message.SkillId))
        {
            triggerIds = await _notificationDbContext.NotificationTriggerSkills
                .Where(x => x.SkillId == message.SkillId)
                .Select(x => x.NotificationTriggerId)
                .ToListAsync();
        }
        return triggerIds;
    }
}

