namespace ConnectorCore.Services;

using System;
using System.Threading.Tasks;
using ConnectorCore.Infrastructure.Exceptions;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;

internal class IotRegistrationService : IIotRegistrationService
{
    private readonly IConfiguration configuration;

    public IotRegistrationService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<string> RegisterDevice(string deviceId, Guid siteId, string connectorId, string connectionString)
    {
        var registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        var device = await registryManager.GetDeviceAsync(deviceId);

        if (device == null)
        {
            device = new Device(deviceId);
            var twin = new Twin(deviceId);
            var newDeviceTags = $"{{ siteId: \"{siteId.ToString()}\", connectorId: \"{connectorId}\" }}";
            twin.Tags = new TwinCollection(newDeviceTags);
            await registryManager.AddDeviceWithTwinAsync(device, twin);
            device = await registryManager.GetDeviceAsync(deviceId);
        }

        return device.Authentication.SymmetricKey.PrimaryKey;
    }

    public async Task<string> GetConnectionString(Guid clientId)
    {
        var configurationKey = $"{clientId.ToString("D").ToUpperInvariant()}:IotHubRegWriteConnString";
        var connectionString = configuration.GetValue<string>(configurationKey);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return await Task.FromResult(connectionString);
        }

        var altConfigurationKey = $"{clientId.ToString("D").ToLowerInvariant()}:IotHubRegWriteConnString";
        connectionString = configuration.GetValue<string>(altConfigurationKey);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return await Task.FromResult(connectionString);
        }

        connectionString = configuration.GetValue<string>("IoTHubRegWriteConnString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new NotFoundException($"Customer {clientId} does not exist or its IoTHub reference has not been configured yet.");
        }

        return await Task.FromResult(connectionString);
    }

    public async Task DeleteDevice(string deviceId, string connectionString)
    {
        var registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        var device = await registryManager.GetDeviceAsync(deviceId);

        if (device != null)
        {
            await registryManager.RemoveDeviceAsync(device);
        }
    }
}
