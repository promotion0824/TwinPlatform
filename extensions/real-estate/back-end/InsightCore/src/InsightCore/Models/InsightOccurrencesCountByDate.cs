using System;

namespace InsightCore.Models
{
    public class InsightOccurrencesCountByDate
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public double AverageDuration { get; set; }
    }
}
