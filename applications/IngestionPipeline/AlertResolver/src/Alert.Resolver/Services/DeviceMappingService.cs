using Microsoft.Extensions.Options;

namespace Willow.Alert.Resolver.Services;

internal interface IDeviceMappingService
{
    IEnumerable<string>? GetDeviceInfo(string deviceId);
}
internal sealed class DeviceMappingService : IDeviceMappingService
{
    private readonly DeviceMapping _deviceMapping;
    private readonly ILogger<DeviceMappingService> _logger;

    public DeviceMappingService(ILogger<DeviceMappingService> logger, IOptions<DeviceMapping> deviceMapping)
    {
        _logger = logger;
        _deviceMapping = deviceMapping.Value;
    }

    public IEnumerable<string>? GetDeviceInfo(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            _logger.LogError("Null DeviceId received");
            return null;
        }

        var deviceDict = _deviceMapping.DeviceInfoDict;
        if (deviceDict.TryGetValue(deviceId.Trim().ToUpperInvariant(), out var deviceInfo))
        {
            return new List<string>(deviceInfo.IpList.Split(',').Select(ip => ip.Trim()));
        }

        _logger.LogError("DeviceId {DeviceId} not found in configured DeviceMappings", deviceId);
        return null;
    }
}
