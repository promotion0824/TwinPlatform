using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminPortalXL.Dto;
using AdminPortalXL.Services;
using AdminPortalXL.Security;
using AdminPortalXL.Models.Directory;
using Swashbuckle.AspNetCore.Annotations;

namespace AdminPortalXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SupervisorsController : ControllerBase
    {
        private readonly IDirectoryApiService _directoryApi;

        public SupervisorsController(IDirectoryApiService directoryApi)
        {
            _directoryApi = directoryApi;
        }

        [HttpGet("me")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(SupervisorDto), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCurrentUser()
        {
            var currentUserId = this.GetCurrentUserId();
            var user = await _directoryApi.GetSupervisor(currentUserId);
            return Ok(SupervisorDto.Map(user));
        }

        [HttpPost("supervisors/{supervisorEmail}/password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword(string supervisorEmail)
        {
            await _directoryApi.SendResetPasswordEmail(supervisorEmail);
            return NoContent();
        }

        [HttpGet("supervisors/resetPasswordTokens/{token}")]
        [ProducesResponseType(typeof(ResetPasswordToken), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetResetPasswordToken([FromRoute] string token)
        {
            var tokenInfo = await _directoryApi.GetResetPasswordToken(token);
            return Ok(tokenInfo);
        }

        [HttpPut("supervisors/{supervisorEmail}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the password", Tags = new [] { "Supervisors" })]
        public async Task<ActionResult> UpdatePassword([FromRoute] string supervisorEmail, [FromBody] UpdatePasswordRequest request)
        {
            await _directoryApi.UpdatePassword(supervisorEmail, request.Password, request.Token);
            return NoContent();
        }
    }
}
