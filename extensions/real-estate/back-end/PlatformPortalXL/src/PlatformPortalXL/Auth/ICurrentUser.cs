using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PlatformPortalXL.Auth;

public interface ICurrentUser
{
    ClaimsPrincipal Value { get; }
}

public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal Value => _httpContextAccessor.HttpContext?.User;
}
