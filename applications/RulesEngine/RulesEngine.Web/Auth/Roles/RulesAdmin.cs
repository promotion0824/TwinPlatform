using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Rules admin role
/// </summary>
public class RulesAdmin : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesAdmin()
        : this(AuthPolicy.AdminPermissionSet)
    {
    }

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesAdmin(params IWillowAuthorizationRequirement[] permissons)
    {
        this.permissons = permissons ?? throw new System.ArgumentNullException(nameof(permissons));
    }

    /// <inheritdoc />
    public string Name => "RulesAdmin";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
