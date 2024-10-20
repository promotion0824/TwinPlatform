using SiteCore.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SiteCore.Domain
{
    public class Metric
    {
        private static readonly CultureInfo UsCultureInfo = CultureInfo.GetCultureInfo("en-US");

        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string FormatString { get; set; }
        public decimal WarningLimit { get; set; }
        public decimal ErrorLimit { get; set; }
        public string Tooltip { get; set; }
        public List<MetricValue> Values { get; set; } = new List<MetricValue>();
        public List<Metric> Metrics { get; set; } = new List<Metric>();

        public decimal? Average => 
            Values != null && Values.Any() ? Values.Average(v => v.Value) : (decimal?)null;

        public string FormattedAverage => 
            Average?.ToString(FormatString, UsCultureInfo);

        public MetricStatus? Status
        {
            get
            {
                var average = Average;

                if (average == null)
                {
                    return null;
                }

                if (WarningLimit > ErrorLimit ||  (ErrorLimit == WarningLimit && ErrorLimit != 0))
                {
                    if (average > WarningLimit)
                    {
                        return MetricStatus.Ok;
                    }
                    else if (average <= ErrorLimit) 
                    {
                        return MetricStatus.Error;
                    }
                    else
                    {
                        return MetricStatus.Warning;
                    }
                }
                else if (ErrorLimit > WarningLimit)
                {
                    if (average < WarningLimit)
                    {
                        return MetricStatus.Ok;
                    }
                    else if (average >= ErrorLimit)
                    {
                        return MetricStatus.Error;
                    }
                    else
                    {
                        return MetricStatus.Warning;
                    }
                }
                else
                {
                    return MetricStatus.Ok;
                }
            }
        }

        public static Metric MapFrom(MetricEntity entity) => new Metric
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            FormatString = entity.FormatString,
            WarningLimit = entity.WarningLimit,
            ErrorLimit = entity.ErrorLimit,
            Tooltip = entity.Tooltip
        };
    }

    public enum MetricStatus
    {
        Ok,
        Warning,
        Error
    }
}
