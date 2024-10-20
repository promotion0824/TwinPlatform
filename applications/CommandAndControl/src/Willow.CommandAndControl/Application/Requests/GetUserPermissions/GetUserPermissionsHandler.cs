namespace Willow.CommandAndControl.Application.Requests.GetUserPermissions;

using System.Security.Claims;

internal class GetUserPermissionsHandler
{
    public static async Task<Results<Ok<UserPermissionsResponseDto>, UnauthorizedHttpResult>> HandleAsync(IUserAuthorizationService client, IHttpContextAccessor httpContextAccessor)
    {
        // Get the current user's email from the claims
        var currentUserEmailClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "emails" || x.Type == "email" || x.Type == ClaimTypes.Email);

        if (currentUserEmailClaim == null)
        {
            return TypedResults.Unauthorized();
        }

        var response = await client.GetAuthorizationResponse(currentUserEmailClaim.Value);

        return TypedResults.Ok(new UserPermissionsResponseDto()
        {
            Permissions = response.Permissions.Select(x => x.Name),
        });
    }
}
