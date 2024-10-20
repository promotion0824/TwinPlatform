using System;
using System.Linq;
using System.Collections.Generic;
using InsightCore.Models;

namespace InsightCore.Dto
{
    public class InsightSourceDto
    {
        public SourceType SourceType { get; set; }
        public string SourceName { get; set; }
        public Guid? SourceId { get; set; }

        public static InsightSourceDto MapFromModel(InsightSource insightSource, string sourceName)
        {
            return new InsightSourceDto
            {
                SourceId = insightSource.SourceId,
                SourceName = insightSource.SourceType == SourceType.App ? sourceName : $"{insightSource.SourceType}",
                SourceType = insightSource.SourceType
            };
        }

    }
}
