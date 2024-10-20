namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

using System.Security.Claims;
using Willow.IoTService.Deployment.DataAccess.PortService;

public class UserInfoService(IHttpContextAccessor httpContextAccessor) : IUserInfoService
{
    public string GetUserName()
    {
        var userClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "name");
        if (userClaim != null)
        {
            return userClaim.Value;
        }

        var userEmailClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type is "emails" or "email" or ClaimTypes.Email);
        return userEmailClaim != null ? userEmailClaim.Value : "Unknown User";
    }

    public string GetUserId()
    {
        var userEmailClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type is "emails" or "email" or ClaimTypes.Email);
        return userEmailClaim != null ? userEmailClaim.Value : GetUserName();
    }
}
