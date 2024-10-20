namespace Willow.PublicApi.Extensions;

using System.IdentityModel.Tokens.Jwt;

internal static class HttpContextExtensions
{
    public static string? GetClientIdFromToken(this HttpContext httpContext)
    {
        string? authorizationHeader = httpContext.Request.Headers.Authorization;
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null;
        }

        string token = authorizationHeader["Bearer ".Length..].Trim();
        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(token);

        string? clientId = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "appid" || claim.Type == "azp")?.Value;

        return clientId;
    }

    public static string? GetClientIdFromBody(this HttpContext httpContext)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return null;
        }

        httpContext.Request.EnableBuffering();

        httpContext.Request.Form.TryGetValue("client_id", out var clientId);

        httpContext.Request.Body.Position = 0;

        return clientId;
    }
}
