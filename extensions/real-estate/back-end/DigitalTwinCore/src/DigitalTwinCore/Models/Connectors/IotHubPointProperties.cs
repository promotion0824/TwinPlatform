using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class IotHubPointProperties
    {
        public string ExternalId { get; set; }

        internal static IotHubPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new IotHubPointProperties
            {
                ExternalId = property.GetStringValueOrDefault(Properties.ExternalID)
            };
        }

        internal static IotHubPointProperties MapFromProperty(JObject property)
        {
            return new IotHubPointProperties
            {
                ExternalId = property[Properties.ExternalID].ToString()
            };
        }
    }
}
