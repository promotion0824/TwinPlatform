using System.Collections.Generic;
using System;
using WorkflowCore.Models;

namespace WorkflowCore.Services.Apis
{
    public class BatchUpdateInsightStatusRequest
    {
        public IEnumerable<Guid> Ids { get; set; }
        public InsightStatus Status { get; set; }
        public string Reason { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public Guid? SourceId { get; set; }
    }
}
