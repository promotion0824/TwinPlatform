using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class ApiPointProperties
    {
        public string ExternalId { get; set; }

        internal static ApiPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new ApiPointProperties
            {
                ExternalId = property.GetStringValueOrDefault(Properties.ExternalID)
            };
        }

        internal static ApiPointProperties MapFromProperty(JObject property)
        {
            return new ApiPointProperties
            {
                ExternalId = property[Properties.ExternalID].ToString()
            };
        }
    }
}
