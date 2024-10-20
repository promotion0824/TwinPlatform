using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerBaseExtensions
    {
        public static Guid GetCurrentUserId(this ControllerBase controller)
        {
            // Token authentication uses UserData claim for storing user id.
            var userIdClaim = controller.User.FindFirst(x => x.Type == ClaimTypes.UserData);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                // Cookie authentication uses NameIdentifier claim for storing user id.
                userIdClaim = controller.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    throw new ArgumentNullException( "Cannot find the user from NameId claim or UserData claim.");
                }
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new ArgumentException("Failed to parse user Id from the claim.");
            }

            return userId;
        }

        public static bool TryGetCurrentUserId(this ControllerBase controller, out Guid userId)
        {
            try
            {
                userId = controller.GetCurrentUserId();
                return true;
            }
            catch(Exception)
            {
                userId = Guid.Empty;
                return false;
            }
        }

        public static string GetUserEmail(this ControllerBase controller)
        {
            var userEmail = controller.User.FindFirst(x => x.Type == ClaimTypes.Email);
            return userEmail?.Value;
        }
    }
}
