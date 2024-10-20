namespace Willow.IoTService.Deployment.Service.Application.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Willow.IoTService.Deployment.Service.Application.Deployments;
using Xunit;

public class DeploymentServiceTests
{
    [Fact]
    public async Task ApplyConfig_ProcessThrowsException_ShouldCleanThenThrow()
    {
        var registryManager = Substitute.For<RegistryManager>();
        var config = new Configuration("configurationId");
        var request = new DeploymentConfiguration(config, "deviceId");
        registryManager.AddConfigurationAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
                       .Returns(config);
        registryManager.ApplyConfigurationContentOnDeviceAsync(
                                                               Arg.Any<string>(),
                                                               Arg.Any<ConfigurationContent>(),
                                                               Arg.Any<CancellationToken>())
                       .Throws<Exception>();
        await registryManager.RemoveConfigurationAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>());

        var service = new DeploymentService(registryManager, NullLogger<DeploymentService>.Instance);
        Func<Task> func = async () => await service.DeployAsync(request);
        await func.Should()
                  .ThrowAsync<Exception>();
        await registryManager.Received().RemoveConfigurationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyConfig_InputValid_ShouldAddedThenApplied()
    {
        var registryManager = Substitute.For<RegistryManager>();
        var config = new Configuration("configurationId");
        var request = new DeploymentConfiguration(config, "deviceId");

        registryManager.AddConfigurationAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>())
                       .Returns(config);
        var service = new DeploymentService(registryManager, NullLogger<DeploymentService>.Instance);
        await service.DeployAsync(request);
        await registryManager.Received().AddConfigurationAsync(Arg.Any<Configuration>(), Arg.Any<CancellationToken>());
        await registryManager.Received().ApplyConfigurationContentOnDeviceAsync(
                                                                                Arg.Any<string>(),
                                                                                Arg.Any<ConfigurationContent>(),
                                                                                Arg.Any<CancellationToken>());
    }
}
