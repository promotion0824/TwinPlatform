using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Features.Auth;
using System;
using System.Security.Claims;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Permissions;

public class CanEditUsersTests
{
    private readonly Mock<IAuthService> _authService;

    public CanEditUsersTests()
    {
        _authService = new Mock<IAuthService>();
    }

    [Fact]
    public void CanEditUsersCustomerIdEvaluator_HandleAsync_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanEditUsersCustomerIdEvaluator>>();

        var evaluator = new CanEditUsersCustomerIdEvaluator(_authService.Object, logger.Object);

        Claim[] claims =
        [
            new Claim(CustomClaimTypes.CustomerId, customerId.ToString()),
        ];
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
        var context = new AuthorizationHandlerContext([ new CanEditUsers() ], user, customerId);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void CanEditUsersTwinIdEvaluator_HandleAsync_ReturnsSuccess()
    {
        const string twinId = "twin-Id";
        _authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>(), twinId))
            .ReturnsAsync(true);
        var logger = new Mock<ILogger<CanEditUsersTwinIdEvaluator>>();

        var evaluator = new CanEditUsersTwinIdEvaluator(_authService.Object, logger.Object);
        var context = new AuthorizationHandlerContext([ new CanEditUsers() ], null, twinId);

        var task = evaluator.HandleAsync(context);

        Assert.True(task.IsCompletedSuccessfully);
        Assert.True(context.HasSucceeded);
    }
}
