using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using AdminPortalXL.Security;

namespace AdminPortalXL.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IDirectoryApiService _directoryApi;

        public AuthenticationController(IDirectoryApiService directoryApi)
        {
            _directoryApi = directoryApi;
        }

        [HttpPost("me/signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Sign in")]
        public async Task<IActionResult> SignIn([FromBody]SignInRequest request)
        {
            var authenticationInfo = await _directoryApi.GetAuthenticationToken(request.AuthorizationCode, request.RedirectUri);
            if (string.IsNullOrEmpty(authenticationInfo?.AccessToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "No token obtained");
            }

            if (authenticationInfo.Supervisor == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "User is not found in database");
            }

            var claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Authentication, authenticationInfo.AccessToken),
                    new Claim(ClaimTypes.NameIdentifier, authenticationInfo.Supervisor.Id.ToString()),
                    new Claim(ClaimTypes.Role, UserRoles.Supervisor)
                },
                CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(authenticationInfo.ExpiresIn),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok();
        }

        [HttpPost("me/signout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Sign out")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

    }
}
