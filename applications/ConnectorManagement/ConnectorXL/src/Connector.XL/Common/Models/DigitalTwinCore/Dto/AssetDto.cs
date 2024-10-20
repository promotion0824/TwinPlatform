namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

using Connector.XL.Requests.Device;

[Serializable]
internal class AssetDto
{
    public string ModelId { get; set; }

    public string TwinId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public bool HasLiveData { get; set; }

    public List<TagDto> Tags { get; set; } = new List<TagDto>();

    public List<TagDto> PointTags { get; set; } = new List<TagDto>();

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; }

    public Guid? FloorId { get; set; }

    public Dictionary<string, Property> Properties { get; set; }

    public List<double> Geometry { get; set; } = new List<double>();

    public string Identifier { get; set; }

    public string ForgeViewerModelId { get; set; }

    public List<PointDto> Points { get; set; }

    public Guid? ParentId { get; set; }

    internal static EquipmentEntity MapToEntity(Guid siteId, Guid clientId, AssetDto asset)
    {
        string externalEquipmentId = null;
        if (asset.Properties.TryGetValue("externalID", out var externalIdProperty))
        {
            externalEquipmentId = externalIdProperty.Value.ToString();
        }

        return new EquipmentEntity
        {
            Id = asset.Id,
            Category = asset.CategoryName,
            ClientId = clientId,
            SiteId = siteId,
            ExternalEquipmentId = externalEquipmentId,
            Name = asset.Name,
            ParentEquipmentId = asset.ParentId,
            Points = asset.Points?.Select(p => PointDto.MapToEntity(siteId, clientId, p)).ToList() ?? new List<PointEntity>(),
            Tags = asset.Tags?.Select(t => new TagEntity
            {
                ClientId = clientId,
                Description = null,
                Name = t.Name,
                Id = Guid.NewGuid(),
            }).ToList() ?? new List<TagEntity>(), //TODO: Generate consistent Id if reqd,
        };
    }

    public static EquipmentDto MapToEquipmentDto(Guid siteId, Guid clientId, AssetDto asset)
    {
        string externalEquipmentId = null;
        if (asset.Properties != null && asset.Properties.TryGetValue("externalID", out var externalIdProperty))
        {
            externalEquipmentId = externalIdProperty.Value.ToString();
        }

        return new EquipmentDto
        {
            Id = asset.Id,
            Category = asset.CategoryName,
            ClientId = clientId,
            SiteId = siteId,
            ExternalEquipmentId = externalEquipmentId,
            Name = asset.Name,
            ParentEquipmentId = asset.ParentId,
            Points = asset.Points?.Select(p => PointDto.MapToEntity(siteId, clientId, p)).ToList() ?? [],
            Tags = asset.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
            FloorId = asset.FloorId,
            PointTags = asset.PointTags?.Select(t => t.Name).ToList() ?? new List<string>(),
        };
    }
}
