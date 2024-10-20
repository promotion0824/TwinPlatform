using System;
using System.Collections.Generic;

namespace InsightCore.Dto
{
    public class DiagnosticsSnapshotDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RuleName { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Ended { get; set; }
        public bool Check { get; set; }
        public List<DiagnosticsSnapshotDto> Diagnostics { get; set; }
    }
}
