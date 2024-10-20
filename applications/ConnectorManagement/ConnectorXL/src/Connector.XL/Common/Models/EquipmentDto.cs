namespace Connector.XL.Common.Models;

using Connector.XL.Requests.Device;

internal class EquipmentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid ClientId { get; set; }

    public Guid SiteId { get; set; }

    public Guid? FloorId { get; set; }

    public string ExternalEquipmentId { get; set; }

    public string Category { get; set; }

    public Guid? ParentEquipmentId { get; set; }

    public IList<PointEntity> Points { get; set; } = new List<PointEntity>();

    public IList<CategoryEntity> Categories { get; set; } = new List<CategoryEntity>();

    public IList<string> Tags { get; set; } = new List<string>();

    public IList<string> PointTags { get; set; } = new List<string>();
}
