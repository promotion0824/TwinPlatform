using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Technical And Maintenance Expertise role
/// </summary>
/// <remarks>
/// You are experts in the hands-on technical aspects of property maintenance.
/// You ensure that all building systems, from HVAC to plumbing, function efficiently.
/// You diagnose issues, conduct repairs, and ensure the technical health of the property.
/// Job titles might include: Maintenance Technician, Specialist - HVAC Technician, Electrician, Plumber
/// </remarks>
public class TechnicalAndMaintenanceExpertiseRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public TechnicalAndMaintenanceExpertiseRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Technical and Maintenance Expertise";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
