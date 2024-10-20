using Microsoft.AspNetCore.Authorization;

namespace RulesEngine.Web;

/// <summary>
/// The Willow authorisation requirement for permissions
/// </summary>
public interface IWillowAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The name of the permission for this requirement
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the permission
    /// </summary>
    public string Description { get; }
}
