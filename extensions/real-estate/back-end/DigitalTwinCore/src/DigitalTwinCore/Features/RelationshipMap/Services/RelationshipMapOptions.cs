namespace DigitalTwinCore.Features.RelationshipMap.Services;

public class RelationshipMapOptions
{
    public static readonly string Section = "RelationshipMap";

    public int MemoryCacheInMinutes { get; set; } = 5;
    public int BlobCacheInHours { get; set; } = 24;
}