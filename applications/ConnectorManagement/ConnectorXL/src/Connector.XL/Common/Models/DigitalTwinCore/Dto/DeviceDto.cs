namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

using Connector.XL.Requests.Device;

internal class DeviceDto
{
    public string ModelId { get; set; }

    public string TwinId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string RegistrationId { get; set; }

    public string RegistrationKey { get; set; }

    public string ExternalId { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsDetected { get; set; }

    public List<PointDto> Points { get; set; }

    public Dictionary<string, Property> Properties { get; set; }

    public Guid? ConnectorId { get; set; }

    public static DeviceEntity MapToEntity(Guid siteId, Guid clientId, DeviceDto dto)
    {
        return new DeviceEntity
        {
            Id = dto.Id,
            Name = dto.Name,
            SiteId = siteId,
            ConnectorId = dto.ConnectorId.GetValueOrDefault(),
            ClientId = clientId,
            ExternalDeviceId = dto.ExternalId,
            IsDetected = dto.IsDetected.GetValueOrDefault(),
            IsEnabled = dto.IsEnabled.GetValueOrDefault(),
            Metadata = DeviceMetadataDto.MapFromDeviceDto(dto).ToConnectorCoreMetadataString(),
            RegistrationId = dto.RegistrationId,
            RegistrationKey = dto.RegistrationKey,
            Points = dto.Points?.Select(p => PointDto.MapToEntity(siteId, clientId, p)).ToList() ?? new List<PointEntity>(),
        };
    }
}
