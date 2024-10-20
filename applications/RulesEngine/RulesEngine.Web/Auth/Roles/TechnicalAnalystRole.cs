using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Willow non-prod reader role
/// </summary>
public class TechnicalAnalystRole : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public TechnicalAnalystRole()
    {
        this.permissons = [
            AuthPolicy.CanEditRules,
            AuthPolicy.CanViewInsights,
            AuthPolicy.CanViewCommands,
            AuthPolicy.CanViewModels,
            AuthPolicy.CanViewRules,
            AuthPolicy.CanViewStandardRules,
            AuthPolicy.CanViewTwins];
    }

    /// <inheritdoc />
    public string Name => "Technical Analyst";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
