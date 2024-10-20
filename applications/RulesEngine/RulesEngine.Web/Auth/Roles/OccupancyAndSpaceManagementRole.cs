using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Occupancy And Space Management role
/// </summary>
/// <remarks>
/// You are responsible for ensuring optimal use of space within properties and enhancing the tenant/occupant experience. You liaise with tenants and departments, coordinate occupancy logistics, and drive strategies to make the best use of available spaces, ensuring tenant and department satisfaction and operational efficiency.
/// Job titles might include: Tenant Coordinator, Space and Experience Leads
/// </remarks>
public class OccupancyAndSpaceManagementRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public OccupancyAndSpaceManagementRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Occupancy and Space Management";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
