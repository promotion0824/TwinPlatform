using System;
using System.Collections.Generic;
using System.Linq;

namespace KPI.Service.Models
{
    public class KPIResponse
    {
        public string PortfolioId { get; set; }
        public Guid? SiteId { get; set; }
        public string XValue { get; set; }
        public string YValue { get; set; }
        public string YUOM { get; set; }
    }

    public class DatedKPIValuesResponse
    {
        public string Name { get; set; }

        public double Average
        {
            get
            {
                return ValuesByDate?.Average(c => c.Value) ?? 0;
            }
        }

        public string Unit { get; set; }
        public List<DatedValue> ValuesByDate { get; set; }
    }

    public class DatedKPIValuesWithTrendResponse : DatedKPIValuesResponse
    {
        public Trend Trend { get; set; }
    }

    public class Trend
    {
        public double Difference { get; set; }
        public string Unit { get; set; }
        public double Average { get; set; }
        public string Sentiment
        {
            get
            {
                return Difference switch
                {
                    > 0 => "Positive",
                    < 0 => "Negative",
                    _ => "Neutral"
                };
            }
        }
    }

    public class DatedValue
    {
        public object Date { get; set; }
        public double Value { get; set; }

    }
}
