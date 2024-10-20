using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Model;
using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Auth.Extensions;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Visitors;
using Willow.ExpressionParser;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Evaluates user permissions by checking twins in the search index.
/// </summary>
/// <remarks>
/// When evaluating a user's permission to access a twin the service will check ancestral twins in the Azure AI Search
/// index. For example if provided with a twin representing room R1 on floor F3 of building B3 if the user is
/// authorised for B3 they will also be authorised for F3 and R1.
/// </remarks>
public class UserPermissionsEvaluation
{
    private readonly IEnumerable<AuthorizedPermission> _permissions;
    private readonly IAncestralTwinsSearchService _ancestralTwinsSearchService;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of <see cref="UserPermissionsEvaluation"/>.
    /// </summary>
    /// <param name="permissions">Set of permissions returned from User Management.</param>
    /// <param name="ancestralTwinsSearchService">Service for looking up twins with ancestors.</param>
    /// <param name="memoryCache">Represents a local in-memory cache.</param>
    public UserPermissionsEvaluation(
        IEnumerable<AuthorizedPermission> permissions,
        IAncestralTwinsSearchService ancestralTwinsSearchService,
        IMemoryCache memoryCache)
    {
        _permissions = permissions;
        _ancestralTwinsSearchService = ancestralTwinsSearchService;
        _memoryCache = memoryCache;
    }

    public async Task<bool> HasPermission<T>(string scopeId) where T : WillowAuthorizationRequirement
    {
        var twin = await ResolveTwin(scopeId);

        return HasPermission<T>(twin);
    }

    public bool HasPermission<T>(ITwinWithAncestors twin) where T : WillowAuthorizationRequirement
    {
        var permsOfT = _permissions.Where(p => p.Name == typeof(T).Name);
        var twinPermissions = Evaluate(twin, permsOfT);
        return twinPermissions.Count != 0;
    }

    private static HashSet<string> Evaluate(ITwinWithAncestors twin, IEnumerable<AuthorizedPermission> permissions)
    {
        var expressionVisitor = new AncestralTwinExpressionVisitor(twin);
        HashSet<string> grantedPerms = [];

        foreach (var permission in permissions)
        {
            var expression = Parser.Deserialize(permission.Expression);

            if (!grantedPerms.Contains(permission.Name) &&
                // Is global, i.e. has no scope, or expression accepts the ancestral twin expression visitor.
                (permission.IsGlobalAssignment() || expressionVisitor.Visit(expression)))
            {
                grantedPerms.Add(permission.Name);
            }
        }

        return grantedPerms;
    }

    /// <summary>
    /// Load twin with locations from the search index.
    /// </summary>
    private async Task<ITwinWithAncestors> ResolveTwin(string twinId)
    {
        return await _memoryCache.GetOrCreateAsync(TwinCaching.CacheKeyForAncestorsLookup(twinId), async cacheEntry =>
        {
            TwinCaching.SetCachingDefaults(cacheEntry);

            return await _ancestralTwinsSearchService.GetTwinById(twinId).ConfigureAwait(false);
        });
    }
}
