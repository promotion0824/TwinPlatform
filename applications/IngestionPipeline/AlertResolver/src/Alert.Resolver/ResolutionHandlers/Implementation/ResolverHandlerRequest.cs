using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;

internal class ResolutionRequest : IResolutionRequest
{

    public string ConnectionString { get; set; }
    private readonly DateTime _utcNow;
    public ResolutionRequest(string customerId,
                             string connectorId,
                             string connectionString,
                             string deviceId,
                             string iotHubName,
                             string connectorName,
                             string connectorType,
                             List<string>? ipAddresses = null)

    {
        CustomerId = customerId;
        ConnectionString = connectionString;
        DeviceId = deviceId;
        IoTHubName = iotHubName;
        ConnectorName = connectorName;
        ConnectorType = connectorType;
        ConnectorId = connectorId;
        if (ipAddresses is not null)
        {
            IpAddresses = ipAddresses;
        }
        _utcNow = DateTime.UtcNow;
    }

    public string DeviceId { get; set; }
    public string IoTHubName { get; set; }
    public string? ConnectorName { get; set; }
    public string? ConnectorType { get; set; }
    public string ConnectorId { get; set; }
    public string CustomerId { get; set; }
    public List<string> IpAddresses { get; } = new();
    public DateTime RequestTime { get { return _utcNow; } }
}
