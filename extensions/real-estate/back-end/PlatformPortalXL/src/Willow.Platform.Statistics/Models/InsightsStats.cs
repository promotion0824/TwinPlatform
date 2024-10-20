namespace Willow.Platform.Statistics
{
    public class InsightsStats
    {
        public int OpenCount    { get; set; }
        public int UrgentCount  { get; set; }
        public int HighCount    { get; set; }
        public int MediumCount  { get; set; }
        public int LowCount     { get; set; }
    }

    public class InsightsStatsByStatus
    {
        public int InProgressCount { get; set; }
        public int NewCount { get; set; }
        public int OpenCount { get; set; }
        public int IgnoredCount { get; set; }
        public int ResolvedCount { get; set; }
    }
}
