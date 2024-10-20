namespace Willow.Api.Common.Extensions;

using Microsoft.AspNetCore.Http;

/// <summary>
/// A class containing extension methods for <see cref="HttpContext"/>.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the bearer token from the request headers.
    /// </summary>
    /// <param name="httpContext">The http context of the request.</param>
    /// <returns>The bearer token if it is present in the request. Null if not.</returns>
    public static string? GetBearerToken(this HttpContext httpContext)
    {
        var requestHeaders = httpContext.Request.Headers;
        var authorizationHeader = requestHeaders["Authorization"];

        if (authorizationHeader.Count > 0)
        {
            var tokenHeader = authorizationHeader[0];

            if (tokenHeader != null)
            {
                return tokenHeader["Bearer ".Length..];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a claim from the request headers.
    /// </summary>
    /// <param name="httpContext">The Http Context of the request.</param>
    /// <param name="type">The type of claim to find.</param>
    /// <returns>The claim value if present. Null if not.</returns>
    public static string? GetClaimValue(this HttpContext httpContext, string type)
    {
        return httpContext.User.FindFirst(type)?.Value;
    }
}
