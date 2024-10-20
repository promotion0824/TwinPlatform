using System;
using Azure;
using Azure.Data.Tables;

namespace Willow.IoTService.Monitoring.Entities
{
    public class AlertRunEntity : ITableEntity
    {
        public string? AlertType { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}