using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    [Serializable]
    public class ModbusPointProperties
    {
        public int DataType { get; set; }
        public int RegisterAddress { get; set; }
        public bool Swap { get; set; }
        public int SlaveId { get; set; }
        public ModbusEndianType Endian { get; set; }
        public float Scale { get; set; }
        public int RegisterType { get; set; }

        public enum ModbusEndianType
        {
            Unknown = -1,
            EndianBig,
            EndianLittle
        }

        internal static ModbusPointProperties MapFromProperty(IDictionary<string, object> property)
        {
            return new ModbusPointProperties
            {
                DataType = property.GetIntValueOrDefault("dataType"),
                Endian = MapEndian(property),
                RegisterType = property.GetIntValueOrDefault("registerType"),
                RegisterAddress = property.GetIntValueOrDefault("registerAddress"),
                Swap = property.GetBoolValueOrDefault("swap"),
                Scale = property.GetFloatValueOrDefault("scale"),
                SlaveId = property.GetIntValueOrDefault(Properties.SlaveId),
            };
        }

        internal static ModbusPointProperties MapFromProperty(JObject property)
        {
            return new ModbusPointProperties
            {
                DataType = (int)property["dataType"],
                Endian = MapEndian(property),
                RegisterType = (int)property["registerType"],
                RegisterAddress = (int)property["registerAddress"],
                Swap = property.TryGetValue("swap", out var swapVal) && (bool)swapVal,
                Scale = (float)property["scale"],
                SlaveId = (int)property[Properties.SlaveId],
            };
        }

        private static ModbusEndianType MapEndian(JObject property)
        {
            return property["endian"].ToString() switch
            {
                "EndianBig" => ModbusEndianType.EndianBig,
                "EndianLittle" => ModbusEndianType.EndianLittle,
                _ => ModbusEndianType.Unknown,
            };
        }

        private static ModbusEndianType MapEndian(IDictionary<string, object> property)
        {
            return  property.GetStringValueOrDefault("endian") switch
            {
                "EndianBig" => ModbusEndianType.EndianBig,
                "EndianLittle" => ModbusEndianType.EndianLittle,
                _ => ModbusEndianType.Unknown,
            };
        }
    }
}
