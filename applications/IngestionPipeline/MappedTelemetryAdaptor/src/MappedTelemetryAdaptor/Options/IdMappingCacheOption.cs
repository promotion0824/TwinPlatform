namespace Willow.MappedTelemetryAdaptor.Options;

internal sealed record IdMappingCacheOption
{
    public const string Section = "IdMappingCache";

    public int RefetchCacheSeconds { get; init; } = 7200;
}
