using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Willow.Common;

using PlatformPortalXL.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Http;
using PlatformPortalXL.Services.MarketPlaceApi;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;

using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Helpers;
using Willow.ExceptionHandling.Exceptions;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.MarketPlace
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AppsController : TranslationController
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMarketPlaceApiService _marketPlaceApi;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly IAppManagementService _appManagementService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;
        private readonly INotificationService _notificationService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public AppsController(
            IAccessControlService accessControl,
            IDateTimeService dateTimeService,
            IMarketPlaceApiService coreApi,
            IImageUrlHelper imageUrlHelper,
            IAppManagementService appManagementService,
            IDirectoryApiService directoryApi,
            ISiteApiService siteApi,
            INotificationService notificationService,
            IUserAuthorizedSitesService userAuthorizedSitesService,
            IHttpRequestHeaders headers
            )
             : base(headers)
        {
            _accessControl = accessControl;
            _dateTimeService = dateTimeService;
            _marketPlaceApi = coreApi;
            _imageUrlHelper = imageUrlHelper;
            _appManagementService = appManagementService;
            _directoryApi = directoryApi;
            _siteApi = siteApi;
            _notificationService = notificationService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
        }

        [HttpGet("me/sites/commonApps")]
        [Authorize]
        [ProducesResponseType(typeof(List<AppDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets apps that are installed at portfolio level.", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> GetCommonAppsAsync()
        {
            var currentUserId = this.GetCurrentUserId();

            IEnumerable<Guid> commonAppIds = null;

            foreach (var site in await _userAuthorizedSitesService.GetAuthorizedSites(currentUserId, Permissions.ViewApps))
            {
                var installedApps = await _marketPlaceApi.GetInstalledApps(site.Id);
                var installedAppIds = installedApps.Select(x => x.AppId);
                commonAppIds = (commonAppIds ?? installedAppIds).Intersect(installedAppIds);
            }

            if (commonAppIds == null)
            {
                return Ok(new List<AppDto>());
            }

            var apps = await _marketPlaceApi.GetApps();
            var commonApps = apps.Where(x => commonAppIds.Contains(x.Id)).ToList();
            var dtos = AppDto.MapFrom(commonApps, _imageUrlHelper);
            dtos?.ForEach(d => d.IsInstalled = true);
            return Ok(dtos);
        }

        [HttpGet("apps")]
        [Authorize]
        [ProducesResponseType(typeof(List<AppDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets apps. If siteId is provided, app installation information will be returned", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> GetApps([FromQuery] Guid? siteId)
        {
            if (siteId.HasValue)
            {
                await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewApps, siteId.Value);
            }

            var apps = await _marketPlaceApi.GetApps();
            var dtos = AppDto.MapFrom(apps, _imageUrlHelper);
            if (siteId.HasValue)
            {
                var privateApps = AppDto.MapFrom(await _marketPlaceApi.GetPrivateApps(siteId.Value), _imageUrlHelper);
                if (privateApps.Any())
                {
                    dtos.AddRange(privateApps);
                }

                var installedApps = await _marketPlaceApi.GetInstalledApps(siteId.Value);
                foreach (var dto in dtos)
                {
                    dto.IsInstalled = installedApps.Any(x => x.AppId == dto.Id);
                }
            }
            return Ok(dtos);
        }

        [HttpGet("apps/{appId}")]
        [Authorize]
        [ProducesResponseType(typeof(AppDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the app. If siteId is provided, the app installation information will be returned", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> GetApp([FromRoute] Guid appId, [FromQuery] Guid? siteId)
        {
            var currentUserId = this.GetCurrentUserId();
            if (siteId.HasValue)
            {
                await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewApps, siteId.Value);
            }

            var app = await _marketPlaceApi.GetApp(appId, siteId);
            var dto = AppDto.MapFrom(app, _imageUrlHelper);
            if (siteId.HasValue)
            {
                var installedApps = await _marketPlaceApi.GetInstalledApps(siteId.Value);
                dto.IsInstalled = installedApps.Any(x => x.AppId == appId);

                if (dto.IsInstalled && !string.IsNullOrEmpty(dto.Manifest.ConfigurationUrl))
                {
                    var siteIdString = siteId.Value.ToString();
                    var userIdString = currentUserId.ToString();
                    var timestampString = _dateTimeService.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                    var uriBuilder = new UriBuilder(dto.Manifest.ConfigurationUrl);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["siteId"] = siteIdString;
                    query["userId"] = userIdString;
                    query["timestamp"] = timestampString;
                    query["sign"] = await GetSignature(appId, siteIdString, userIdString, timestampString);
                    uriBuilder.Query = query.ToString();
                    dto.Manifest.ConfigurationUrl = uriBuilder.ToString();
                }
            }
            return Ok(dto);
        }

        [HttpPost("sites/{siteId}/installedApps")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Install an app on the specific site", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> InstallApp([FromRoute] Guid siteId, [FromBody] InstallAppRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageApps, siteId);

            await _appManagementService.InstallApp(siteId, request.AppId, this.GetCurrentUserId());
            return NoContent();
        }

        [HttpDelete("sites/{siteId}/installedApps/{appId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Uninstall an app", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> UninstallApp([FromRoute] Guid siteId, [FromRoute] Guid appId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageApps, siteId);

            await _appManagementService.UninstallApp(siteId, appId);
            return NoContent();
        }

        [HttpPost("apps/{appId}/requestActivation")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Send customer service request for app activation", Tags = new[] { "MarketPlace" })]
        public async Task<IActionResult> RequestAppActivationAsync([FromRoute] Guid appId, [FromQuery] Guid? siteId)
        {
            var currentUserId = this.GetCurrentUserId();

            var user = await _directoryApi.GetUser(currentUserId);

            var siteOrPortfolio = "Portfolio";
            if (siteId.HasValue)
            {
                await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewApps, siteId.Value);
                var site = await _siteApi.GetSite(siteId.Value);
                siteOrPortfolio = site.Name;
            }

            var app = await _marketPlaceApi.GetApp(appId);
            if (app == null)
            {
                throw new NotFoundException().WithData(new { appId });
            }

            var templateParameters = new
            {
                AppName = app.Name,
                SiteOrPortfolio = siteOrPortfolio
            };
            await _notificationService.SendNotificationAsync(new Willow.Notifications.Models.Notification
            {
                CorrelationId = Guid.NewGuid(),
                CommunicationType = CommunicationType.Email,
                CustomerId = user.CustomerId,
                Data = templateParameters.ToDictionary(),
                Tags = null,
                TemplateName = "ContactRequest",
                UserId = user.Id,
                UserType = user.Type.ToString(),
                Locale = this.Language
            });

            return NoContent();
        }

        private async Task<string> GetSignature(Guid appId, string siteIdString, string userIdString, string timestampString)
        {
            var payload = new { siteId = siteIdString, userId = userIdString, timestamp = timestampString };
            var signature = await _marketPlaceApi.GetSignature(appId, JsonSerializerHelper.Serialize(payload));
            return signature;
        }
    }
}
