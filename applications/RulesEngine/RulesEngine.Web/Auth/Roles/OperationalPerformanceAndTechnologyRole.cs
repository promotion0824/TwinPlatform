using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Occupancy And Space Management role
/// </summary>
/// <remarks>
/// You harness technology and data analytics to drive operational excellence.
/// You focus on leveraging technology for facility improvements and derive insights from performance data to ensure the facilities are run optimally.
/// Job titles might include: Technology Stakeholder, Performance Analyst, Performance Engineer
/// </remarks>
public class OperationalPerformanceAndTechnologyRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public OperationalPerformanceAndTechnologyRole()
    {
        this.permissons = AuthPolicy.ReadOnlyPermissionSet;
    }

    /// <inheritdoc />
    public string Name => "Operational Performance and Technology";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
