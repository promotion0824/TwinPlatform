using System;
using System.Collections.Generic;

namespace Willow.Calendar
{
    /// <summary>
    /// Specifies an event that has a recurrence
    /// </summary>
    public interface IRecurringEvent
    {
        public Event Recurrence { get; set; }
    }

    /// <summary>
    /// Specifies an event that has a recurrence
    /// </summary>
    public class RecurringEvent : IRecurringEvent
    {
        public Event Recurrence { get; set; }
    }
}
