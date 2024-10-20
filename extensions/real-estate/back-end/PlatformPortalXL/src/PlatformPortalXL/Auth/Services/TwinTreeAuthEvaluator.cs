using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.Auth.Services;

public interface ITwinTreeAuthEvaluator
{
    /// <summary>
    /// Takes a twin tree and returns a filtered version that contains only
    /// the nodes the user should be able to see in the scope selector.
    /// </summary>
    /// <param name="twinTree">The tree to filter.</param>
    Task<IEnumerable<NestedTwinDto>> GetPrunedTree(List<NestedTwinDto> twinTree);
}

public class TwinTreeAuthEvaluator : ITwinTreeAuthEvaluator
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public TwinTreeAuthEvaluator(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<NestedTwinDto>> GetPrunedTree(List<NestedTwinDto> twinTree)
    {
        var prunedNode = await GetPrunedNode(new NestedTwinDto { Children = twinTree });
        return prunedNode.Node.Children.ToList();
    }

    /// <summary>
    /// To facilitate adding ancestors of nodes that match, we return
    /// the transformed node plus a boolean indicating whether we matched,
    /// so we can use the boolean to propagate up the tree.
    /// </summary>
    private async Task<(NestedTwinDto Node, bool Keep)> GetPrunedNode(NestedTwinDto node)
    {
        var prunedChildren = new List<(NestedTwinDto Node, bool Keep)>();

        foreach (var child in node.Children)
        {
            // Use a loop rather than `Select` so we can process one at a time
            // and avoid spawning a large number of parallel tasks.
            prunedChildren.Add(await GetPrunedNode(child));
        }
        var directMatch = node.Twin is not null && await CanViewTwin(node.Twin.Id);

        NestedTwinDto newTree = new()
        {
            Twin = node.Twin,
            Children = prunedChildren
                .Where(r => r.Keep)
                .Select(r => r.Node)
                .ToList()
        };

        var keep = directMatch || prunedChildren.Any(r => r.Keep);
        return (newTree, keep);
    }

    private async ValueTask<bool> CanViewTwin(string twinId)
    {
        return await _authService.HasPermission<CanEditTwins>(_currentUser.Value, twinId) ||
               await _authService.HasPermission<CanViewSearchAndExplore>(_currentUser.Value, twinId) ||
               await _authService.HasPermission<CanViewTwins>(_currentUser.Value, twinId);
    }
}
