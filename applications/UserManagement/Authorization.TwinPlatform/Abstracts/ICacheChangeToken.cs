using Microsoft.Extensions.Primitives;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Cache Change Token Interface.
/// </summary>
public interface ICacheChangeToken : IChangeToken
{
    /// <summary>
    /// Set the token as changed, so the underlying cache can be invalidated.
    /// </summary>
    public void TriggerChange();

}
