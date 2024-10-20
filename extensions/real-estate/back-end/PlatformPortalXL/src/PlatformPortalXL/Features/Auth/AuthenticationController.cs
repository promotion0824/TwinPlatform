using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Models;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Authorization.TwinPlatform.Common.Abstracts;
using PlatformPortalXL.Infrastructure.SingleTenant;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Features.Auth;
using PlatformPortalXL.Configs;

namespace PlatformPortalXL.Controllers
{
    internal record ValidateAuthInfoResponse(bool Validated, bool FgaEnabled, ObjectResult Result);

    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private const string RefreshTokenClaimType = "RefreshToken";

        private readonly IDirectoryApiService _directoryApi;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly SingleTenantOptions _singleTenantOptions;
        private readonly CustomerInstanceConfigurationOptions _customerInstanceConfigurationOptions;

        public AuthenticationController(IDirectoryApiService directoryApi,
                                        ILogger<AuthenticationController> logger,
                                        IOptions<SingleTenantOptions> singleTenantOptions,
                                        IOptions<CustomerInstanceConfigurationOptions> customerInstanceConfigurationOptions,
                                        IUserAuthorizationService userAuthorizationService = null)
        {
            _directoryApi = directoryApi;
            _logger = logger;
            _userAuthorizationService = userAuthorizationService;
            _singleTenantOptions = singleTenantOptions.Value;
            _customerInstanceConfigurationOptions = customerInstanceConfigurationOptions.Value;
        }

        /// <summary>
        /// Sign In with a token or authorization code.
        /// </summary>
        /// <param name="request">The token or Authorization code details</param>
        /// <returns>Expiry details</returns>
        [HttpPost("me/signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Sign in", Tags = new[] { "Users" })]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            _logger.LogInformation("Starting Signin process.");

            bool tokenExists = !string.IsNullOrEmpty(request.Token);
            var authenticationInfo = tokenExists
                ? await _directoryApi.SignInWithToken(request.Token)
                : await _directoryApi.SignInWithAuthorizationCode(request.AuthorizationCode, request.RedirectUri, request.CodeVerifier, request.SignInType);

            var (validated, fgaEnabled, result) = await ValidateAuthInfo(authenticationInfo);
            if (!validated)
            {
                _logger.LogInformation("Validate AuthInfo result {StatusCode} {Value}", result?.StatusCode, result?.Value);
                return result;
            }

            if (fgaEnabled && authenticationInfo.CustomerUser == null)
            {
                await FetchUserInfoFromUM(authenticationInfo);
            }

            await SetClaimsAndSignInUser(authenticationInfo, fgaEnabled);

            _logger.LogInformation("Completed HttpContext Signin.");

            var authenticationExpiry = new AuthenticationExpiry
            {
                UserId = GetCustomerUserIdForSignedInUser(authenticationInfo),
                ExpiresIn = authenticationInfo.ExpiresIn
            };

