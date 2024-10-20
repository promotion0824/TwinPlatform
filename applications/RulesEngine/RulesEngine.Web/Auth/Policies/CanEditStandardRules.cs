using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Policy for editing rules
/// </summary>
public class CanEditStandardRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanEditStandardRules";

    /// <inheritdoc />
    public string Description => "Can edit standard rules";
}

/// <summary>
/// Policy evaluator for editing rules
/// </summary>
public class CanEditStandardRulesEvaluator(IUserService userService) : StandardRulesEvaluator<CanEditStandardRules>(userService)
{
}
