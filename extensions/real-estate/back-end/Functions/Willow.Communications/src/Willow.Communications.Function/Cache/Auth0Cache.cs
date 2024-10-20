using System;
using Willow.Common;

namespace Willow.Communications.Function.Cache;
internal interface IAuth0Cache
{
    public ICache Cache { get; }
}

internal class Auth0Cache : IAuth0Cache
{
    private readonly ICache _cache;

    public Auth0Cache(ICache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public ICache Cache => _cache;
}
