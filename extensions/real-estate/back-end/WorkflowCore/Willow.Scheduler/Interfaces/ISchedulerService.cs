using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Willow.Calendar;

namespace Willow.Scheduler
{
    public interface ISchedulerService
    {
        /// <summary>
        /// Loads all schedules to determine which schedules need to run
        /// </summary>
        Task CheckSchedules(DateTime dtNow, string language);

        /// <summary>
        /// Loads all schedules for a given list of owner ids
        /// </summary>
        Task<IEnumerable<(Schedule Schedule, Event Event, DateTime SiteDateTime)>> GetMatchingByOwnerIds(IList<Guid> ownerIds, DateTime dtCheck);

        /// <summary>
        /// Get schedules for the ticket templates for a given site
        /// </summary>
        Task<List<ScheduleHit>> GetSchedulesByOwnerId(DateTime utcNow, IList<Guid> ownerIds);
    }
}
