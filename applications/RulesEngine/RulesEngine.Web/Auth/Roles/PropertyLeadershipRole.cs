using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Property Leadership role
/// </summary>
/// <remarks>
/// You are focused on the overarching strategy and direction of property assets.
/// You are responsible for driving value from real estate portfolios, making informed decisions based on property analytics, and ensuring that the properties align with the broader organisational objectives.
/// Job titles might include: Asset Manager, Property Manager, Sustainability Manager, Property Analyst, …
/// </remarks>
public class PropertyLeadershipRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public PropertyLeadershipRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Property Leadership";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
