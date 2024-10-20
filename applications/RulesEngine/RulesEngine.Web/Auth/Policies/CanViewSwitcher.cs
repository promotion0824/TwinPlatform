namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing rules
/// </summary>
public class CanViewSwitcher : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewSwitcher";

    /// <inheritdoc />
    public string Description => "Can view switcher";
}

/// <summary>
/// Policy evaluator for viewing rules
/// </summary>
public class CanViewSwitcherEvaluator : PermissionEvaluator<CanViewSwitcher>
{
    /// <summary>
    /// Creates a new <see cref="CanViewSwitcherEvaluator" />
    /// </summary>
    public CanViewSwitcherEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
