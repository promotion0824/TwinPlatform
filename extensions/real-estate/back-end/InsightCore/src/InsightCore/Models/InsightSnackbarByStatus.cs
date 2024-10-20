using InsightCore.Infrastructure.Configuration;
using InsightCore.Models;
using System;

namespace InsightCore.Dto
{
    public class InsightSnackbarByStatus
    {
        public Guid? Id { get; set; }
        public InsightStatus Status { get; set; }
        public int Count { get; set; }
        public SourceType? SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string SourceName { get; set; }
    }
}
