namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Connector.XL.Requests.Device;

[Serializable]
internal class PointDto
{
    public string ModelId { get; set; }

    public string TwinId { get; set; }

    public Guid Id { get; set; }

    public Guid TrendId { get; set; }

    public string ExternalId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public List<TagDto> Tags { get; set; }

    public PointType Type { get; set; }

    public PointValue CurrentValue { get; set; }

    public decimal? DisplayPriority { get; set; }

    public List<PointAssetDto> Assets { get; set; }

    public Dictionary<string, Property> Properties { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsDetected { get; set; }

    public Guid? DeviceId { get; set; }

    public string TrendInterval { get; set; }

    public string CategoryName { get; set; }

    public PointMetadataDto Metadata { get; set; }

    public static PointEntity MapToEntity(Guid siteId, Guid clientId, PointDto dto)
    {
        return new PointEntity
        {
            Id = dto.Id,
            EntityId = dto.TrendId,
            Category = dto.CategoryName,
            ClientId = clientId,
            DeviceId = dto.DeviceId.GetValueOrDefault(),
            Equipment = dto.Assets?.Select(a => MapEquipment(siteId, clientId, a)).ToList() ?? new List<EquipmentEntity>(),
            Device = MapDevice(siteId, clientId, dto.DeviceId.GetValueOrDefault(), dto),
            ExternalPointId = dto.ExternalId,
            IsDetected = dto.IsDetected.GetValueOrDefault(),
            IsEnabled = dto.IsEnabled.GetValueOrDefault(),
            Metadata = MapScaleFactor(dto).ToConnectorCoreMetadataString(dto.TrendInterval),
            Name = dto.Name,
            SiteId = siteId,
            Type = (int)dto.Type, //TODO: VERIFY THAT THESE ARE COMPATIBLE
            Unit = dto.CurrentValue?.Unit,
            Tags = dto.Tags?.Select(t => new TagEntity
            {
                ClientId = clientId,
                Description = null,
                Name = t.Name,
                Id = Guid.NewGuid(),
            }).ToList() ?? new List<TagEntity>(), //TODO: Generate consistent Id if reqd
        };
    }

    private static DeviceEntity MapDevice(Guid siteId, Guid clientId, Guid deviceId, PointDto dto)
    {
        return new DeviceEntity
        {
            Id = deviceId,
            Metadata = dto.Metadata?.DeviceMetadata?.ToConnectorCoreMetadataString(),
            SiteId = siteId,
            ClientId = clientId,
        };
    }

    private static EquipmentEntity MapEquipment(Guid siteId, Guid clientId, PointAssetDto pointAssetDto)
    {
        return new EquipmentEntity
        {
            Id = pointAssetDto.Id,
            Name = pointAssetDto.Name,
            SiteId = siteId,
            ClientId = clientId,
        };
    }

    private static PointMetadataDto MapScaleFactor(PointDto dto)
    {
        if (dto == null)
        {
            return null;
        }

        var scale = 1.0f;
        var isCalculated = dto.Properties.TryGetValue("scaleFactor", out var value) && float.TryParse(value.Value?.ToString(), out scale);

        switch (dto.Metadata.Communication?.Protocol)
        {
            case PointCommunicationProtocol.BACnet:
                if (dto.Metadata.Communication.BacNet != null)
                {
                    dto.Metadata.Communication.BacNet.IsCalculated = isCalculated;
                    dto.Metadata.Communication.BacNet.Scale = scale;
                }

                break;
            case PointCommunicationProtocol.Modbus:
                // backwards compatability for existing Modbus controllers using "Scale" capability
                if (dto.Metadata.Communication.Modbus != null)
                {
                    if (isCalculated)
                    {
                        dto.Metadata.Communication.Modbus.Scale = scale;
                    }

                    dto.Metadata.Communication.Modbus.IsCalculated = true;
                }

                break;
            case PointCommunicationProtocol.OpcUa:
                if (dto.Metadata.Communication.OpcUa != null)
                {
                    dto.Metadata.Communication.OpcUa.IsCalculated = isCalculated;
                    dto.Metadata.Communication.OpcUa.Scale = scale;
                }

                break;
            case PointCommunicationProtocol.OpcDa:
                if (dto.Metadata.Communication.OpcDa != null)
                {
                    dto.Metadata.Communication.OpcDa.IsCalculated = isCalculated;
                    dto.Metadata.Communication.OpcDa.Scale = scale;
                }

                break;
        }

        return dto.Metadata;
    }
}

internal class PointValue
{
    public string Unit { get; set; }

