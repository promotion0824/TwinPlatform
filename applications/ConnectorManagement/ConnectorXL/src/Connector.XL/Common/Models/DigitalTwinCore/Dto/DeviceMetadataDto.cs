namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

using System.Text.Json;
using System.Text.Json.Serialization;
using Connector.XL.Requests.Device;

[Serializable]
internal class DeviceMetadataDto
{
    //BACNet
    public string Address { get; set; }

    public string MacAddress { get; set; }

    //Modbus
    public string IpAddress { get; set; }

    //PointGrab
    public string AttachmentState { get; set; }

    public string ConnectionStatus { get; set; }

    public string FwVersion { get; set; }

    public string GeoPosition { get; set; }

    public string Height { get; set; }

    public string MetricPosition { get; set; }

    public string Rotation { get; set; }

    public string SerialNo { get; set; }

    internal static DeviceMetadataDto MapFromEntity(DeviceEntity device)
    {
        if (string.IsNullOrWhiteSpace(device.Metadata))
        {
            return new DeviceMetadataDto();
        }

        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(device.Metadata);

        return new DeviceMetadataDto
        {
            Address = GetStringProperty(metadata, "Address"),
            MacAddress = GetStringProperty(metadata, "MacAddress"),
            IpAddress = GetStringProperty(metadata, "IpAddress"),
            AttachmentState = GetStringProperty(metadata, "AttachmentState"),
            ConnectionStatus = GetStringProperty(metadata, "ConnectionStatus"),
            FwVersion = GetStringProperty(metadata, "FwVersion"),
            GeoPosition = GetStringProperty(metadata, "GeoPosition"),
            Height = GetStringProperty(metadata, "Height"),
            MetricPosition = GetStringProperty(metadata, "MetricPosition"),
            Rotation = GetStringProperty(metadata, "Rotation"),
            SerialNo = GetStringProperty(metadata, "SerialNo"),
        };
    }

    internal static DeviceMetadataDto MapFromDeviceDto(DeviceDto dto)
    {
        if (dto?.Properties == null)
        {
            return new DeviceMetadataDto();
        }

        var output = new DeviceMetadataDto();

        foreach (var property in dto.Properties)
        {
            switch (property.Key)
            {
                case var prop when string.Equals(prop, "Address", StringComparison.OrdinalIgnoreCase):
                    output.Address = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "MACAddress", StringComparison.OrdinalIgnoreCase):
                    output.MacAddress = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "IPAddress", StringComparison.OrdinalIgnoreCase):
                    output.IpAddress = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "serialNumber", StringComparison.OrdinalIgnoreCase):
                    output.SerialNo = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "AttachmentState", StringComparison.OrdinalIgnoreCase):
                    output.AttachmentState = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "ConnectionStatus", StringComparison.OrdinalIgnoreCase):
                    output.ConnectionStatus = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "FwVersion", StringComparison.OrdinalIgnoreCase):
                    output.FwVersion = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "GeoPosition", StringComparison.OrdinalIgnoreCase):
                    output.GeoPosition = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "Height", StringComparison.OrdinalIgnoreCase):
                    output.Height = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "MetricPosition", StringComparison.OrdinalIgnoreCase):
                    output.MetricPosition = property.Value.Value?.ToString();
                    break;
                case var prop when string.Equals(prop, "Rotation", StringComparison.OrdinalIgnoreCase):
                    output.Rotation = property.Value.Value?.ToString();
                    break;
            }
        }

        return output;
    }

    private static string GetStringProperty(Dictionary<string, object> metadata, string propertyName)
    {
        return metadata.ContainsKey(propertyName) ? metadata[propertyName].ToString() : null;
    }

    internal string ToConnectorCoreMetadataString()
    {
        return JsonSerializer.Serialize(this,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            });
    }
}
