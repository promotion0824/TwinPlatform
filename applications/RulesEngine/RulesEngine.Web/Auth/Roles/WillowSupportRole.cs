using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow Support role (reader)
/// </summary>
/// /// <remarks>
/// This is a PIM-only group for production
/// </remarks>
public class WillowSupportRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowSupportRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Support";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
