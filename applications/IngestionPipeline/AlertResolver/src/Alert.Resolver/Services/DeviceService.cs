using Microsoft.Azure.Devices;
using Willow.Alert.Resolver.Enumerations;
using Willow.Alert.Resolver.Helpers;

namespace Willow.Alert.Resolver.Services;

internal interface IDeviceService
{
    Task<bool> RestartDeviceModuleAsync(string ioTHubConnectionString,
                                        string deviceId,
                                        string connectorType,
                                        string connectorName);

    Task<bool> PingDeviceModuleAsync(string ioTHubConnectionString,
                                     string deviceId);
}

internal sealed class DeviceService : IDeviceService
{
    private const int SuccessStatusCode = 200;
    private readonly ILogger<DeviceService> _logger;
    private readonly IModuleHelper _moduleHelper;

    public DeviceService(ILogger<DeviceService> logger,
                         IModuleHelper moduleHelper)

    {
        _logger = logger;
        _moduleHelper = moduleHelper;
    }

    public async Task<bool> RestartDeviceModuleAsync(string ioTHubConnectionString,
                                                     string deviceId,
                                                     string connectorType,
                                                     string connectorName)
    {
        using var serviceClient = ServiceClient.CreateFromConnectionString(ioTHubConnectionString);
        var methodInvocation = GetMethodInvocation(DirectMethods.RestartModule);
        _logger.LogDebug("Getting dependent modules");
        var modules = _moduleHelper.GetDependentModules(connectorType, connectorName);
        var invokeResults = new List<int>();
        foreach (var payload in _moduleHelper.GetRestartPayloads(modules))
        {
            methodInvocation.SetPayloadJson(payload);
            var result = await InvokeEdgeAgentMethodAsync(deviceId, serviceClient, methodInvocation);
            invokeResults.Add(result.Status);
        }

        return invokeResults.All(x => x is SuccessStatusCode);
    }

    public async Task<bool> PingDeviceModuleAsync(string ioTHubConnectionString,
                                                  string deviceId)
    {
        using var serviceClient = ServiceClient.CreateFromConnectionString(ioTHubConnectionString);
        var methodInvocation = GetMethodInvocation(DirectMethods.Ping);
        var response = await InvokeEdgeAgentMethodAsync(deviceId, serviceClient, methodInvocation);
        return response.Status == SuccessStatusCode;
    }

    private static CloudToDeviceMethod GetMethodInvocation(DirectMethods directMethods)
    {
        var methodInvocation = new CloudToDeviceMethod(directMethods.ToString())
        {
            ResponseTimeout = TimeSpan.FromSeconds(30)
        };
        return methodInvocation;
    }

    private async Task<CloudToDeviceMethodResult> InvokeEdgeAgentMethodAsync(string deviceId, ServiceClient serviceClient, CloudToDeviceMethod methodInvocation)
    {
        _logger.LogInformation("Invoking direct method for device: {DeviceId} and method: {MethodName}", deviceId, methodInvocation.MethodName);
        var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, "$edgeAgent", methodInvocation);
        _logger.LogInformation("Response status: {Status}, payload:\n\t{Payload}", response.Status, response.GetPayloadAsJson());
        return response;
    }
}
