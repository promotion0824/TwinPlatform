using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanViewSearchAndExploreTests
{
    private readonly Mock<IAuthService> _authService;
    private readonly Mock<ILogger<CanViewSearchAndExploreEvaluator>> _logger;

    public CanViewSearchAndExploreTests()
    {
        _authService = new Mock<IAuthService>();
        _logger = new Mock<ILogger<CanViewSearchAndExploreEvaluator>>();
    }

    [Fact]
    public void CanViewSearchAndExploreEvaluator_HandleAsync_ReturnsSuccess()
    {
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);

        var evaluator = new CanViewSearchAndExploreEvaluator(_authService.Object, _logger.Object);
        var context = new AuthorizationHandlerContext([ new CanViewSearchAndExplore() ], null, null);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
