using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.DataValidation;
using System.ComponentModel.DataAnnotations;

namespace Willow.Workflow
{
    public class EventDto
    {
        [HtmlContent]
        public string               Name           { get; set; }

        [Required]
        [DateAsString(ErrorMessage = "StartDate is not a valid datetime")]
        public string               StartDate      { get; set; } // DO NOT MAKE THESE DateTime or it will get converted to UTC

        [DateAsString(ErrorMessage = "EndDate is not a valid datetime")]
        public string               EndDate        { get; set; }

        public string               Timezone       { get; set; }

        [Range(0, 6, ErrorMessage = "Occurs value must be between 0 and 6: 0=Once, 1=Daily, 2=Weekly, 3=Monthly, 4=Yearly, 5=Minutely, 6=Hourly")]
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