    public object Value { get; set; }
}

[Serializable]
internal class PointAssetDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}

[Serializable]
internal class PointMetadataDto
{
    public PointCommunicationDto Communication { get; set; }

    public DeviceMetadataDto DeviceMetadata { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval)
    {
        return Communication?.Protocol switch
        {
            PointCommunicationProtocol.Api => Communication.Api?.ToConnectorCoreMetadataString(trendInterval),
            PointCommunicationProtocol.BACnet => Communication.BacNet?.ToConnectorCoreMetadataString(trendInterval),
            PointCommunicationProtocol.IotHub => Communication.IotHub?.ToConnectorCoreMetadataString(trendInterval),
            PointCommunicationProtocol.Modbus => Communication.Modbus?.ToConnectorCoreMetadataString(trendInterval),
            PointCommunicationProtocol.OpcDa => Communication.OpcDa?.ToConnectorCoreMetadataString(trendInterval),
            PointCommunicationProtocol.OpcUa => Communication.OpcUa?.ToConnectorCoreMetadataString(trendInterval),
            _ => "{}",
        };
    }
}

[Serializable]
internal class PointCommunicationDto
{
    public PointCommunicationProtocol Protocol { get; set; }

    public ApiPointProperties Api { get; set; }

    public BacNetPointProperties BacNet { get; set; }

    public ModbusPointProperties Modbus { get; set; }

    public OpcDaPointProperties OpcDa { get; set; }

    public OpcUaPointProperties OpcUa { get; set; }

    public IotHubPointProperties IotHub { get; set; }
}

[Serializable]
internal class ApiPointProperties
{
    public string ExternalId { get; set; }

    internal static ApiPointProperties MapFromProperty(IDictionary<string, object> property)
    {
        return new ApiPointProperties
        {
            ExternalId = property.ContainsKey("externalId") ? property["externalId"] as string : null,
        };
    }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        var output = new Dictionary<string, string>();
        output["ExternalId"] = ExternalId;
        output["PollingInterval"] = trendInterval;

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}

[Serializable]
internal class BacNetPointProperties
{
    public enum BacNetPointObjectType
    {
        /// <summary>
        /// Analog Input.
        /// </summary>
        AnalogInput,

        /// <summary>
        /// Analog Output.
        /// </summary>
        AnalogOutput,

        /// <summary>
        /// Analog Value.
        /// </summary>
        AnalogValue,

        /// <summary>
        /// Binary Input.
        /// </summary>
        BinaryInput,

        /// <summary>
        /// Binary Output.
        /// </summary>
        BinaryOutput,

        /// <summary>
        /// Binary Value.
        /// </summary>
        BinaryValue,

        /// <summary>
        /// Device.
        /// </summary>
        Device,

        /// <summary>
        /// Multi-state Input.
        /// </summary>
        MultistateInput,

        /// <summary>
        /// Multi-state Output.
        /// </summary>
        MultistateOutput,

        /// <summary>
        /// Multi-state Value.
        /// </summary>
        MultistateValue,
    }

    public int DeviceId { get; set; }

    public int ObjectId { get; set; }

    public float Scale { get; set; }

    public bool IsCalculated { get; set; }

    public BacNetPointObjectType ObjectType { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        var output = new Dictionary<string, string>
        {
            ["DeviceId"] = DeviceId.ToString(),
            ["ObjectId"] = ObjectId.ToString(),
            ["Scale"] = Scale.ToString(CultureInfo.InvariantCulture),
            ["IsCalculated"] = IsCalculated.ToString(CultureInfo.InvariantCulture),
            ["ObjectType"] = ObjectType switch
            {
                BacNetPointObjectType.AnalogInput => "AI",
                BacNetPointObjectType.AnalogOutput => "AO",
                BacNetPointObjectType.AnalogValue => "AV",
                BacNetPointObjectType.BinaryInput => "BI",
                BacNetPointObjectType.BinaryOutput => "BO",
                BacNetPointObjectType.BinaryValue => "BV",
                BacNetPointObjectType.Device => "DEV",
                BacNetPointObjectType.MultistateInput => "MSI",
                BacNetPointObjectType.MultistateOutput => "MSO",
                BacNetPointObjectType.MultistateValue => "MSV",
                _ => null,
            },
            ["PollingInterval"] = trendInterval,
        };

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}

[Serializable]
internal class IotHubPointProperties
{
    public string ExternalId { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        var output = new Dictionary<string, string>
        {
            ["ExternalId"] = ExternalId,
            ["PollingInterval"] = trendInterval,
        };

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}

[Serializable]
internal class ModbusPointProperties
{
    public enum ModbusDataType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Bit.
        /// </summary>
        Bit,

