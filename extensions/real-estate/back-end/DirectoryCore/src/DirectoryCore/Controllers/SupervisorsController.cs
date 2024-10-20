using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Http;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SupervisorsController : TranslationController
    {
        private readonly ILogger<SupervisorsController> _logger;
        private readonly IAuth0Service _auth0Service;
        private readonly ISupervisorsService _supervisorsService;

        public SupervisorsController(
            ILogger<SupervisorsController> logger,
            IAuth0Service auth0Service,
            ISupervisorsService supervisorsService,
            IHttpRequestHeaders headers
        )
            : base(headers)
        {
            _logger = logger;
            _auth0Service = auth0Service;
            _supervisorsService = supervisorsService;
        }

        [HttpPost("supervisors/signIn")]
        [ProducesResponseType(typeof(SupervisorAuthenticationInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetAuthenticationToken(
            [FromQuery] string authorizationCode,
            [FromQuery] string redirectUri = null
        )
        {
            var auth0Response = await _auth0Service.GetAccessTokenByAuthCode(
                authorizationCode,
                redirectUri,
                false
            );

            if (auth0Response != null && !string.IsNullOrWhiteSpace(auth0Response.IdToken))
            {
                var result = new SupervisorAuthenticationInfo
                {
                    AccessToken = auth0Response.AccessToken,
                    ExpiresIn = auth0Response.ExpiresIn
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(auth0Response.IdToken);
                    var auth0UserId = jwtToken.Payload.Sub;
                    var willowSupervisor = await _supervisorsService.GetSupervisorByAuth0Id(
                        auth0UserId
                    );

                    if (willowSupervisor == null)
                    {
                        _logger.LogWarning(
                            "The user does not exist for '{Auth0UserId}'. User data from Auth0: {Auth0UserData}",
                            auth0UserId,
                            JsonConvert.SerializeObject(jwtToken.Payload)
                        );
                    }
                    else
                    {
                        result.Supervisor = SupervisorDto.MapFrom(willowSupervisor);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error has occurred processing Id Token");
                    throw;
                }

                return Ok(result);
            }

            return Forbid();
        }

        [HttpPost("supervisors/{supervisorEmail}/password/reset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ResetPassword(string supervisorEmail)
        {
            await _supervisorsService.SendResetPasswordEmail(supervisorEmail, Language);
            return NoContent();
        }

        [HttpGet("supervisors/{supervisorId}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSupervisor(Guid supervisorId)
        {
            var supervisor = await _supervisorsService.GetSupervisor(supervisorId);
            if (supervisor == null)
            {
                throw new ResourceNotFoundException("supervisor", supervisorId);
            }

            return Ok(SupervisorDto.MapFrom(supervisor));
        }

        [HttpGet("supervisors/resetPasswordTokens/{token}")]
        [ProducesResponseType(typeof(ResetPasswordTokenDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetResetPasswordToken([FromRoute] string token)
        {
            var email = await _supervisorsService.GetSupervisorEmailByToken(token);
            return Ok(new ResetPasswordTokenDto { Email = email });
        }

        [HttpPut("supervisors/{supervisorEmail}/password")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ChangeSupervisorPassword(
            string supervisorEmail,
            ChangePasswordRequest changePasswordRequest
        )
        {
            await _supervisorsService.ChangeSupervisorPassword(
                supervisorEmail,
                changePasswordRequest.EmailToken,
                changePasswordRequest.Password
            );
            return NoContent();
        }
    }
}
