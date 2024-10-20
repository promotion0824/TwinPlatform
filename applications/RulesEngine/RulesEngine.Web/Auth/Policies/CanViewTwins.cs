namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing models
/// </summary>
public class CanViewTwins : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewTwins";

    /// <inheritdoc />
    public string Description => "Can view twins";
}

/// <summary>
/// Policy evaluator for viewing models
/// </summary>
public class CanViewTwinsEvaluator : PermissionEvaluator<CanViewTwins>
{
    /// <summary>
    /// Creates a new <see cref="CanViewTwinsEvaluator" />
    /// </summary>
    public CanViewTwinsEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
