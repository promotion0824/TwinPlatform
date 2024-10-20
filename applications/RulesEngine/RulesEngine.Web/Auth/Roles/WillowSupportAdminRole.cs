using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow Support Admins role
/// </summary>
/// <remarks>
/// This is a PIM-only group for production
/// </remarks>
public class WillowSupportAdminRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowSupportAdminRole()
    {
        this.permissons = AuthPolicy.AdminPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Support Admin";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