        /// <summary>
        /// 8-bit unsigned integer.
        /// </summary>
        Uint8,

        /// <summary>
        /// 8-bit signed integer.
        /// </summary>
        Int8,

        /// <summary>
        /// Unsigned 16-bit integer.
        /// </summary>
        Uint16,

        /// <summary>
        /// Signed 16-bit integer.
        /// </summary>
        Int16,

        /// <summary>
        /// Unsigned 32-bit integer.
        /// </summary>
        Uint32,

        /// <summary>
        /// Signed 32-bit integer.
        /// </summary>
        Int32,

        /// <summary>
        /// 32-bit floating point.
        /// </summary>
        Float,

        /// <summary>
        /// Unsigned 64-bit integer.
        /// </summary>
        Uint64,

        /// <summary>
        /// Signed 64-bit integer.
        /// </summary>
        Int64,

        /// <summary>
        /// 64-bit floating point.
        /// </summary>
        Double,
    }

    public enum ModbusEndianType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Big endian.
        /// </summary>
        EndianBig,

        /// <summary>
        /// Little endian.
        /// </summary>
        EndianLittle,
    }

    public enum ModbusRegisterType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Coil.
        /// </summary>
        Coil,

        /// <summary>
        /// Discrete input.
        /// </summary>
        DiscreteInput,

        /// <summary>
        /// Input register.
        /// </summary>
        InputRegister,

        /// <summary>
        /// Holding register.
        /// </summary>
        HoldingRegister,
    }

    public ModbusDataType DataType { get; set; }

    public int RegisterAddress { get; set; }

    public bool Swap { get; set; }

    public int SlaveId { get; set; }

    public ModbusEndianType Endian { get; set; }

    public float Scale { get; set; }

    public bool IsCalculated { get; set; }

    public ModbusRegisterType RegisterType { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        //TODO: Need example data
        var output = new Dictionary<string, string>
        {
            ["DataType"] = ((int)DataType).ToString(),
            ["RegisterAddress"] = RegisterAddress.ToString(),
            ["Swap"] = Swap.ToString(),
            ["SlaveId"] = SlaveId.ToString(),
            ["Endian"] = Endian.ToString(),
            ["Scale"] = Scale.ToString(CultureInfo.InvariantCulture),
            ["IsCalculated"] = IsCalculated.ToString(CultureInfo.InvariantCulture),
            ["RegisterType"] = RegisterType.ToString(),
            ["PollingInterval"] = trendInterval,
        };

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}

[Serializable]
internal class OpcDaPointProperties
{
    public string NodeId { get; set; }

    public float Scale { get; set; }

    public bool IsCalculated { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        var output = new Dictionary<string, string>
        {
            ["NodeId"] = NodeId,
            ["PollingInterval"] = trendInterval,
            ["Scale"] = Scale.ToString(CultureInfo.InvariantCulture),
            ["IsCalculated"] = IsCalculated.ToString(CultureInfo.InvariantCulture),
        };

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}

[Serializable]
internal class OpcUaPointProperties
{
    public string NodeId { get; set; }

    public float Scale { get; set; }

    public bool IsCalculated { get; set; }

    internal string ToConnectorCoreMetadataString(string trendInterval = "900")
    {
        var output = new Dictionary<string, string>
        {
            ["NodeId"] = NodeId,
            ["PollingInterval"] = trendInterval,
            ["Scale"] = Scale.ToString(CultureInfo.InvariantCulture),
            ["IsCalculated"] = IsCalculated.ToString(CultureInfo.InvariantCulture),
        };

        return JsonSerializer.Serialize(output,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}
