using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteCore.Entities
{
    [Table("SiteMetricValues")]
    public class SiteMetricValueEntity
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public Guid MetricId { get; set; }
        public decimal Value { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
