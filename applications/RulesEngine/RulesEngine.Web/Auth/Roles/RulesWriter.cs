using System.Collections.Generic;

namespace RulesEngine.Web;

/// <summary>
/// Rules writer role
/// </summary>
public class RulesWriter : IWillowRole
{
    private readonly IWillowAuthorizationRequirement[] permissons;

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesWriter()
        : this(AuthPolicy.ReadWritePermissionSet)
    {
    }

    /// <summary>
    /// Role constructor
    /// </summary>
    public RulesWriter(params IWillowAuthorizationRequirement[] permissons)
    {
        this.permissons = permissons ?? throw new System.ArgumentNullException(nameof(permissons));
    }

    /// <inheritdoc />
    public string Name => "RulesWriter";

    /// <inheritdoc />
    public IEnumerable<IWillowAuthorizationRequirement> Permissions => permissons;
}

