using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow non-prod reader role
/// </summary>
public class WillowNonProdRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowNonProdRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Non-Prod";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
