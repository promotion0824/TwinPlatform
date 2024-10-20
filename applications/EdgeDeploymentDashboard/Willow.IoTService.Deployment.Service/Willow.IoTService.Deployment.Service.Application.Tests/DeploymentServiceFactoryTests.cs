using NSubstitute;

namespace Willow.IoTService.Deployment.Service.Application.Tests;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Willow.IoTService.Deployment.Service.Application.Deployments;
using Xunit;

public class DeploymentServiceFactoryTests
{
    [Fact]
    public void Create_InputValid_ShouldCreateService()
    {
        var tokenCredentialMock = Substitute.For<Azure.Core.TokenCredential>();
        var factory = new DeploymentServiceFactory(NullLoggerFactory.Instance, tokenCredentialMock);
        var iotHubName = "hostname";
        var service = factory.Create(iotHubName);
        service.Hostname.Should()
               .Be($"{iotHubName}.azure-devices.net");
    }
}
