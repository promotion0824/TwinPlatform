using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileXL.Models
{
    // This code was copy and pasted from the Scheduler repo. Need to move code into a shared NuGet
    public class Event
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public Recurrence Occurs { get; set; } = Recurrence.Once;
        public int MaxOccurrences { get; set; } = int.MaxValue;
        public int Interval { get; set; } = 1;
        public IList<DayOccurrence> DayOccurrences { get; set; } = new List<DayOccurrence>();
        public IList<int> Days { get; set; } = new List<int>();

        public enum Recurrence
        {
            Once = 0,
            Daily = 1,
            Weekly = 2,
            Monthly = 3,
            Yearly = 4
        }

        public class DayOccurrence
        {
            public int Ordinal { get; set; } // 1=first, 2=second, -1=last
            public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
        }
    }
}
