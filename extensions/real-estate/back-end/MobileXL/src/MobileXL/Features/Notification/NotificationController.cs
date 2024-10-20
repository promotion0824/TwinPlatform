using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Features.Directory;
using MobileXL.Security;
using MobileXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Common;

namespace MobileXL.Features.Notification
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]

    public class NotificationController : ControllerBase
    {
        private readonly IPushNotificationServer _pushNotificationServer;
        public NotificationController(IPushNotificationServer pushNotificationServer)
        {
            _pushNotificationServer = pushNotificationServer;
        }

        [HttpPost("installations")]
        [MobileAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Create or Update signed in user's device", Tags = new [] { "Users" })]
        public async Task<IActionResult> CreateOrUpdateInstallation([FromBody]InstallationRequest installationRequest)
        {
            if ((string.Compare(installationRequest.Platform.ToString(), "fcm", true) != 0) && (string.Compare(installationRequest.Platform.ToString(), "apns", true) != 0))
            {
                throw new ArgumentException("Unsupported installation platform requested.").WithData(new { InstallationRequest = installationRequest });
            }

            var userId = this.GetCurrentUserId();
            await _pushNotificationServer.AddOrUpdateInstallation(userId, installationRequest.Handle, installationRequest.Platform.ToString());
            return NoContent();
        }

        [HttpDelete("installations")]
        [MobileAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Delete signed in user device installation", Tags = new [] { "Users" })]
        public async Task<IActionResult> DeleteInstallation(string pnsHandle)
        {
            var userId = this.GetCurrentUserId();
            await _pushNotificationServer.DeleteInstallation(userId, pnsHandle);
            return NoContent();
        }

    }
}
