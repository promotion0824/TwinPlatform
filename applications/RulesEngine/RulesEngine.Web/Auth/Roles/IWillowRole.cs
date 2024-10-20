using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// A Willow user role
/// </summary>
public interface IWillowRole
{
    /// <summary>
    /// The role name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Permissions configured for this role
    /// </summary>
    public IEnumerable<IWillowAuthorizationRequirement> Permissions { get; }
}
