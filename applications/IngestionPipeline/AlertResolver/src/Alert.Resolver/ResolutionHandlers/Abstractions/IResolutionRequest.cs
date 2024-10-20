namespace Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
internal interface IResolutionRequest
{
    string ConnectionString { get; set; }
    string DeviceId { get; set; }
    string? ConnectorName { get; set; }
    string? ConnectorType { get; set; }
    string ConnectorId { get; set; }
    string CustomerId { get; set; }
    DateTime RequestTime { get; }
}
