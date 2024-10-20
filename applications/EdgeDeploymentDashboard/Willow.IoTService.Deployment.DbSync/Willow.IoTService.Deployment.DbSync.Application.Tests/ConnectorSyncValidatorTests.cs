namespace Willow.IoTService.Deployment.DbSync.Application.Tests;

using ConnectorCore.Contracts;
using Willow.IoTService.Deployment.DbSync.Application.Tests.Infrastructure;
using Xunit;

public class ConnectorSyncValidatorTests
{
    private readonly ConnectorMessageValidator validator = new();

    [Fact]
    public async Task ConnectorSyncValidator_Validate_Fails_For_Empty_ConnectorId()
    {
        var message = new ConnectorSyncMessageMock
        {
            Archived = false,
            ConnectorId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            Enabled = true,
            Status = ConnectorUpdateStatus.Enable,
            Timestamp = DateTime.UtcNow,
        };
        var result = await validator.ValidateAsync(message);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ConnectorSyncValidator_Validate_Fails_For_Empty_CustomerId()
    {
        var message = new ConnectorSyncMessageMock
        {
            Archived = false,
            ConnectorId = Guid.NewGuid(),
            CustomerId = Guid.Empty,
            Enabled = true,
            Status = ConnectorUpdateStatus.Enable,
            Timestamp = DateTime.UtcNow,
        };
        var result = await validator.ValidateAsync(message);

        Assert.False(result.IsValid);
    }
}
