using System;

namespace InsightCore.Dto
{
    public class InsightStatisticsByPriority
    {
        public Guid Id { get; set; }
        public int OpenCount { get; set; }
        public int UrgentCount { get; set; }
        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }
    }
}
