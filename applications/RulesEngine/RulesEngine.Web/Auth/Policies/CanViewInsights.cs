namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing insights
/// </summary>
public class CanViewInsights : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewInsights";

    /// <inheritdoc />
    public string Description => "Can view insights";
}

/// <summary>
/// Policy evaluator for viewing insights
/// </summary>
public class CanViewInsightsEvaluator : PermissionEvaluator<CanViewInsights>
{
    /// <summary>
    /// Creates a new <see cref="CanViewInsightsEvaluator" />
    /// </summary>
    public CanViewInsightsEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
