using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class OpcUaPointProperties
    {
        public string NodeId { get; set; }

        internal static OpcUaPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new OpcUaPointProperties
            {
                NodeId = property.GetStringValueOrDefault(Properties.NodeID)
            };
        }

        internal static OpcUaPointProperties MapFromProperty(JObject property)
        {
            return new OpcUaPointProperties
            {
                NodeId = property[Properties.NodeID].ToString()
            };
        }
    }
}
