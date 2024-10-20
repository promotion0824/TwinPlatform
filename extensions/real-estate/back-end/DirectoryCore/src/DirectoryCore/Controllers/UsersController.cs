using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Controllers.Responses;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Enums;
using DirectoryCore.Http;
using DirectoryCore.Infrastructure.Extensions;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using DirectoryCore.Services.AzureB2C;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class UsersController : TranslationController
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IAuth0Service _auth0Service;
        private readonly IAzureB2CService _azureB2CService;
        private readonly IUsersService _usersService;
        private readonly ISupervisorsService _supervisorsService;
        private readonly IConfiguration _configuration;
        private readonly AzureADB2COptions _azureB2COptions;
        private readonly WillowContextOptions _willowContextOptions;
        private readonly Guid _customerId;

        public UsersController(
            ILogger<UsersController> logger,
            IAuth0Service auth0Service,
            IAzureB2CService azureB2CService,
            IUsersService usersService,
            ISupervisorsService supervisorsService,
            IConfiguration configuration,
            IOptions<AzureADB2COptions> azureB2COptions,
            IHttpRequestHeaders headers,
            IOptions<WillowContextOptions> options
        )
            : base(headers)
        {
            _logger = logger;
            _auth0Service = auth0Service;
            _usersService = usersService;
            _supervisorsService = supervisorsService;
            _azureB2CService = azureB2CService;
            _configuration = configuration;
            _azureB2COptions = azureB2COptions.Value;
            _willowContextOptions = options.Value;

            _customerId = _willowContextOptions.CustomerInstanceConfiguration?.Id ?? Guid.Empty;
        }

        [HttpPost("signIn")]
        [ProducesResponseType(typeof(AuthenticationInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetAuthenticationToken(
            [FromQuery] string token,
            [FromQuery] string authorizationCode,
            [FromQuery] string codeVerifier = null,
            [FromQuery] string redirectUri = null,
            [FromQuery] bool isMobile = false,
            [FromQuery] SignInType signInType = SignInType.SignIn
        )
        {
            var expiresInSeconds = 8 * 60 * 60;
            var refreshToken = string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                // If codeVerifier is included in request, use B2C. If codeVerifier is not included, use Auth0
                if (string.IsNullOrEmpty(codeVerifier))
                {
                    _logger.LogInformation("Signin using Auth0.");
                    var auth0Response = await _auth0Service.GetAccessTokenByAuthCode(
                        authorizationCode,
                        redirectUri,
                        isMobile
                    );

                    if (string.IsNullOrEmpty(auth0Response?.IdToken))
                    {
                        return Forbid();
                    }

                    token = auth0Response.AccessToken;
                    expiresInSeconds = auth0Response.ExpiresIn;
                    refreshToken = auth0Response.RefreshToken;
                }
                else
                {
                    _logger.LogInformation("Signin using B2C.");
                    var b2cResponse = await _azureB2CService.GetAccessTokenByAuthCode(
                        authorizationCode,
                        redirectUri,
                        codeVerifier,
                        signInType
                    );

                    token = b2cResponse.AccessToken;
                    expiresInSeconds = b2cResponse.ExpiresIn;
                    refreshToken = b2cResponse.RefreshToken;
                }
            }

            var result = new AuthenticationInfo
            {
                AccessToken = token,
                ExpiresIn = expiresInSeconds,
                RefreshToken = refreshToken
            };

            await ParseTokenAndSetUserDetails(result);

            return Ok(result);
        }

        [HttpPost("requestNewAccessToken")]
        [ProducesResponseType(typeof(AuthenticationInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> RequestNewAccessToken(
            [FromHeader] string refreshToken,
            [FromQuery] bool isMobile = false,
            [FromQuery] AuthProvider authProvider = AuthProvider.AzureB2C
        )
        {
            int expiresInSeconds;
            string token;

            if (authProvider == AuthProvider.Auth0)
            {
                var auth0Response = await _auth0Service.GetNewAccessToken(refreshToken, isMobile);

                if (string.IsNullOrEmpty(auth0Response?.IdToken))
                {
                    return Forbid();
                }

                token = auth0Response.AccessToken;
                expiresInSeconds = auth0Response.ExpiresIn;
                refreshToken = auth0Response.RefreshToken;
            }
            else
            {
                var b2cResponse = await _azureB2CService.GetNewAccessToken(refreshToken);

                token = b2cResponse.AccessToken;
                expiresInSeconds = b2cResponse.ExpiresIn;
                refreshToken = b2cResponse.RefreshToken;
            }

            var result = new AuthenticationInfo
            {
                AccessToken = token,
                ExpiresIn = expiresInSeconds,
                RefreshToken = refreshToken
            };

            await ParseTokenAndSetUserDetails(result);

            return Ok(result);
        }

        [HttpPost("users/{userEmail}/initialize")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> InitializeUser(
            string userEmail,
            InitializeUserRequest initializeUserRequest
        )
        {
            await _usersService.InitializeUser(userEmail, initializeUserRequest);
            return NoContent();
        }

        [Authorize]
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(UserDto), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUser(Guid userId, [FromQuery] UserType? userType)
        {
            var user = await _usersService.GetUser(userId, userType ?? UserType.Customer);
            if (user == null)
            {
                throw new ResourceNotFoundException("user", userId);
            }

            return Ok(UserDto.MapFrom(user));
        }

        [HttpPost("users/{userEmail}/password/reset")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ResetPassword(string userEmail)
        {
            await _usersService.SendResetPasswordEmail(userEmail, Language);
            return NoContent();
        }

        [HttpGet("initializeUserTokens/{token}")]
        [ProducesResponseType(typeof(InitializeUserTokenDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetInitializeUserToken([FromRoute] string token)
        {
            var email = await _usersService.GetUserEmailByToken(token);
            return Ok(new InitializeUserTokenDto { Email = email });
        }

        [HttpGet("resetPasswordTokens/{token}")]
        [ProducesResponseType(typeof(ResetPasswordTokenDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetResetPasswordToken([FromRoute] string token)
        {
            var email = await _usersService.GetUserEmailByToken(token);
            return Ok(new ResetPasswordTokenDto { Email = email });
        }

        [HttpPut("users/{userEmail}/password")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ChangeUserPassword(
            string userEmail,
            ChangePasswordRequest changePasswordRequest
        )
        {
            await _usersService.ChangePassword(
                userEmail,
                changePasswordRequest.EmailToken,
                changePasswordRequest.Password
            );
            return NoContent();
        }

        [Authorize]
        [HttpGet("sites/{siteId}/users")]
        [ProducesResponseType(typeof(IList<UserDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSiteUsers(Guid siteId)
        {
            var siteUsers = await _usersService.GetSiteUsers(siteId);
            return Ok(UserDto.MapFrom(siteUsers));
        }

        [Authorize]
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _usersService.GetRoles();
            return Ok(RoleDto.MapFrom(roles));
        }

        [Authorize]
        [HttpGet("portfolios/{portfolioId}/users")]
        [ProducesResponseType(typeof(IList<UserDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPortfolioUsers(Guid portfolioId)
        {
            var users = await _usersService.GetPortfolioUsers(portfolioId);
            return Ok(UserDto.MapFrom(users));
        }

        [Authorize]
        [HttpPost("users/fullNames")]
        [ProducesResponseType(typeof(List<FullNameDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get users full names using User Ids", Tags = new[] { "Users" })]
        public async Task<IActionResult> GetUsersFullNamesByUserIds(List<Guid> userIds)
        {
            var users = await _usersService.GetFullNamesByUserIdsAync(userIds);
            return Ok(FullNameDto.MapFromUsers(users));
        }

        /// <summary>
        /// Get users profile data using user Ids or user emails
        /// </summary>
        /// <param name="getUserProfileRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("users/profiles")]
        [ProducesResponseType(typeof(List<UserProfileDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(
            "Get users profile data using user Ids or user emails",
            Tags = new[] { "Users" }
        )]
        public async Task<IActionResult> GetUsersProfiles(
            GetUsersProfilesRequest getUserProfileRequest
        )
        {
            getUserProfileRequest.Ids ??= [];
            getUserProfileRequest.Emails ??= [];

            var users = await _usersService.GetUsersProfilesAsync(getUserProfileRequest);
            var userProfileDtos = UserProfileDto.MapFrom(users);
            return Ok(userProfileDtos);
        }

        /// <summary>
        /// Retrieves the details of a user based on their user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user details.</returns>
        [Authorize]
        [HttpGet("users/{userId}/userDetails")]
        [ProducesResponseType(typeof(UserDetailsDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(UserDetailsDto), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Get users details", Tags = new[] { "Users" })]
        public async Task<IActionResult> GetUserDetails([FromRoute] Guid userId)
        {
            var userDetails = await _usersService.GetUserDetailsAsync(userId);
            return Ok(userDetails);
        }

        #region private methods

        /// <summary>
        /// If the user has no assignments to the customer identified
        /// in the customer environment variable, regard it as not being a valid user.
        /// in that case, do not set authenticationInfo.anything. The login for this User Id falls through - login for another User Id may succeed.
        /// </summary>
        /// <param name="authenticationInfo">Authentication Information</param>
        /// <returns>Task</returns>
        private async Task ParseTokenAndSetUserDetails(AuthenticationInfo authenticationInfo)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(authenticationInfo.AccessToken);

                // Azure B2C flow
                if (
                    !string.IsNullOrEmpty(jwtToken.Issuer)
                    && jwtToken.Issuer.StartsWith(_azureB2COptions.Instance)
                )
                {
                    var userEmail = jwtToken
                        .Payload.Claims.FirstOrDefault(
                            x => x.Type == ClaimTypes.Email || x.Type == "email"
                        )
                        ?.Value;

                    _logger.Audit(userEmail, "B2C login");

                    // Role information is not stored in b2c, need to check users and supervisors
                    var customerUser = await _usersService.GetUserByEmailAddress(userEmail);
                    if (customerUser == null)
                    {
                        var supervisorUser = await _supervisorsService.GetSupervisorByEmailAddress(
                            userEmail
                        );

                        if (supervisorUser == null)
                        {
                            _logger.LogWarning(
                                "A user does not exist for '{UserEmail}'. User data from Azure B2C: {B2cUserData}",
                                userEmail,
                                JsonConvert.SerializeObject(jwtToken.Payload)
                            );
                        }
                        else
                        {
                            authenticationInfo.UserType = UserTypeNames.Supervisor;
                            authenticationInfo.Supervisor = SupervisorDto.MapFrom(supervisorUser);
                        }
                    }
                    else
                    {
                        if (!IsMatchingCustomer(customerUser))
                        {
                            return;
                        }

                        authenticationInfo.UserType = UserTypeNames.CustomerUser;
                        authenticationInfo.CustomerUser = UserDto.MapFrom(customerUser);
                    }
                }
                // Auth0 flow
                else
                {
                    var auth0UserId = jwtToken.Payload.Sub;
                    var userType = jwtToken
                        .Payload.Claims.FirstOrDefault(
                            x => x.Type == ClaimTypes.Role || x.Type == "role"
                        )
                        ?.Value;

                    _logger.Audit(auth0UserId, "{userType} auth0 login", userType);

                    // Backward compatible: the auth0 users created at the beginning do not have Role information in JWT.
                    if (string.IsNullOrEmpty(userType))
                    {
                        userType = UserTypeNames.CustomerUser;
                    }

                    authenticationInfo.UserType = userType;
                    if (userType == UserTypeNames.CustomerUser)
                    {
                        var customerUser = await _usersService.GetUserByAuth0Id(auth0UserId);
                        if (customerUser == null)
                        {
                            _logger.LogWarning(
                                "A customer user does not exist for '{Auth0UserId}'. User data from Auth0: {Auth0UserData}",
                                auth0UserId,
                                JsonConvert.SerializeObject(jwtToken.Payload)
                            );
                        }
                        else
                        {
                            if (!IsMatchingCustomer(customerUser))
                            {
                                return;
                            }

                            authenticationInfo.CustomerUser = UserDto.MapFrom(customerUser);
                        }
                    }
                    else if (userType == UserTypeNames.Supervisor)
                    {
                        var supervisor = await _supervisorsService.GetSupervisorByAuth0Id(
                            auth0UserId
                        );
                        if (supervisor == null)
                        {
                            _logger.LogWarning(
                                "A supervisor does not exist for '{Auth0UserId}'. User data from Auth0: {Auth0UserData}",
                                auth0UserId,
                                JsonConvert.SerializeObject(jwtToken.Payload)
                            );
                        }
                        else
                        {
                            authenticationInfo.Supervisor = SupervisorDto.MapFrom(supervisor);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "The auth0 user {Auth0UserId} has an unknown role {Role}.'",
                            auth0UserId,
                            userType
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred processing access token");
                throw;
            }
        }

        /// <summary>
        /// Checks that the user customer ID matches the configured customer ID, if there is a configured customer ID
        /// </summary>
        /// <param name="user">LoggedIn User</param>
        /// <returns>True if the user customer ID matches the configured customer ID or the customer ID is not configured</returns>
        private bool IsMatchingCustomer(User user)
        {
            // CustomerId is not set in the configuration
            if (_customerId == Guid.Empty)
            {
                return true;
            }

            // CustomerId is match - valid user
            if (user.CustomerId == _customerId)
            {
                return true;
            }
            else
            {
                // Invalid so log warning
                _logger.LogWarning(
                    "The CustomerId {CustomerID} for '{UserEmail} does not match the configured customer ID {ConfiguredCustomerId}.",
                    user.CustomerId,
                    user.Email,
                    _customerId
                );

                return false;
            }
        }
        #endregion
    }
}
