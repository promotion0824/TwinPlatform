using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Repository
{
    public static class InspectionHelper
    {
        public static CheckRecordStatus? CalculateSummaryStatus(IList<CheckRecordStatus> statuses)
        {
            int GetSeverityLevel(CheckRecordStatus status)
            {
                switch (status)
                {
                    case CheckRecordStatus.Overdue: return 1000;
                    case CheckRecordStatus.Due: return 900;
                    case CheckRecordStatus.Missed: return 800;
                    case CheckRecordStatus.Completed: return 700;
                    case CheckRecordStatus.NotRequired: return 600;
                }
                throw new ArgumentOutOfRangeException(nameof(status), $"Unknown status {status}");
            }

            if (statuses.Count == 0)
            {
                return null;
            }
            var summarySeverityLevel = statuses.Select(x => GetSeverityLevel(x)).Max();
            switch (summarySeverityLevel)
            {
                case 1000: return CheckRecordStatus.Overdue;
                case 900: return CheckRecordStatus.Due;
                case 800: return CheckRecordStatus.Missed;
                case 700: return CheckRecordStatus.Completed;
                default: return CheckRecordStatus.NotRequired;
            }
        }
        
    }
}
