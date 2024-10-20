using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow Delivery Team role
/// </summary>
public class WillowDeliveryRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public WillowDeliveryRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Willow Delivery";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
