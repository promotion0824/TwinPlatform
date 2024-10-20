using System;

namespace InsightCore.Dto
{
    public class InsightStatisticsByStatus
    {
        public Guid Id { get; set; }
        public int InProgressCount { get; set; }
        public int ReadyToResolveCount { get; set; }
        public int NewCount { get; set; }
        public int OpenCount { get; set; }
        public int IgnoredCount { get; set; }
        public int ResolvedCount { get; set; }
        public int AutoResolvedCount { get; set; }
    }
}
