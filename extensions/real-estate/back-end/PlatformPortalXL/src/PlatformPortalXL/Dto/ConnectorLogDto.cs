using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class ConnectorLogDto
    {
        public long Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public Guid ConnectorId { get; set; }

        public int PointCount { get; set; }
        public int RetryCount { get; set; }
        
        public int ErrorCount { get; set; }

        public string Source { get; set; }

        private static ConnectorLogDto MapFrom(ConnectorLogRecord record)
        {
            return new ConnectorLogDto
            {
                Id =  record.Id,
                Source = record.Source,
                ConnectorId = record.ConnectorId,
                EndTime = record.EndTime,
                ErrorCount = record.ErrorCount,
                PointCount = record.PointCount,
                StartTime = record.StartTime,
                RetryCount = record.RetryCount
            };
        }
        public static List<ConnectorLogDto> MapFrom(List<ConnectorLogRecord> recordsCore)
        {
            return recordsCore.Select(MapFrom).ToList();
        }

    }
}
