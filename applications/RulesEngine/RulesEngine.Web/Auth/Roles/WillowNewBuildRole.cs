using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow New Build Team role
/// </summary>
public class WillowNewBuildRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowNewBuildRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow New Build";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
