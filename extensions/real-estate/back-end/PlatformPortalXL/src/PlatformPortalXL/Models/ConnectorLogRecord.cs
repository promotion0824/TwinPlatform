using System;

namespace PlatformPortalXL.Models
{
    public class ConnectorLogRecord
    {
        public long Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public Guid ConnectorId { get; set; }

        public int PointCount { get; set; }

        public int ErrorCount { get; set; }

        public string Source { get; set; }
        
        public int RetryCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Errors { get; set; }
    }
}
