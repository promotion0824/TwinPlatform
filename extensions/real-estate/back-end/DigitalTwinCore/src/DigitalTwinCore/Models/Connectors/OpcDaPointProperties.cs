using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class OpcDaPointProperties
    {
        public string NodeId { get; set; }

        internal static OpcDaPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new OpcDaPointProperties
            {
                NodeId = property.GetStringValueOrDefault(Properties.NodeID)
            };
        }

        internal static OpcDaPointProperties MapFromProperty(JObject property)
        {
            return new OpcDaPointProperties
            {
                NodeId = property[Properties.NodeID].ToString()
            };
        }
    }
}
