using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Models;
using MobileXL.Security;
using MobileXL.Services.Apis.DirectoryApi;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using MobileXL.Options;
using Authorization.TwinPlatform.Common.Abstracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace MobileXL.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private const string RefreshTokenClaimType = "RefreshToken";

        private readonly IDirectoryApiService _directoryApi;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly SingleTenantOptions _options;
        private readonly IUserAuthorizationService _userAuthorizationService;

        public AuthenticationController(IDirectoryApiService directoryApi,
                                        ILogger<AuthenticationController> logger,
                                        IOptions<SingleTenantOptions> options,
                                        IUserAuthorizationService userAuthorizationService = null)
        {
            _directoryApi = directoryApi;
            _logger = logger;
            _options = options.Value;
            _userAuthorizationService = userAuthorizationService;
        }

        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Sign in", Tags = new[] { "Authentication" })]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            _logger.LogInformation("Starting Signin process.");

            var authInfo = await _directoryApi.SignIn(request.AuthorizationCode, request.RedirectUri, request.CodeVerifier, request.SignInType);

            var (success, result) = await ValidateAuthInfo(authInfo);
            if (!success)
            {
                return result;
            }

            await SetClaimsAndSignInUser(authInfo);

            var authenticationExpiry = new AuthenticationExpiry
            {
                UserId = GetUserIdForSignedInUser(authInfo),
                ExpiresIn = authInfo.ExpiresIn
            };

            _logger.LogInformation("Completed Signin process.");

            return Ok(authenticationExpiry);
        }

        [HttpPost("refreshSession")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Refresh Session", Tags = new[] { "Authentication" })]
        public async Task<IActionResult> RefreshSession()
        {
            var refreshTokenClaim = User.Claims?.SingleOrDefault(x => x.Type == RefreshTokenClaimType);
            if (refreshTokenClaim == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Refresh token could not be found.");
            }

            var authInfo = await _directoryApi.RequestNewAccessToken(refreshTokenClaim.Value);

            var (success, result) = await ValidateAuthInfo(authInfo);
            if (!success)
            {
                return result;
            }

            await SetClaimsAndSignInUser(authInfo);

            var authenticationExpiry = new AuthenticationExpiry
            {
                UserId = GetUserIdForSignedInUser(authInfo),
                ExpiresIn = authInfo.ExpiresIn
            };

            return Ok(authenticationExpiry);
        }

        [HttpPost("signout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Sign out", Tags = new[] { "Authentication" })]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(PlatformAuthenticationSchemes.MobileCookieScheme);
            return NoContent();
        }

        private async Task<(bool, IActionResult)> ValidateAuthInfo(AuthenticationInfo authenticationInfo)
        {
            if (string.IsNullOrEmpty(authenticationInfo?.AccessToken))
            {
                return (false, StatusCode(StatusCodes.Status403Forbidden, "No token obtained"));
            }

            // Verify if the user is a group user and have correct permission assigned
            if (_options.UseSingleTenant)
            {
                var userEmail = new JwtSecurityTokenHandler().ReadJwtToken(authenticationInfo.AccessToken).Payload.Claims
                                                                .FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "email");
                var authResult = await _userAuthorizationService.GetAuthorizationResponse(userEmail?.Value);
                if (authResult != null && authResult.Permissions.Any(p => p.Name == "CanActAsCustomerAdmin"))
                {
                    _logger.LogInformation("Signin as a group user.");
                    return (true, null);
                }
            }

            if (authenticationInfo.CustomerUser == null)
            {
                return (false, StatusCode(StatusCodes.Status403Forbidden, "CustomerUser is not found in database"));
            }

            return (true, null);
        }

        private async Task SetClaimsAndSignInUser(AuthenticationInfo authInfo)
        {
            var claims = new JwtSecurityTokenHandler().ReadJwtToken(authInfo.AccessToken).Payload.Claims;
            var emailAddressClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "email")?.Value;
            var nameClaim = claims.FirstOrDefault(x => x.Type == "name")?.Value;

            var userType = UserTypeNames.CustomerUser;

            var claimsIdentity = new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Authentication, authInfo.AccessToken),
                    new Claim(ClaimTypes.NameIdentifier, GetUserIdForSignedInUser(authInfo).ToString()),
                    new Claim(ClaimTypes.Role, userType),
                    new Claim(RefreshTokenClaimType, authInfo.RefreshToken ?? string.Empty),
                    new Claim(ClaimTypes.Email, emailAddressClaim ?? string.Empty),
                    new Claim(ClaimTypes.Name, nameClaim ?? string.Empty)
                },
                PlatformAuthenticationSchemes.MobileCookieScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
				//The cookie lifetime should be longer than access token lifetime for refresh token to work
				//Default life time of refresh token is : 14 days 7776000
				//https://learn.microsoft.com/en-us/azure/active-directory-b2c/configure-tokens?pivots=b2c-custom-policy#configure-token-lifetime

				ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                PlatformAuthenticationSchemes.MobileCookieScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        [HttpGet("heartbeat")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Heart Beat", Tags = new[] { "HeartBeat" })]
        public IActionResult HeartBeat()
        {
            return NoContent();
        }

        private Guid GetUserIdForSignedInUser(AuthenticationInfo authInfo)
        {
            var userId = authInfo.CustomerUser?.Id;
            // Use a static userId for single tenant group user login, otherwise just use the customer user id
            return userId ?? (_options.UseSingleTenant ? new Guid(_options.CustomerUserIdForGroupUser) : Guid.Empty);
        }
    }
}
