using Microsoft.EntityFrameworkCore;
using NotificationCore.Entities;
using NotificationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationCore.TriggerFilterRules;


/// <summary>
/// Represents a filter rule based on twin category (twin primary model) for triggering notifications.
/// </summary>
public class TwinCategoryFilter : ITriggerFilterRule
{
    private readonly NotificationDbContext _notificationDbContext;

    public TwinCategoryFilter(NotificationDbContext notificationDbContext)
    {
        _notificationDbContext = notificationDbContext;
    }

    public NotificationSource[] Source => new[] { NotificationSource.Insight };

    /// <summary>
    /// Applies the twin category (twin model id) filter rule to the given notification message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    /// <returns>A list of trigger IDs that match the twin category filter rule.</returns>
    public async Task<List<Guid>> Apply(NotificationMessage message)
    {
        var triggerIds = new List<Guid>();
        if (!string.IsNullOrWhiteSpace(message.ModelId))
        {
            triggerIds = await _notificationDbContext.NotificationTriggers
                 .Where(x => x.Focus == NotificationFocus.TwinCategory)
                 .Include(x => x.TwinCategories)
                 .Where(x => x.TwinCategories.Count == 0 || x.TwinCategories.Any(y => y.CategoryId == message.ModelId))
                 .Select(x => x.Id)
                 .ToListAsync();
        }
        return triggerIds;
    }
}

