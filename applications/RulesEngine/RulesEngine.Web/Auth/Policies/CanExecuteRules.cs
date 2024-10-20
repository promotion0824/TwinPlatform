namespace RulesEngine.Web;

/// <summary>
/// Policy for executing rules
/// </summary>
public class CanExecuteRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanExecuteRules";

    /// <inheritdoc />
    public string Description => "Can execute rules";
}

/// <summary>
/// Policy evaluator for executing rules
/// </summary>
public class CanExecuteRulesEvaluator : PermissionEvaluator<CanExecuteRules>
{
    /// <summary>
    /// Creates a new <see cref="CanExecuteRulesEvaluator" />
    /// </summary>
    public CanExecuteRulesEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
