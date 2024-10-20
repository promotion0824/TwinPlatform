using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace PlatformPortalXL.Helpers
{
    public interface IControllerHelper
    {
        Guid GetCurrentUserId(ControllerBase controller);
    }

    public class ControllerHelper : IControllerHelper
    {
        public Guid GetCurrentUserId(ControllerBase controller)
        {
            // Token authentication uses UserData claim for storing user id.
            var userIdClaim = controller.User.FindFirst(x => x.Type == ClaimTypes.UserData);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                // Cookie authentication uses NameIdentifier claim for storing user id.
                userIdClaim = controller.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    throw new ArgumentNullException("Cannot find the user from NameId claim or UserData claim.");
                }
            }

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new ArgumentException("Failed to parse user Id from the claim.");
            }

            return userId;
        }
    }
}
