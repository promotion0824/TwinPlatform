using NotificationCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace NotificationCore.TriggerFilterRules;


/// <summary>
/// Represents a filter rule for triggering notifications.
/// </summary>
public interface ITriggerFilterRule
{
    /// <summary>
    /// Gets the source of the notification message that the filter rule applies to.
    /// Each source type can have different filter rules.
    /// </summary>
    NotificationSource[] Source { get; }

    /// <summary>
    /// Apply the filter rule to the message and return the trigger ids that match the rule.
    /// </summary>
    /// <param name="message">The notification message to apply the filter rule to.</param>
    /// <returns>Result contains a list of trigger ids that match the filter.</returns>
    Task<List<Guid>> Apply(NotificationMessage message);
}

