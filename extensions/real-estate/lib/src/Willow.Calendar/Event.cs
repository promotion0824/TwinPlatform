using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Willow.Calendar
{
    /// <summary>
    /// Specifies a single or recurring event
    /// </summary>
    public class Event
    {
        public string               Name           { get; set; }
        
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm")]
        public DateTime             StartDate      { get; set; }
      
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-ddTHH:mm")]
        public DateTime?            EndDate        { get; set; }
        
        public string               Timezone       { get; set; }
        public Recurrence           Occurs         { get; set; } = Recurrence.Once;
        public int                  MaxOccurrences { get; set; } = int.MaxValue;
        public int                  Interval       { get; set; } = 1;
        public IList<DayOccurrence> DayOccurrences { get; set; } = new List<DayOccurrence>();
        public IList<int>           Days           { get; set; } = new List<int>();
        
        public enum Recurrence
        {
            Once    = 0,
            Daily   = 1,
            Weekly  = 2,
            Monthly = 3,
            Yearly  = 4,

            Minutely = 5,
            Hourly   = 6
        }

        public class DayOccurrence
        {
            public int       Ordinal   { get; set; } // 1=first, 2=second, -1=last
            public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
        }
    }
}
