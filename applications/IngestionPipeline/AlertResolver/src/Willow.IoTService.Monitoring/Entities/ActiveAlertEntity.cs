using System;
using Azure;
using Azure.Data.Tables;

namespace Willow.IoTService.Monitoring.Entities
{
    public class ActiveAlertEntity : ITableEntity
    {
        public string? AlertKey { get; set; }
        public DateTime AlertRaised { get; set; }
        public DateTime LatestOccurence { get; set; }
        public int AlertCount { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}