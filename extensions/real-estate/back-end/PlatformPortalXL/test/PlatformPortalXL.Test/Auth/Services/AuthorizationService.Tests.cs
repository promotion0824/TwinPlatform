using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Model;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Services;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task GetUserPermissions_HasGlobalPermission_EvaluatesPermission()
    {
        const string email = "barry@big-corp.com";
        Claim[] claims =
        [
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, "b0c1204e-f714-41f8-8b88-738208bc189b")
        ];
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
        AuthorizedPermission[] permissions = [new AuthorizedPermission { Name = "CanEditTwins" }];
        var userManagementService = new Mock<IUserManagementService>();

        userManagementService.Setup(s => s.GetPermissionsAsync(email)).ReturnsAsync(permissions);

        var ancestralTwinsProvider = new Mock<IAncestralTwinsSearchService>();
        var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var authService = new AuthorizationService(userManagementService.Object, ancestralTwinsProvider.Object, cache);

        // Act
        var hasPermission = await authService.HasPermission<CanEditTwins>(user);

        hasPermission.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissions_ReturnsUserPermissions()
    {
        const string email = "barry@big-corp.com";
        Claim[] claims =
        [
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, "b0c1204e-f714-41f8-8b88-738208bc189b")
        ];
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
        var permissions = GetGlobalPermissions();
        var userManagementService = new Mock<IUserManagementService>();

        userManagementService.Setup(s => s.GetPermissionsAsync(email)).ReturnsAsync(permissions);

        var ancestralTwinsProvider = new Mock<IAncestralTwinsSearchService>();
        var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var authService = new AuthorizationService(userManagementService.Object, ancestralTwinsProvider.Object, cache);

        // Act
        var userPermissions = await authService.GetUserPermissions(user);

        userPermissions.Should().HaveCount(permissions.Length);
    }

    private static AuthorizedPermission[] GetGlobalPermissions()
    {
        string[] permissions =
        [
            "CanEditTwins",
            "CanInstallConnectors",
            "CanViewInsights",
            "CanViewSearchAndExplore",
            "CanViewTickets",
            "ViewSites"
        ];

        // As no Expression is included these are global permissions applicable to all twins.
        return permissions.Select(p => new AuthorizedPermission { Name = p }).ToArray();
    }
}
