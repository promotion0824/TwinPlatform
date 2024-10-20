namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing commands
/// </summary>
public class CanViewCommands : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewCommands";

    /// <inheritdoc />
    public string Description => "Can view commands";
}

/// <summary>
/// Policy evaluator for viewing commands
/// </summary>
public class CanViewCommandsEvaluator : PermissionEvaluator<CanViewCommands>
{
    /// <summary>
    /// Creates a new <see cref="CanViewCommandsEvaluator" />
    /// </summary>
    public CanViewCommandsEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
