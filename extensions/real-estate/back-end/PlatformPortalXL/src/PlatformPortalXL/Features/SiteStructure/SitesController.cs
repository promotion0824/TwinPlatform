using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Forge;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Batch;
using Willow.Common;
using Willow.Data;
using Willow.Workflow;

namespace PlatformPortalXL.Features.SiteStructure
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SitesController : SitesBaseController
    {
        private readonly IAccessControlService _accessControl;
        private readonly IForgeService _forgeService;
        private readonly ISiteService _siteService;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IStaleCache _staleCache;

        public SitesController(
            ISiteService siteService,
            IDirectoryApiService directoryApiService,
            IWorkflowApiService workflowApiService,
            IPortfolioDashboardService portfolioDashboardService,
            ITimeZoneService timeZoneService,
            IImageUrlHelper imageUrlHelper,
            IAccessControlService accessControl,
            ISiteApiService siteApiService,
            IForgeService forgeService,
            IDigitalTwinApiService digitalTwinApiService,
            IStaleCache staleCache)
            : base(siteApiService, directoryApiService, workflowApiService, portfolioDashboardService, timeZoneService, imageUrlHelper)
        {
            _accessControl = accessControl;
            _forgeService = forgeService;
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _digitalTwinApiService = digitalTwinApiService;
            _staleCache = staleCache;
        }

        /// <summary>
        /// Gets a list of sites which the signed-in user can access.
        /// </summary>
        /// <param name="includeStatsByStatus">Whether to include the ticket and insight statistics by status. This is
        /// a temporary parameter to help with debugging a resource issue.</param>
        /// <param name="includeWeather">Whether to include weather data for each site. This is a temporary parameter
        /// to help with debugging a resource issue.</param>
        /// <param name="includeDtIds">Whether to include the digital twin ids for each site.</param>
        /// <returns><see cref="SiteDetailDto"/></returns>

        [HttpGet("me/sites")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteDetailDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of sites which the signed-in user can access", Tags = new [] { "Users" })]
        public async Task<ActionResult> GetSites(
            [FromQuery] bool includeStatsByStatus = false,
            [FromQuery] bool includeWeather = false,
            [FromQuery] bool includeDtIds = false)
        {
            var currentUserId = this.GetCurrentUserId();
            var currentUserEmail = this.GetUserEmail();

            List<SiteDetailDto> sites;

            if (string.IsNullOrWhiteSpace(currentUserEmail))
            {
                sites = await _siteService.GetSites(currentUserId, includeWeather, includeDtIds);
            }
            else
            {
                sites = await _staleCache.GetOrCreateAsync(
                    $"SitesController-GetSites-{currentUserEmail}-{includeWeather}",
                    async () => await _siteService.GetSites(currentUserId, includeWeather, includeDtIds),
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return Ok(sites);
        }

        [MapToApiVersion("2.0")]
        [HttpPost("v{version:apiVersion}/me/sites")]
        [Authorize]
        [ProducesResponseType(typeof(BatchDto<SiteMiniDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of sites with minimal set of details which the signed-in user can access", Tags = new[] { "Users" })]
        public async Task<ActionResult> GetSitesV2([FromBody] BatchSitesRequest request)
        {
            var userId = this.GetCurrentUserId();
            var siteMiniDtos = await _siteService.GetSitesV2(userId, request);

            return Ok(siteMiniDtos);
        }

        [HttpGet("sites/{siteId}")]
        [Authorize]
        [ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the site with the given id which the signed-in user can access", Tags = new [] { "Sites" })]
        public async Task<ActionResult> GetSite([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var site = await _siteApiService.GetSite(siteId);
            var customer = await _directoryApiService.GetCustomer(site.CustomerId);
            SiteDetailDto siteDto = await MapToSiteDetailAsync(site, customer.Features.IsConnectivityViewEnabled);
            return Ok(siteDto);
        }

        [HttpDelete("sites/{siteId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Soft-deletes a site", Tags = new[] { "Sites" })]
        public async Task<ActionResult> DeleteSite([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            await _siteApiService.DeleteSite(siteId);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/logo")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Updates site image", Tags = new[] { "Sites" })]
        public async Task<IActionResult> UpdateSiteLogo([FromRoute]Guid siteId, IFormFile logoImage)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            byte[] logoImageBytes;
            using (var memoryStream = new MemoryStream())
            {
                logoImage.OpenReadStream().CopyTo(memoryStream);
                logoImageBytes = memoryStream.ToArray();
            }
            var site = await _siteApiService.UpdateSiteLogo(siteId, logoImageBytes, logoImage.FileName);

            var siteDto = SiteDetailDto.Map(site, _imageUrlHelper);
            return Ok(siteDto);
        }

        [HttpGet("sites/{siteId}/preferences/timeMachine")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Get time machine preferences", Tags = new [] { "Sites" })]
        public async Task<IActionResult> GetTimeMachinePreferences([FromRoute]Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var sitePreferences = await _siteApiService.GetSitePreferences(siteId);
            return Ok(sitePreferences.TimeMachine);
        }

        [HttpGet("sites/{siteId}/preferences/moduleGroups")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Get Module Groups preferences", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetModuleGroupsPreferences([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var sitePreferences = await _siteApiService.GetSitePreferences(siteId);
            return Ok(sitePreferences.ModuleGroups);
        }

        [HttpPut("sites/{siteId}/preferences/timeMachine")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Create or update time machine preferences", Tags = new [] { "Sites" })]
        public async Task<IActionResult> CreateOrUpdateTimeMachine([FromRoute]Guid siteId, [FromBody] JsonElement timeMachinePreference)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);
            await _siteApiService.CreateOrUpdateTimeMachinePreferences(siteId, new TimeMachinePreferencesRequest { TimeMachine = timeMachinePreference });
            return NoContent();
        }

        [HttpPut("sites/{siteId}/preferences/moduleGroups")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Create or update module groups preferences", Tags = new[] { "Sites" })]
        public async Task<IActionResult> CreateOrUpdateModuleGroups([FromRoute] Guid siteId, [FromBody] JsonElement moduleGroupsPreferences)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);
            await _siteApiService.CreateOrUpdateModuleGroupsPreferences(siteId, new ModuleGroupsPreferencesRequest { ModuleGroups = moduleGroupsPreferences });
            return NoContent();
        }

        [HttpGet("sites/{siteId}/module")]
        [Authorize]
        [ProducesResponseType(typeof(LayerGroupModuleDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the 3d module for a site", Tags = new[] { "Sites" })]
        public async Task<ActionResult<LayerGroupModuleDto>> GetSiteModule3DAsync([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var module = await _siteApiService.GetModule3DAsync(siteId);
            var moduleDto = LayerGroupModuleDto.MapFrom(module);
            return Ok(moduleDto);
        }

        [HttpPost("sites/{siteId}/module")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(LayerGroupModuleDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Uploads a 3d module for a site", Tags = new[] { "Sites" })]
        public async Task<ActionResult> UploadSiteModule3DAsync([FromRoute] Guid siteId, IFormFile file)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            if (file == null)
            {
                throw new ArgumentNullException("Multipart formdata file expected").WithData(new { SiteId = siteId });
            }

            try
            {
                var uploadInfo = await _forgeService.StartConvertToSvfAsync(siteId, new List<IFormFile>() { file });

                var request = new CreateUpdateModule3DRequest
                {
                    Modules3D = uploadInfo.Select(i => new Module3DInfo { Url = i.ForgeInfo.Urn, ModuleName = i.File.FileName }).ToList()
                };

                var validationError = await _siteApiService.UploadModule3DAsync(siteId, request);
                if (validationError != null)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
                }

                await _siteService.Broadcast(siteId, uploadInfo.Select(x => x.ForgeInfo.Urn).ToList());

                return NoContent();
            }
            catch (Autodesk.Forge.Client.ApiException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StatusCodes.Status409Conflict:
                        return StatusCode(StatusCodes.Status429TooManyRequests);
                    default:
                        throw;
                }
            }
        }

        [HttpDelete("sites/{siteId}/module")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Gets the 3d module of a site", Tags = new[] { "Sites" })]
        public async Task<ActionResult> DeleteSiteModule3DAsync([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            await _siteApiService.DeleteModule3DAsync(siteId);
            return NoContent();
        }

        [HttpPost("sites")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Creates a new ADT site", Tags = new[] { "Sites" })]
        public async Task<ActionResult> CreateNewAdtSite([FromBody] NewAdtSiteRequest request)
        {
            var createdSiteUrl = await _digitalTwinApiService.NewAdtSite(request);

            return Ok(createdSiteUrl);
        }

        [HttpGet("scopes/{scopeId}/preferences/timeMachine")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Get time machine preferences by scope", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetTimeMachinePreferencesByScope([FromRoute] string scopeId)
        {
            var userId = this.GetCurrentUserId();
            await _siteService.CheckScopePermission(userId, Permissions.ViewSites, scopeId);
            var scopePreferences = await _siteApiService.GetSitePreferencesByScope(scopeId);
            return Ok(scopePreferences.TimeMachine);
        }

        [HttpPut("scopes/{scopeId}/preferences/timeMachine")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Create or update time machine preferences by scope", Tags = new[] { "Sites" })]
        public async Task<IActionResult> CreateOrUpdateTimeMachineByScope([FromRoute] string scopeId, [FromBody] JsonElement timeMachinePreference)
        {
            var userId = this.GetCurrentUserId();
            await _siteService.CheckScopePermission(userId, Permissions.ManageSites, scopeId);
            await _siteApiService.CreateOrUpdateTimeMachinePreferencesByScope(scopeId, new TimeMachinePreferencesRequest { TimeMachine = timeMachinePreference });
            return NoContent();
        }
    }
}
