using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanEditTwinsTests
{
    private readonly Mock<IAuthService> _authService;

    public CanEditTwinsTests()
    {
        _authService = new Mock<IAuthService>();
    }

    [Fact]
    public void CanEditTwinsEvaluator_HandleAsync_ReturnsSuccess()
    {
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanEditTwinsEvaluator>>();

        var evaluator = new CanEditTwinsEvaluator(_authService.Object, logger.Object);
        var context = new AuthorizationHandlerContext([ new CanEditTwins() ], null, null);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void CanEditTwinsTwinIdEvaluator_HandleAsync_ReturnsSuccess()
    {
        const string twinId = "twin-Id";
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>(), twinId))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanEditTwinsTwinIdEvaluator>>();

        var evaluator = new CanEditTwinsTwinIdEvaluator(_authService.Object, logger.Object);
        var context = new AuthorizationHandlerContext([ new CanEditTwins() ], null, twinId);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
