using System;

namespace WorkflowCore.Dto
{
    public class SiteStatistics
    {
        public Guid Id              { get; set; }
        public int  OverdueCount    { get; set; }
        public int  UrgentCount     { get; set; }
        public int  HighCount       { get; set; }
        public int  MediumCount     { get; set; }
        public int  LowCount        { get; set; }
        public int  OpenCount       { get; set; }
    }
}