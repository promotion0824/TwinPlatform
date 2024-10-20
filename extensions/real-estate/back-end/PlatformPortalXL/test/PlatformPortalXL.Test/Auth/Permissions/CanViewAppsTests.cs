using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanViewAppsTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly Mock<ILogger<CanViewAppsEvaluator>> _logger;

    public CanViewAppsTests()
    {
        _authService = new Mock<IAuthService>();
        _logger = new Mock<ILogger<CanViewAppsEvaluator>>();
    }

    [Fact]
    public void CanViewAppsEvaluator_HandleAsync_ReturnsSuccess()
    {
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);

        var evaluator = new CanViewAppsEvaluator(_authService.Object, _logger.Object);
        var context = new AuthorizationHandlerContext([ new CanViewApps() ], null, null);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
