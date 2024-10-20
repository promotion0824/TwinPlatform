using InsightCore.Models;
using InsightCore.Services;
using System;
using System.Collections.Generic;

namespace InsightCore.Test.MockServices
{
    public class MockAnalyticsService : IAnalyticsService
    {
        private readonly IDictionary<string, string> _createdInsights = new Dictionary<string, string>();

        public void TrackInsightCreation(InsightType type, string name, string description, int priority, DateTime occurredDate, Dictionary<string, string> properties)
        {
            _createdInsights.Add(name, description);
        }
    }
}
