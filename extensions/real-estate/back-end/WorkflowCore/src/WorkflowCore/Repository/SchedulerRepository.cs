using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Scheduler;
using WorkflowCore.Entities;

using Microsoft.EntityFrameworkCore;

namespace WorkflowCore.Repository
{
    public class SchedulerRepository : ISchedulerRepository
    {
        private readonly WorkflowContext _context;

        public SchedulerRepository(WorkflowContext context)
        {
            _context = context;
        }
        
        public Task<IEnumerable<Schedule>> GetSchedules()
        {
            var result = _context.Schedules.Where( s=> s.Active).Select(ScheduleEntity.MapToModel);

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Schedule>> GetSchedulesByOwnerId(IList<Guid> ownerIds)
        {
            var result = _context.Schedules.Where( s=> s.Active && ownerIds.Contains(s.OwnerId) ).Select(ScheduleEntity.MapToModel);

            return Task.FromResult(result);
        }

        public async Task DeleteSchedule(Guid scheduleId)
        {
            var schedule = _context.Schedules.Find(scheduleId);

            if(schedule != null)
            { 
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }
    }
}
