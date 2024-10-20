using InsightCore.Models;
using Microsoft.Extensions.Configuration;
using Segment;
using Segment.Model;
using System;
using System.Collections.Generic;

namespace InsightCore.Services
{
    public interface IAnalyticsService
    {
        void TrackInsightCreation(InsightType type, string name, string description, int priority, DateTime occurredDate, Dictionary<string, string> properties);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private const string IdentityUserId = "WillowCommand";
        private const string IdentityFirstName = "Willow";
        private const string IdentityLastName = "Command";

        public AnalyticsService(IConfiguration configuration)
        {
            var segmentKey = configuration.GetValue<string>("Segment:ApiKey");
            Analytics.Initialize(segmentKey);
        }

        public void TrackInsightCreation(InsightType type, string name, string description, int priority, DateTime occurredDate, Dictionary<string, string> properties)
        {
            Analytics.Client.Identify(IdentityUserId, new Traits {
                { "firstName", IdentityFirstName },
                { "lastName", IdentityLastName },
                { "name", $"{IdentityFirstName} {IdentityLastName}" },
                { "company", properties.GetValueOrDefault("Company", string.Empty) }
            });

            var trackProperties = new Properties {
                { "insight_type", type },
                { "summary", name},
                { "description", description },
                { "priority", priority },
                { "occurred_date", occurredDate }
            };
            foreach (var property in properties)
            {
                trackProperties.Add(property.Key, property.Value);
            }
            Analytics.Client.Track(IdentityUserId, "Insight_Created", trackProperties);
        }
    }
}
