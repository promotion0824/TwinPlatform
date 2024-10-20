using System;
using MobileXL.Models;

namespace MobileXL.Services.Apis.WorkflowApi.Responses
{
    public class WorkflowSubmitCheckRecordResponse
    {
        public class InsightInformation
        {
            public string TwinId { get; set; }
			public InsightType Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Priority { get; set; }
        }

        public InsightInformation RequiredInsight { get; set; }
    }
}
