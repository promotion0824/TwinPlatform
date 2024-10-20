using System;
using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Constants
{
    public static class TelemetryCustomDimensions
    {
        public const string AppConfigId = "AppConfigId";

        public const string CustomerId = "CustomerId";

        public const string SiteId = "SiteId";

        public const string CorrelationId = "CorrelationId";

        public const string ErrorCode = "ErrorCode";

        public const string EventSource = "EventSource";

        public static IDictionary<string, string> New()
        {
            return new Dictionary<string, string>();
        }

        public static IDictionary<string, string> WithCustomerId(this IDictionary<string, string> dictionary, Guid value)
        {
            dictionary.TryAdd(CustomerId, value.ToString());
            return dictionary;
        }

        public static IDictionary<string, string> WithAppConfigId(this IDictionary<string, string> dictionary, Guid value)
        {
            dictionary.TryAdd(AppConfigId, value.ToString());
            return dictionary;
        }

        public static IDictionary<string, string> WithEventSource(this IDictionary<string, string> dictionary, string value)
        {
            dictionary.TryAdd(EventSource, value);
            return dictionary;
        }
    }
}

