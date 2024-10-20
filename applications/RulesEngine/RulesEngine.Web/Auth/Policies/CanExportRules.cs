namespace RulesEngine.Web;

/// <summary>
/// Policy for exporting rules
/// </summary>
public class CanExportRules : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanExportRules";

    /// <inheritdoc />
    public string Description => "Can export rules";
}

/// <summary>
/// Policy evaluator for exporting rules
/// </summary>
public class CanExportRulesEvaluator : PermissionEvaluator<CanExportRules>
{
    /// <summary>
    /// Creates a new <see cref="CanExportRulesEvaluator" />
    /// </summary>
    public CanExportRulesEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
