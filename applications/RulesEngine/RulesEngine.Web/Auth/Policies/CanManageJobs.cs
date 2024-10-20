namespace RulesEngine.Web;

/// <summary>
/// Policy for executing rules
/// </summary>
public class CanManageJobs : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanManageJobs";

    /// <inheritdoc />
    public string Description => "Can manage jobs";
}

/// <summary>
/// Policy evaluator for executing rules
/// </summary>
public class CanManageJobsEvaluator : PermissionEvaluator<CanManageJobs>
{
    /// <summary>
    /// Creates a new <see cref="CanManageJobsEvaluator" />
    /// </summary>
    public CanManageJobsEvaluator(IUserService userService)
        : base(userService)
    {
    }
}
