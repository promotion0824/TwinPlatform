namespace Willow.Alert.Resolver;

internal sealed record DeviceMapping
{
    public IDictionary<string, DeviceInfo> DeviceInfoDict { get; init; } = new Dictionary<string, DeviceInfo>();
}

internal sealed  record DeviceInfo
{
    public string IpList { get; init; } = string.Empty;
}
