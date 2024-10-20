using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Facility And Infrastructure Management role
/// </summary>
/// <remarks>
/// You oversee the physical infrastructure of the properties. You ensure facilities are maintained at peak condition, pre-empt potential issues, and drive initiatives that uphold the structural integrity and functionality of the buildings.
/// Job titles might include: Facility Manager, Building Engineer, Maintenance Supervisor
/// </remarks>
public class FacilityAndInfrastructureManagementRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public FacilityAndInfrastructureManagementRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Facility and Infrastructure Management";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
