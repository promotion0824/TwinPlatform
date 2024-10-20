using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow Performance Engineers role
/// </summary>
public class WillowPerformanceEngineerRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowPerformanceEngineerRole()
    {
        this.permissons = AuthPolicy.AdminPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Performance Engineer";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
