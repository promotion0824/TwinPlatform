using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Scheduler
{
    public interface IScheduleRecipient
    {
        /// <summary>
        /// Loads all schedules to determine which schedules need to run
        /// </summary>
        Task PerformScheduleHit(ScheduleHit scheduleHit, string language);
    }
}
