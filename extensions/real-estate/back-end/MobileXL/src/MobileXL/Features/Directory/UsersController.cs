using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MobileXL.Dto;
using MobileXL.Models;
using MobileXL.Options;
using MobileXL.Security;
using MobileXL.Services;
using MobileXL.Services.Apis.DirectoryApi;
using MobileXL.Services.Apis.SiteApi;
using Swashbuckle.AspNetCore.Annotations;

namespace MobileXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;
        private readonly SingleTenantOptions _options;

        public UsersController(
            ILogger<UsersController> logger,
            ITimeZoneService timeZoneService,
            IAccessControlService accessControl,
            IDirectoryApiService directoryApi,
            ISiteApiService siteApi,
            IOptions<SingleTenantOptions> options)
        {
            _logger = logger;
            _timeZoneService = timeZoneService;
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _siteApi = siteApi;
            _options = options.Value;
        }

        [HttpGet("me")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(MeDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the information of current signed-in user", Tags = new[] { "Users" })]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userId = this.GetCurrentUserId();
            var sitesList = new List<Site>();

            var customerUser = await _directoryApi.GetCustomerUser(userId);
            var customer = await _directoryApi.GetCustomer(customerUser.CustomerId);
            var customerUserPreferences = await _directoryApi.GetCustomerUserPreferences(customerUser.CustomerId, customerUser.Id);
            var allSites = await _siteApi.GetSites(customerUser.CustomerId);
            foreach (var site in allSites)
            {
                var canAccess = await _accessControl.CanCustomerUserAccessSite(customerUser.Id, Permissions.ViewSites, site.Id);
                if (canAccess)
                {
                    sitesList.Add(site);
                }
            }

            var me = new MeDto
            {
                Id = customerUser.Id,
                FirstName = customerUser.FirstName,
                LastName = customerUser.LastName,
                Initials = customerUser.Initials,
                Email = customerUser.Email,
                Company = customerUser.Company,
                Preferences = customerUserPreferences,
                AccountExternalId = customer.AccountExternalId,
                CustomerId = customerUser.CustomerId,
                Customer = customer
            };
            // If user is a group user, then user the email and name from b2c
            SetGroupUserNameAndEmail(me);


            me.Sites = new List<SiteSimpleDto>();
            foreach (var site in sitesList)
            {
                var siteFeatures = await _directoryApi.GetSiteFeatures(site.Id);
                if (siteFeatures.IsTicketingDisabled && (!siteFeatures.IsInspectionEnabled))
                {
                    continue;
                }
                var siteDto = SiteSimpleDto.MapFrom(site);
                siteDto.Features = SiteFeaturesDto.Map(siteFeatures);
                siteDto.TimeZone = _timeZoneService.GetTimeZoneType(site.TimeZoneId);
                me.Sites.Add(siteDto);
            }
            return Ok(me);
        }

        [HttpPost("users/{userEmail}/password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Resets password", Tags = new [] { "Users" })]
        public async Task<ActionResult> ResetPassword([FromRoute] string userEmail)
        {
            var account = await _directoryApi.GetAccount(userEmail);

            if (account.UserType == UserTypeNames.CustomerUser)
            {
                await _directoryApi.ResetCustomerUserPassword(userEmail);
            }
            else
            {
                var ex = new ArgumentException("Failed to reset password. Unknown user type");

                ex.Data.Add("Email", userEmail);
                ex.Data.Add("UserType", account.UserType);

                throw ex;
            }
            return NoContent();
        }

        [HttpGet("userResetPasswordTokens/{token}")]
        [ProducesResponseType(typeof(ResetPasswordToken), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the information associated with the given reset-password token", Tags = new[] { "Users" })]
        public async Task<ActionResult> GetResetPasswordToken([FromRoute] string token)
        {
            var tokenInfo = await _directoryApi.GetCustomerUserResetPasswordToken(token);
            return Ok(tokenInfo);
        }

        [HttpPut("users/{userEmail}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the password", Tags = new [] { "Users" })]
        public async Task<ActionResult> UpdatePassword([FromRoute] string userEmail, [FromBody] UpdatePasswordRequest request)
        {
            var account = await _directoryApi.GetAccount(userEmail);
            
            if (account.UserType == UserTypeNames.CustomerUser)
            {
                await _directoryApi.UpdateCustomerUserPassword(userEmail, request.Password, request.Token);
            }
            else
            {
                var ex = new ArgumentException("Failed to update password. Unknown user type");

                ex.Data.Add("Email", userEmail);
                ex.Data.Add("UserType", account.UserType);

                throw ex;
            }
            return NoContent();
        }

        [MobileAuthorize]
        [HttpPut("me/preferences")]
        [ProducesResponseType(typeof(CustomerUserPreferences), StatusCodes.Status204NoContent)]
        [SwaggerOperation("Create or update Customer User preferences", Tags = new [] { "Users" })]
        public async Task<IActionResult> CreateOrUpdateCustomerUserPreferences([FromBody] CustomerUserPreferencesRequest customerUserPreferencesRequest)
        {
            var userId = this.GetCurrentUserId();
            var customerUser = await _directoryApi.GetCustomerUser(userId);
            await _directoryApi.CreateOrUpdateCustomerUserPreferences(customerUser.Id, userId, customerUserPreferencesRequest);
            return NoContent();
        }

        /// <summary>
        /// Sets the email address and user's first/last name from the B2C claims
        /// </summary>
        private void SetGroupUserNameAndEmail(MeDto meDto)
        {
            if (_options.UseSingleTenant && meDto.Id == new Guid(_options.CustomerUserIdForGroupUser))
            {
                meDto.Email = User.FindFirst(x => x.Type == ClaimTypes.Email)?.Value;
                // Can only get the full name from the b2c claim, so need to retrieve the first and last name from it
                // And in some cases, the name could only have one word, this is counted as last name and first name will be empty
                var names = User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value?.Split(' ');
                if ((names?.Length ?? 0) > 0)
                {
                    meDto.LastName = names.Last();
                    meDto.FirstName = string.Join(' ', names.SkipLast(1));
                    meDto.Initials = $"{(meDto.FirstName.Length > 0 ? meDto.FirstName[0] : string.Empty)}{meDto.LastName[0]}";
                }
            }
        }
    }
}
