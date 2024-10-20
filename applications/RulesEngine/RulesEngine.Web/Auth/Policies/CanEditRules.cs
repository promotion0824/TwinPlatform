namespace RulesEngine.Web;

/// <summary>
/// Policy for editing rules
/// </summary>
public class CanEditRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanEditRules";

    /// <inheritdoc />
    public string Description => "Can edit rules";
}

/// <summary>
/// Policy evaluator for editing rules
/// </summary>
public class CanEditRulesEvaluator : PermissionEvaluator<CanEditRules>
{
    /// <summary>
    /// Creates a new <see cref="CanEditRulesEvaluator" />
    /// </summary>
    public CanEditRulesEvaluator(IUserService userService)
        : base(userService)
	{
	}
}
