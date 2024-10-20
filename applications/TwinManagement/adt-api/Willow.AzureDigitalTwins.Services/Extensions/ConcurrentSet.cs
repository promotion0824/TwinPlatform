using System.Collections.Concurrent;

namespace Willow.AzureDigitalTwins.Services.Extensions;

public class ConcurrentSet<T>
{
    private ConcurrentDictionary<T, bool> _dict = new();
    public void Add(T val)
    {
        _dict[val] = true;
    }
    public bool Contains(T val)
    {
        return _dict.ContainsKey(val);
    }

    public bool Remove(T val)
    {
        return _dict.TryRemove(val, out _);
    }
}
