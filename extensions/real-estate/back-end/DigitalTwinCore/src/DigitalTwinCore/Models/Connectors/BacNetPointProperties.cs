using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class BacNetPointProperties
    {
        public int DeviceId { get; set; }
        public int ObjectId { get; set; }

        public BacNetPointObjectType ObjectType { get; set; }

        public enum BacNetPointObjectType
        {
            Unknown,
            AnalogInput,
            AnalogOutput,
            AnalogValue,
            BinaryInput,
            BinaryOutput,
            BinaryValue,
            Device,
            MultistateInput,
            MultistateOutput,
            MultistateValue
        }

        internal static BacNetPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new BacNetPointProperties
            {
                DeviceId = property.GetIntValueOrDefault(Properties.DeviceId),
                ObjectId = property.GetIntValueOrDefault(Properties.ObjectId),
                ObjectType = MapObjectType(property)
            };
        }

        internal static BacNetPointProperties MapFromProperty(JObject property)
        {
            return new BacNetPointProperties
            {
                DeviceId = (int)property[Properties.DeviceId],
                ObjectId = (int)property[Properties.ObjectId],
                ObjectType = MapObjectType(property)
            };
        }

        private static BacNetPointObjectType MapObjectType(IDictionary<string, object> property)
        {
            return property.GetStringValueOrDefault(Properties.ObjectType) switch
            {
                "AI" => BacNetPointObjectType.AnalogInput,
                "AO" => BacNetPointObjectType.AnalogOutput,
                "AV" => BacNetPointObjectType.AnalogValue,
                "BI" => BacNetPointObjectType.BinaryInput,
                "BO" => BacNetPointObjectType.BinaryOutput,
                "BV" => BacNetPointObjectType.BinaryValue,
                "DEV" => BacNetPointObjectType.Device,
                "MSI" => BacNetPointObjectType.MultistateInput,
                "MSO" => BacNetPointObjectType.MultistateOutput,
                "MSV" => BacNetPointObjectType.MultistateValue,
                _ => BacNetPointObjectType.Unknown
            };
        }

        private static BacNetPointObjectType MapObjectType(JObject property)
        {
            return property[Properties.ObjectType].ToString() switch
            {
                "AI" => BacNetPointObjectType.AnalogInput,
                "AO" => BacNetPointObjectType.AnalogOutput,
                "AV" => BacNetPointObjectType.AnalogValue,
                "BI" => BacNetPointObjectType.BinaryInput,
                "BO" => BacNetPointObjectType.BinaryOutput,
                "BV" => BacNetPointObjectType.BinaryValue,
                "DEV" => BacNetPointObjectType.Device,
                "MSI" => BacNetPointObjectType.MultistateInput,
                "MSO" => BacNetPointObjectType.MultistateOutput,
                "MSV" => BacNetPointObjectType.MultistateValue,
                _ => BacNetPointObjectType.Unknown
            };
        }
    }
}
