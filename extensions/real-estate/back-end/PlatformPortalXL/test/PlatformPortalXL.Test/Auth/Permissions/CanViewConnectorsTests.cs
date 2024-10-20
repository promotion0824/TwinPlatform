using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanViewConnectorsTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly Mock<ILogger<CanViewConnectorsEvaluator>> _logger;

    public CanViewConnectorsTests()
    {
        _authService = new Mock<IAuthService>();
        _logger = new Mock<ILogger<CanViewConnectorsEvaluator>>();
    }

    [Fact]
    public void CanViewConnectorsEvaluator_HandleAsync_ReturnsSuccess()
    {
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);

        var evaluator = new CanViewConnectorsEvaluator(_authService.Object, _logger.Object);
        var context = new AuthorizationHandlerContext([ new CanViewConnectors() ], null, null);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
