using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using PlatformPortalXL.Auth;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using Xunit;

namespace PlatformPortalXL.Test.Auth.Services;

public class TwinTreeAuthEvaluatorTests
{
    [Fact]
    public async Task PrunedTreeIsValid_WhenLeafSiteAuthorized()
    {
        var currentUser = new Mock<ICurrentUser>();
        var authService = new Mock<IAuthService>();
        const string authorizedScope = "WIL-57CM";
        authService
            .Setup(x => x.HasPermission<WillowAuthorizationRequirement>(It.IsAny<ClaimsPrincipal>(), authorizedScope))
            .ReturnsAsync(true);

        var sut = new TwinTreeAuthEvaluator(authService.Object, currentUser.Object);

        var twinTreeContents = await File.ReadAllTextAsync("twin-tree-ddk.json");
        var twinTree = JsonConvert.DeserializeObject<List<NestedTwinDto>>(twinTreeContents);

        // Act
        var prunedTree = await sut.GetPrunedTree(twinTree);

        var flattened = GetFlattenedNodes(prunedTree).ToList();
        Assert.Contains(flattened, t => t.Id == "WIL-EU-Region");
        Assert.Contains(flattened, t => t.Id == "WIL-CanaryWharf");
        Assert.Contains(flattened, t => t.Id == "WIL-57CM");
    }

    private static IEnumerable<TwinDto> GetFlattenedNodes(IEnumerable<NestedTwinDto> twinTree)
    {
        foreach (var node in twinTree)
        {
            yield return node.Twin;
            foreach(var child in GetFlattenedNodes(node.Children))
            {
                yield return child;
            }
        }
    }
}
