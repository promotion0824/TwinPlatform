namespace Connector.XL.Requests.Device;

using System.ComponentModel.DataAnnotations.Schema;

internal class DeviceEntity
{
    /// <summary>
    /// Gets or sets device's ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets device's name.
    /// </summary>
    public string Name { get; set; }

    public Guid ClientId { get; set; }

    public Guid SiteId { get; set; }

    public string ExternalDeviceId { get; set; }

    public string RegistrationId { get; set; }

    public string RegistrationKey { get; set; }

    public string Metadata { get; set; }

    public bool IsDetected { get; set; }

    public Guid ConnectorId { get; set; }

    [NotMapped]
    public IList<PointEntity> Points { get; set; }

    public bool IsEnabled { get; set; }
}

internal class EquipmentEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid ClientId { get; set; }

    public Guid SiteId { get; set; }

    public string ExternalEquipmentId { get; set; }

    public string Category { get; set; }

    public Guid? ParentEquipmentId { get; set; }

    public IList<PointEntity> Points { get; set; }

    public IList<TagEntity> Tags { get; set; }
}

internal class PointEntity
{
    public Guid Id { get; set; }

    public Guid EntityId { get; set; }

    public string Name { get; set; }

    public Guid ClientId { get; set; }

    public Guid SiteId { get; set; }

    public string Unit { get; set; }

    public int Type { get; set; }

    public string ExternalPointId { get; set; }

    public string Category { get; set; }

    public string Metadata { get; set; }

    public bool IsDetected { get; set; }

    public Guid DeviceId { get; set; }

    [NotMapped]
    public DeviceEntity Device { get; set; }

    [NotMapped]
    public IList<TagEntity> Tags { get; set; }

    [NotMapped]
    public IList<EquipmentEntity> Equipment { get; set; }

    public bool IsEnabled { get; set; }
}

internal class TagEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public Guid? ClientId { get; set; }
}
