using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow non-prod admin role
/// </summary>
public class WillowNonProdAdminRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowNonProdAdminRole()
    {
        this.permissons = AuthPolicy.AdminPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Non-Prod Admin";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
