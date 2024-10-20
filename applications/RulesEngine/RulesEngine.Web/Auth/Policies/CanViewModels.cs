namespace RulesEngine.Web;

/// <summary>
/// Policy for viewing models
/// </summary>
public class CanViewModels : IWillowAuthorizationRequirement
{
    /// <inheritdoc />
    public string Name => "CanViewModels";

    /// <inheritdoc />
    public string Description => "Can view models";
}

/// <summary>
/// Policy evaluator for viewing models
/// </summary>
public class CanViewModelsEvaluator : PermissionEvaluator<CanViewModels>
{

    /// <summary>
    /// Creates a new <see cref="CanViewModelsEvaluator" />
    /// </summary>
    public CanViewModelsEvaluator(IUserService userService)
        : base(userService)
    {

    }


}
