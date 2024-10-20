using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Calendar;

namespace Willow.Scheduler
{
    public class ScheduleHit
    {
        public Guid     ScheduleId { get; set; }
        public Guid     OwnerId    { get; set; }
        public DateTime HitDate    { get; set; }
        public string   EventName  { get; set; }
		public Event.Recurrence Recurrence { get; set; }
	}
}
