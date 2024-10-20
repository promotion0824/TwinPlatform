namespace Willow.IoTService.Deployment.Service.Application.Tests;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Willow.IoTService.Deployment.Service.Application.Deployments;
using Xunit;

public class EdgeConnectorTransformerTests
{
    [Fact]
    public void CanTransform_EdgeConnectorType_ShouldReturnTrue()
    {
        var edgeConnectorEnvMock = Substitute.For<IEdgeConnectorEnvService>();
        var edgeConnectorTransformer = new EdgeConnectorTransformer(NullLogger<EdgeConnectorTransformer>.Instance, edgeConnectorEnvMock);

        edgeConnectorTransformer.CanTransform("DefaultModbusConnector").Should().BeTrue();
    }

    [Fact]
    public void CanTransform_NonEdgeConnectorType_ShouldReturnFalse()
    {
        var edgeConnectorEnvMock = Substitute.For<IEdgeConnectorEnvService>();
        var edgeConnectorTransformer = new EdgeConnectorTransformer(NullLogger<EdgeConnectorTransformer>.Instance, edgeConnectorEnvMock);

        edgeConnectorTransformer.CanTransform("DefaultStreamAnalyticsConnector").Should().BeFalse();
    }

    [Fact]
    public void CanTransform_CASBACnetRPC_ShouldReturnTrue()
    {
        var edgeConnectorEnvMock = Substitute.For<IEdgeConnectorEnvService>();
        var edgeConnectorTransformer = new EdgeConnectorTransformer(NullLogger<EdgeConnectorTransformer>.Instance, edgeConnectorEnvMock);

        edgeConnectorTransformer.CanTransform("CASBACnetRPC").Should().BeTrue();
    }
}
