namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing rules
/// </summary>
public class CanViewRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewRules";

    /// <inheritdoc />
    public string Description => "Can view rules";
}

/// <summary>
/// Policy evaluator for viewing rules
/// </summary>
public class CanViewRulesEvaluator : PermissionEvaluator<CanViewRules>
{
    /// <summary>
    /// Creates a new <see cref="CanViewRulesEvaluator" />
    /// </summary>
    public CanViewRulesEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
