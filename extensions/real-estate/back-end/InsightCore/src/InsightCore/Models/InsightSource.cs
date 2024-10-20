using InsightCore.Models;
using System;

namespace InsightCore.Dto
{
    public class InsightSource
    {
        public SourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
    }
}
