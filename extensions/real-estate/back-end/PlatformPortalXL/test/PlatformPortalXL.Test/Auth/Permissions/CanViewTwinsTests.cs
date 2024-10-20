using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanViewTwinsTests
{
    private readonly Mock<IAuthService> _authService;

    public CanViewTwinsTests()
    {
        _authService = new Mock<IAuthService>();
    }

    [Fact]
    public void CanViewTwinsEvaluator_HandleAsync_ReturnsSuccess()
    {
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanViewTwinsEvaluator>>();

        var evaluator = new CanViewTwinsEvaluator(_authService.Object, logger.Object);
        var context = new AuthorizationHandlerContext([ new CanViewTwins() ], null, null);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void CanViewTwinsTwinIdEvaluator_HandleAsync_ReturnsSuccess()
    {
        const string twinId = "twin-Id";
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>(), twinId))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanViewTwinsTwinIdEvaluator>>();

        var evaluator = new CanViewTwinsTwinIdEvaluator(_authService.Object, logger.Object);
        var context = new AuthorizationHandlerContext([ new CanViewTwins() ], null, twinId);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
