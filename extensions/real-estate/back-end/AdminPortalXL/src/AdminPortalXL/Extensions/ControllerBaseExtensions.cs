using System;
using System.Security.Claims;
using Willow.Infrastructure.Exceptions;

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
                    throw new BadRequestException("Cannot find the user from NameId claim or UserData claim.");
                }
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new BadRequestException("Failed to parse user Id from the claim.");
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
            catch(BadRequestException)
            {
                userId = Guid.Empty;
                return false;
            }
        }
    }
}