            return Ok(authenticationExpiry);
        }

        [HttpPost("me/refreshSession")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Refresh Session", Tags = new[] { "Users" })]
        public async Task<IActionResult> RefreshSession()
        {
            var refreshTokenClaim = User.Claims?.SingleOrDefault(x => x.Type == RefreshTokenClaimType);
            if (refreshTokenClaim == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Refresh token could not be found.");
            }

            var authenticationInfo = await _directoryApi.RequestNewAccessToken(refreshTokenClaim.Value);

            var (validated, fgaEnabled, result) = await ValidateAuthInfo(authenticationInfo);
            if (!validated)
            {
                return result;
            }

            if (fgaEnabled && authenticationInfo.CustomerUser == null)
            {
                await FetchUserInfoFromUM(authenticationInfo);
            }

            await SetClaimsAndSignInUser(authenticationInfo, fgaEnabled);

            var authenticationExpiry = new AuthenticationExpiry
            {
                UserId = GetCustomerUserIdForSignedInUser(authenticationInfo),
                ExpiresIn = authenticationInfo.ExpiresIn
            };

            return Ok(authenticationExpiry);
        }


        [HttpPost("me/signout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Sign out", Tags = new[] { "Users" })]
        public async Task<IActionResult> SignMeOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        private async Task<ValidateAuthInfoResponse> ValidateAuthInfo(AuthenticationInfo authenticationInfo)
        {
            _logger.LogInformation("Validating access token");

            if (string.IsNullOrEmpty(authenticationInfo?.AccessToken))
            {
                return new ValidateAuthInfoResponse(false, false, StatusCode(StatusCodes.Status403Forbidden, "No token obtained"));
            }

            // Consult the User Management service to check whether the user is one of the
            // AD groups that gives admin access to the system.
            var isFgaEnabled = false;

            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(authenticationInfo.AccessToken);
            var emailClaim = jwtToken.Payload.Claims.FirstOrDefault(x => x.Type is ClaimTypes.Email or "email");
            if (emailClaim == null)
            {
                return new ValidateAuthInfoResponse(false, false, StatusCode(StatusCodes.Status403Forbidden, "Email claim not found"));
            }

            // Get the list of authorized permission for the supplied user email from api/permission, internally
            // results are cached
            var authResult = await _userAuthorizationService.GetAuthorizationResponse(emailClaim.Value);

            var isAdmin = authResult.Permissions.Any(p => p.Name == nameof(CanActAsCustomerAdmin));
            isFgaEnabled = authResult.Permissions.Any(r => r.Name == nameof(CanUseFineGrainedAuth));

            using var scope = _logger.BeginScope("{Permissions}",
                authResult.Permissions.Any() ? string.Join(", ", authResult.Permissions.Select(p => p.Name)) : "[None]");
            _logger.LogInformation("Single tenant user check: {UserEmail}, FGA: {IsFgaEnabled}, is admin: {IsAdmin}", emailClaim.Value, isFgaEnabled, isAdmin);

            if (isAdmin || isFgaEnabled)
            {
                _logger.LogInformation("Signin as a group user");
                return new ValidateAuthInfoResponse(true, isFgaEnabled, null);
            }

            if (authenticationInfo.CustomerUser == null)
            {
                return new ValidateAuthInfoResponse(false, isFgaEnabled, StatusCode(StatusCodes.Status403Forbidden, "User is not found in database"));
            }

            return new ValidateAuthInfoResponse(true, isFgaEnabled, null);
        }

        private async Task FetchUserInfoFromUM(AuthenticationInfo authenticationInfo)
        {
            var emailClaim = new JwtSecurityTokenHandler().ReadJwtToken(authenticationInfo.AccessToken).Payload.Claims
                                                          .FirstOrDefault(x => x.Type is ClaimTypes.Email or "email");

            var userFromUM = await _userAuthorizationService.GetUserByEmailAsync(emailClaim?.Value);
            authenticationInfo.CustomerUser = new Willow.Platform.Users.User
            {
                // Assign the statice customer user id for AD group user
                Id = userFromUM?.Id ?? Guid.Parse(_singleTenantOptions.CustomerUserIdForGroupUser),
                CustomerId = Guid.Parse(_customerInstanceConfigurationOptions.Id)
            };
        }

        private async Task SetClaimsAndSignInUser(AuthenticationInfo authenticationInfo, bool isFgaEnabled)
        {
            var currentClaims = new JwtSecurityTokenHandler()
                                            .ReadJwtToken(authenticationInfo.AccessToken).Payload.Claims.ToArray();
            var emailClaim = currentClaims.FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "email")?.Value;
            var nameClaim = currentClaims.FirstOrDefault(x => x.Type == "name")?.Value;

            Claim[] revisedClaims =
            [
                new Claim(ClaimTypes.Authentication, authenticationInfo.AccessToken),
                // Use a static userId for single tenant group user login
                new Claim(ClaimTypes.NameIdentifier, GetCustomerUserIdForSignedInUser(authenticationInfo).ToString()),
                new Claim(RefreshTokenClaimType, authenticationInfo.RefreshToken ?? string.Empty),
                new Claim(ClaimTypes.Email, emailClaim ?? string.Empty),
                new Claim(ClaimTypes.Name, nameClaim ?? string.Empty),
                new Claim(CustomClaimTypes.CustomerId, authenticationInfo.CustomerUser?.CustomerId.ToString() ?? string.Empty),
                new Claim(CustomClaimTypes.IsFineGrainedAuthEnabled, isFgaEnabled.ToString())
            ];
            var claimsIdentity = new ClaimsIdentity(revisedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(authenticationInfo.ExpiresIn),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            _logger.LogInformation("Start HttpContext Signin");
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private Guid GetCustomerUserIdForSignedInUser(AuthenticationInfo authenticationInfo)
        {
            // Use a static userId for AD group user login
            return authenticationInfo.CustomerUser?.Id ?? new Guid(_singleTenantOptions.CustomerUserIdForGroupUser);
        }
    }
}
