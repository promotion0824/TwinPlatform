using Microsoft.EntityFrameworkCore;
using NotificationCore.Entities;
using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationCore.TriggerFilterRules;


/// <summary>
/// Represents a filter rule that filters triggers based on the twin ID of the notification message.
/// </summary>
public class TwinFilter : ITriggerFilterRule
{
    private readonly NotificationDbContext _notificationDbContext;

    public TwinFilter(NotificationDbContext notificationDbContext)
    {
        _notificationDbContext = notificationDbContext;
    }

    public NotificationSource[] Source => new[] { NotificationSource.Insight };

    /// <summary>
    /// Applies the twin filter rule to the given notification message.
    /// </summary>
    /// <param name="message">The notification message to apply the filter rule to.</param>
    /// <returns>A list of trigger IDs that match the twin ID of the notification message.</returns>
    public async Task<List<Guid>> Apply(NotificationMessage message)
    {
        var triggerIds = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(message.TwinId))
        {
            triggerIds = await _notificationDbContext.NotificationTriggerTwins
                .Where(x => x.TwinId == message.TwinId)
                .Select(x => x.NotificationTriggerId)
                .ToListAsync();
        }
        return triggerIds;
    }
}

