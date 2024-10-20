using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Models.Connectors
{
    public class PointCommunication
    {
        public PointCommunicationProtocol Protocol { get; set; }
        public ApiPointProperties Api { get; set; }
        public BacNetPointProperties BacNet { get; set; }
        public ModbusPointProperties Modbus { get; set; }
        public OpcDaPointProperties OpcDa { get; set; }
        public OpcUaPointProperties OpcUa { get; set; }
        public IotHubPointProperties IotHub { get; set; }

        internal static PointCommunication MapFromCustomProperty(JObject property)
        {
            T getComponentProp<T>(string key, Func<JObject, T> mapper) =>
                property.GetValueOrDefault(key) switch
                {
                    JObject d => mapper(d),
                    _ => default,
                };

            var output = new PointCommunication
            {
                Protocol = MapProtocol(property),
            };

            switch (output.Protocol)
            {
                case PointCommunicationProtocol.Api:
                    {
                        output.Api = getComponentProp<ApiPointProperties>("api", ApiPointProperties.MapFromProperty);
                        break;
                    }
                case PointCommunicationProtocol.BACnet:
                    {
                        output.BacNet = getComponentProp<BacNetPointProperties>("baCnet", BacNetPointProperties.MapFromProperty);
                        break;
                    }
                case PointCommunicationProtocol.IotHub:
                    {
                        output.IotHub = getComponentProp<IotHubPointProperties>("ioTHub", IotHubPointProperties.MapFromProperty);
                        break;
                    }
                case PointCommunicationProtocol.Modbus:
                    {
                        output.Modbus = getComponentProp<ModbusPointProperties>("modbus", ModbusPointProperties.MapFromProperty);
                        break;
                    }
                case PointCommunicationProtocol.OpcDa:
                    {
                        output.OpcDa = getComponentProp<OpcDaPointProperties>("opcda", OpcDaPointProperties.MapFromProperty);
                        break;
                    }
                case PointCommunicationProtocol.OpcUa:
                    {
                        output.OpcUa = getComponentProp<OpcUaPointProperties>("opcua", OpcUaPointProperties.MapFromProperty);
                        break;
                    }
            }

            return output;
        }

        internal static PointCommunication MapFromComponent(IDictionary<string, object> component)
        {
            // TODO: Replace static calls with table-driven IPointCommunicationAdapter implementations (assemblies could be loaded at run-time as well) or use inheritance or case-classes 
            T getComponentProp<T>(string key, Func<IDictionary<string,object>,T> mapper) =>
                component.GetValueOrDefaultIgnoreCase(key) switch
                {
                    IDictionary<string,object> d => mapper(d),
                    _ => default,
                };

            var output = new PointCommunication
            {
                Protocol = MapProtocol(component),
            };

            switch (output.Protocol)
            {
                case PointCommunicationProtocol.Api:
                {
                    output.Api = getComponentProp<ApiPointProperties>("api", ApiPointProperties.MapFromProperty);
                    break;
                }
                case PointCommunicationProtocol.BACnet:
                {
                    output.BacNet = getComponentProp<BacNetPointProperties>("baCnet", BacNetPointProperties.MapFromProperty);
                    break;
                }
                case PointCommunicationProtocol.IotHub:
                {
                    output.IotHub = getComponentProp<IotHubPointProperties>("ioTHub", IotHubPointProperties.MapFromProperty);
                    break;
                }
                case PointCommunicationProtocol.Modbus:
                {
                    output.Modbus = getComponentProp<ModbusPointProperties>("modbus", ModbusPointProperties.MapFromProperty);
                    break;
                }
                case PointCommunicationProtocol.OpcDa:
                {
                    output.OpcDa = getComponentProp<OpcDaPointProperties>("opcda", OpcDaPointProperties.MapFromProperty);
                    break;
                }
                case PointCommunicationProtocol.OpcUa: 
                {
                    output.OpcUa = getComponentProp<OpcUaPointProperties>("opcua", OpcUaPointProperties.MapFromProperty);
                    break;
                }
            }

            return output;
        }

        private static PointCommunicationProtocol MapProtocol(IDictionary<string, object> component)
        {
            if (!component.ContainsKey("protocol"))
            {
                return PointCommunicationProtocol.Unknown;
            }

            return (component["protocol"] as string) switch
            {
                "API" => PointCommunicationProtocol.Api,
                "BACnet" => PointCommunicationProtocol.BACnet,
                "Modbus" => PointCommunicationProtocol.Modbus,
                "OPC DA" => PointCommunicationProtocol.OpcDa,
                "OPC UA" => PointCommunicationProtocol.OpcUa,
                "KNX" => PointCommunicationProtocol.Knx,
                "LonWorks" => PointCommunicationProtocol.LonWorks,
                "DALI" => PointCommunicationProtocol.Dali,
                "MBus" => PointCommunicationProtocol.MBus,
                "SNMP" => PointCommunicationProtocol.Snmp,
                "MQTT" => PointCommunicationProtocol.Mqtt,
                "IoTHub" => PointCommunicationProtocol.IotHub,
                _ => PointCommunicationProtocol.Unknown,
            };
        }

        private static PointCommunicationProtocol MapProtocol(JObject property)
        {
            if (!property.ContainsKey("protocol"))
            {
                return PointCommunicationProtocol.Unknown;
            }

            switch (property["protocol"].ToString().ToLower())
            {
                case "api":
                    return PointCommunicationProtocol.Api;
                case "bacnet":
                    return PointCommunicationProtocol.BACnet;
                case "modbus":
                    return PointCommunicationProtocol.Modbus;
                case "opc da":
                    return PointCommunicationProtocol.OpcDa;
                case "opc ua":
                    return PointCommunicationProtocol.OpcUa;
                case "knx":
                    return PointCommunicationProtocol.Knx;
                case "lonworks":
                    return PointCommunicationProtocol.LonWorks;
                case "dali":
                    return PointCommunicationProtocol.Dali;
                case "mbus":
                    return PointCommunicationProtocol.MBus;
                case "snmp":
                    return PointCommunicationProtocol.Snmp;
                case "mqtt":
                    return PointCommunicationProtocol.Mqtt;
                case "iothub":
                    return PointCommunicationProtocol.IotHub;
                default:
                    return PointCommunicationProtocol.Unknown;
            };
        }
    }

    [Serializable]
    public enum PointCommunicationProtocol
    {
        Unknown,
        Api,
        BACnet,
        Modbus,
        OpcDa,
        OpcUa,
        Knx,
        LonWorks,
        Dali,
        MBus,
        Snmp,
        Mqtt,
        IotHub
    }
}
