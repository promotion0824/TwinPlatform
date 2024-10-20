namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

/// <summary>
/// Point Communication Protocol.
/// </summary>
[Serializable]
public enum PointCommunicationProtocol
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// API.
    /// </summary>
    Api,

    /// <summary>
    /// BACnet.
    /// </summary>
    BACnet,

    /// <summary>
    /// Modbus.
    /// </summary>
    Modbus,

    /// <summary>
    /// OPC DA.
    /// </summary>
    OpcDa,

    /// <summary>
    /// OPC UA.
    /// </summary>
    OpcUa,

    /// <summary>
    /// KNX.
    /// </summary>
    Knx,

    /// <summary>
    /// LonWorks.
    /// </summary>
    LonWorks,

    /// <summary>
    /// Dali.
    /// </summary>
    Dali,

    /// <summary>
    /// MBus.
    /// </summary>
    MBus,

    /// <summary>
    /// SNMP.
    /// </summary>
    Snmp,

    /// <summary>
    /// MQTT.
    /// </summary>
    Mqtt,

    /// <summary>
    /// Iot Hub.
    /// </summary>
    IotHub,
}
