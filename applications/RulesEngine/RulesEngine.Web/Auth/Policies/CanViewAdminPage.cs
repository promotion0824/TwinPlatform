namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing admin pages
/// </summary>
public class CanViewAdminPage : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewAdminPage";

    /// <inheritdoc />
    public string Description => "Can view admin page";
}

/// <summary>
/// Policy evaluator for viewing admin page
/// </summary>
public class CanViewAdminPageEvaluator : PermissionEvaluator<CanViewAdminPage>
{
    /// <summary>
    /// Creates a new <see cref="CanViewAdminPageEvaluator" />
    /// </summary>
    public CanViewAdminPageEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
