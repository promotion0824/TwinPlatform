using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Calendar;

namespace WorkflowCore.Dto
{
    public class EventDto
    {
        public string               Name           { get; set; }
        public string               StartDate      { get; set; }
        public string               EndDate        { get; set; }
        public string               Timezone       { get; set; }
        public Event.Recurrence     Occurs         { get; set; } = Event.Recurrence.Once;
        public int                  MaxOccurrences { get; set; } = int.MaxValue;
        public int                  Interval       { get; set; } = 1;
        public IList<Event.DayOccurrence> DayOccurrences { get; set; } = new List<Event.DayOccurrence>();
        public IList<int>           Days           { get; set; } = new List<int>();

        public static Event MapToModel(EventDto dto)
        {
            return new Event
            {
                Name             = dto.Name,   
                StartDate        = DateTime.Parse(dto.StartDate),
                EndDate          = string.IsNullOrWhiteSpace(dto.EndDate) ? null : DateTime.Parse(dto.EndDate),
                Timezone         = dto.Timezone,
                Occurs           = dto.Occurs,
                MaxOccurrences   = dto.MaxOccurrences,
                Interval         = dto.Interval,
                DayOccurrences   = dto.DayOccurrences,
                Days             = dto.Days,
            };
        }
    }
}
