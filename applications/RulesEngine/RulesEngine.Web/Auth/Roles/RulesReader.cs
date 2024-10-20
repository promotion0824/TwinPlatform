using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Rules reader role
/// </summary>
public class RulesReader : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesReader()
        : this(AuthPolicy.ReadOnlyPermissionSet)
    {
    }

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesReader(params IWillowAuthorizationRequirement[] permissons)
    {
        this.permissons = permissons ?? throw new System.ArgumentNullException(nameof(permissons));
    }

    /// <inheritdoc />
    public string Name => "RulesReader";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}
