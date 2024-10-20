using Authorization.TwinPlatform.Abstracts;

namespace Authorization.TwinPlatform.Models;

/// <summary>
/// Change Token implementation for authorization api.
/// </summary>
public class CacheChangeToken : ICacheChangeToken
{
    private bool _hasChanged;

    public bool HasChanged => _hasChanged;
    public bool ActiveChangeCallbacks => false;

    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => null;

    /// <summary>
    /// Set the token as changed, so the underlying cache can be invalidated.
    /// </summary>
    public void TriggerChange()
    {
        _hasChanged = true;
    }
}
