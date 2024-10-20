using System;

namespace WorkflowCore.Controllers.Responses
{
    public class SubmitCheckRecordResponse
    {
        public enum InsightType
        {
            Alert = 3,
            Note = 4
        }

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
