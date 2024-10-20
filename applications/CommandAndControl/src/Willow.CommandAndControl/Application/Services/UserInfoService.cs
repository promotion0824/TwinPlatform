namespace Willow.CommandAndControl.Application.Services;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Class to provide information about the logged in user.
/// </summary>
public class UserInfoService : IUserInfoService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoService"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Context accessor.</param>
    public UserInfoService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Returns the username of the logged in user.
    /// </summary>
    /// <returns>A user, or null if the username and email cannot be found in the auth token.</returns>
    public User? GetUser()
    {
        return IsAuthenticated() ? ReadUserInfo() : null;
    }

    private bool IsAuthenticated()
    {
        var isAuthenticated = httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated;

        return isAuthenticated.HasValue && isAuthenticated.Value;
    }

    private User ReadUserInfo()
    {
        var email = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "emails" || x.Type == "email" || x.Type == ClaimTypes.Email);
        var name = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "name");
        return new User
        {
            Email = email?.Value,
            Name = name?.Value,
        };
    }
}
