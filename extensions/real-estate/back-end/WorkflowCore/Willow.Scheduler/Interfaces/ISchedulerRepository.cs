using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Scheduler
{
    public interface ISchedulerRepository
    {
        /// <summary>
        /// Loads all schedules 
        /// </summary>
        Task<IEnumerable<Schedule>> GetSchedules();

        // Loads schedules that belong to the provided owner ids
        Task<IEnumerable<Schedule>> GetSchedulesByOwnerId(IList<Guid> ownerIds);

        Task DeleteSchedule(Guid scheduleId);
    }
}
