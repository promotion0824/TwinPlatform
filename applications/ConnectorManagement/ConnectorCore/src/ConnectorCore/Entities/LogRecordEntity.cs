namespace ConnectorCore.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    internal class LogRecordEntity
    {
        public long Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public Guid ConnectorId { get; set; }

        [NotMapped]
        public ConnectorEntity Connector { get; set; }

        public int PointCount { get; set; }

        public int ErrorCount { get; set; }

        public int RetryCount { get; set; }

        public string Errors { get; set; }

        public string Source { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
