using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;
using SiteCore.Services.ImageHub;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TimeZoneConverter;
using Willow.Common;

namespace SiteCore.Controllers
{
	[ApiController]
	[ApiConventionType(typeof(DefaultApiConventions))]
	[Produces("application/json")]
	public class SitesController : ControllerBase
	{
		private readonly ISiteService _siteService;
		private readonly IFloorService _floorService;
		private readonly IImagePathHelper _imagePathHelper;
		private readonly IModuleTypesService _moduleTypesService;
		private readonly IModulesService _modulesService;
        private readonly ISiteExtendService _siteExtendService;

		public SitesController(
			ISiteService siteService,
			IFloorService floorService,
			IImagePathHelper imagePathHelper,
			IModuleTypesService moduleTypesService,
			IModulesService modulesService,
			ISiteExtendService siteExtendService)
		{
			_siteService = siteService;
			_floorService = floorService;
			_imagePathHelper = imagePathHelper;
			_moduleTypesService = moduleTypesService;
			_modulesService = modulesService;
            _siteExtendService = siteExtendService;
		}
        [Obsolete("This endpoint is failing when we have many sites, Please use the post endpoint.")]
		[HttpGet("sites")]
		[Authorize]
		[ProducesResponseType(typeof(List<SiteSimpleDto>), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Gets all sites")]
		public async Task<IActionResult> GetAllSites([FromQuery] Guid[] siteIds)
		{
			List<Site> sites;
			if (siteIds == null || siteIds.Length <= 0)
			{
				sites = await _siteService.GetAllSites();
			}
			else
			{
				sites = new List<Site>();
				foreach (var siteId in siteIds)
				{
					var site =  await _siteService.GetSite(siteId);
					sites.Add(site);
				}
			}

			return Ok(SiteSimpleDto.MapFrom(sites, _imagePathHelper));
		}

        [HttpPost("sites")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteSimpleDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets all sites")]
        public async Task<IActionResult> GetAllSitesByIdsAsync([FromBody] List<Guid> siteIds)
        {
            List<SiteSimpleDto> sites = new List<SiteSimpleDto>();
            if (siteIds != null && siteIds.Any())
            {
                var sitesByIds = await _siteService.GetAllSitesByIdsAsync(siteIds);
                sites = SiteSimpleDto.MapFrom(sitesByIds, _imagePathHelper);
            }

            return Ok(sites);
        }

        [HttpGet("customers/{customerId}/sites")]
		[Authorize]
		[ProducesResponseType(typeof(List<SiteDetailDto>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[SwaggerOperation("Gets a list of sites for the given customer and/or portfolio")]
		public async  Task<IActionResult> GetSites([FromRoute] Guid customerId, Guid? portfolioId = null)
		{
			var sites = await _siteService.GetSites(customerId, portfolioId);
			return Ok(SiteDetailDto.MapFrom(sites, _imagePathHelper));
		}

		[HttpPost("customers/{customerId}/portfolios/{portfolioId}/sites")]
		[Authorize]
		[ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[SwaggerOperation("Creates a new site")]
		public async Task<IActionResult> CreateSite(
			[FromRoute] Guid customerId,
			[FromRoute] Guid portfolioId,
			[FromBody] CreateSiteRequest request)
		{
			if (request.FloorCodes is not { Count: > 0 })
			{
				throw new ArgumentException("Missing floor codes").WithData(new { CustomreId = customerId, PortfolioId = portfolioId });
			}

			if (request.FloorCodes.GroupBy(fc => fc).Any(g => g.Count() > 1))
			{
				throw new ArgumentException("Floor codes must be unique.").WithData(new { CustomreId = customerId, PortfolioId = portfolioId, request.FloorCodes });
			}

			try
			{
				TZConvert.GetTimeZoneInfo(request.TimeZoneId);
			}
			catch (TimeZoneNotFoundException)
			{
				throw new ArgumentException($"Unknown timezone id {request.TimeZoneId}").WithData(new { CustomreId = customerId, PortfolioId = portfolioId, request.TimeZoneId });
			}

			var site = await _siteService.CreateSite(customerId, portfolioId, request);
			await _floorService.InitializeSiteFloors(site.Id, request.FloorCodes);
			await _moduleTypesService.CreateDefaultModuleTypesAsync(site.Id);

			return Ok(SiteDetailDto.MapFrom(site, _imagePathHelper));
		}

		[HttpPut("customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}")]
		[Authorize]
		[ProducesResponseType(typeof(SiteDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[SwaggerOperation("Update site")]
		public async Task<IActionResult> UpdateSite(
			[FromRoute] Guid customerId,
			[FromRoute] Guid portfolioId,
			[FromRoute] Guid siteId,
			[FromBody] UpdateSiteRequest request)
		{
			try
			{
				TZConvert.GetTimeZoneInfo(request.TimeZoneId);
			}
			catch (TimeZoneNotFoundException)
			{
				throw new ArgumentException($"Unknown timezone id {request.TimeZoneId}").WithData(new { CustomerId = customerId, PortflolioId = portfolioId, siteId = siteId, TimeZoneId = request.TimeZoneId });
			}

			var site = await _siteService.UpdateSite(customerId, portfolioId, siteId, request);

			return Ok(SiteDetailDto.MapFrom(site, _imagePathHelper));
		}

		[HttpGet("sites/{siteId}")]
		[Authorize]
		[ProducesResponseType(typeof(SiteDetailDto), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[SwaggerOperation("Gets a site by id")]
		public async Task<IActionResult> GetSite([FromRoute] Guid siteId)
		{
			var site = await _siteService.GetSite(siteId);
			return Ok(SiteDetailDto.MapFrom(site, _imagePathHelper));
		}

		[HttpDelete("sites/{siteId}")]
		[Authorize]
		[ProducesResponseType((int) HttpStatusCode.NoContent)]
		[SwaggerOperation("Soft-deletes a site")]
		public async Task<IActionResult> DeleteSite([FromRoute] Guid siteId)
		{
			await _siteService.SoftDeleteSite(siteId);
			return Ok();
		}

		[Authorize]
		[HttpPut("sites/{siteId}/logo")]
		[ProducesResponseType(typeof(SiteDetailDto), (int)HttpStatusCode.OK)]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> UpdateSiteLogo([FromRoute] Guid siteId, IFormFile logoImage)
		{
			byte[] logoImageContent;
			using (var memoryStream = new MemoryStream())
			{
				logoImage.OpenReadStream().CopyTo(memoryStream);
				logoImageContent = memoryStream.ToArray();
			}
			var site = await _siteService.UpdateSiteLogo(siteId, logoImageContent);
			return Ok(SiteDetailDto.MapFrom(site, _imagePathHelper));
		}

		[Authorize]
		[HttpGet("sites/{siteId}/preferences")]
		[ProducesResponseType(typeof(SitePreferences), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Get site preferences")]
		public async Task<IActionResult> GetSitePreferences([FromRoute] Guid siteId)
		{
			var sitePreferences = await _siteService.GetSitePreferences(siteId);
			return Ok(sitePreferences);
		}

		[Authorize]
		[HttpPut("sites/{siteId}/preferences")]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[SwaggerOperation("Create or update site preferences")]
		public async Task<IActionResult> CreateOrUpdateSitePreferences(
			[FromRoute] Guid siteId,
			[FromBody] SitePreferencesRequest sitePreferencesRequest)
		{
			await _siteService.CreateOrUpdateSitePreferences(siteId, sitePreferencesRequest);
			return NoContent();
		}

		[HttpPost("sites/{siteId}/module")]
		[Authorize]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[SwaggerOperation("Uploads a 3d module for a site")]
		public async Task<ActionResult> UploadModule3DAsync(
		[FromRoute] Guid siteId,
		[FromBody] CreateUpdateModule3DRequest request)
		{
			var floor = await _floorService.GetFloorBySiteId(siteId);
			await _modulesService.DeleteModulesBySiteAsync(siteId);
			await _floorService.Upload3DFloorModules(siteId, floor.Id, request);
			return NoContent();
		}

		[HttpDelete("sites/{siteId}/module")]
		[Authorize]
		[ProducesResponseType((int)HttpStatusCode.NoContent)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[SwaggerOperation("Deletes the 3d module for a site")]
		public async Task<ActionResult> DeleteModule3DAsync([FromRoute] Guid siteId)
		{
			await _modulesService.DeleteModulesBySiteAsync(siteId);
			return NoContent();
		}

		[HttpGet("sites/{siteId}/module")]
		[Authorize]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[SwaggerOperation("Returns the 3d module for a site")]
		public async Task<ActionResult<LayerGroupModuleDto>> GetModule3DAsync([FromRoute] Guid siteId)
		{
			var modules = await _modulesService.GetModulesBySiteAsync(siteId);
			return LayerGroupModuleDto.MapFrom(modules.FirstOrDefault());
		}

		[HttpGet("sites/extend")]
		[Authorize]
		[ProducesResponseType(typeof(List<Site>), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Gets all sites")]
		public async Task<IActionResult> GetSites([FromQuery] bool? isInspectionEnabled = null,
			[FromQuery] IEnumerable<Guid> siteIds = null)
		{
			return Ok(await _siteExtendService.GetSites(isInspectionEnabled, siteIds));
		}

		[HttpGet("sites/{siteId}/extend")]
		[Authorize]
		[ProducesResponseType(typeof(Site), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[SwaggerOperation("Gets a site by id")]
		public async Task<IActionResult> GetSiteExtend([FromRoute] Guid siteId)
		{
			return Ok(await _siteExtendService.GetSite(siteId));
		}

		[HttpGet("customers/{customerId}/portfolios/{portfolioId}/sites/extend")]
		[Authorize]
		[ProducesResponseType(typeof(List<Site>), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Gets a list of sites for the given customer and portfolio")]
		public async Task<IActionResult> GetPortfolioSites([FromRoute] Guid customerId, [FromRoute] Guid portfolioId)
		{
			return Ok(await _siteExtendService.GetPortfolioSites(customerId, portfolioId));
		}

		[HttpPost("customers/{customerId}/portfolios/{portfolioId}/sites/extend")]
		[Authorize]
		[ProducesResponseType(typeof(Site), StatusCodes.Status200OK)]
		[SwaggerOperation("Creates a new site")]
		public async Task<IActionResult> CreateSiteExtend(
			[FromRoute] Guid customerId,
			[FromRoute] Guid portfolioId,
			[FromBody] CreateSiteRequest request)
		{
			return Ok(await _siteExtendService.CreateSite(customerId, portfolioId, request));
		}

		[HttpPut("customers/{customerId}/portfolios/{portfolioId}/sites/{siteId}/extend")]
		[Authorize]
		[ProducesResponseType(typeof(Site), StatusCodes.Status200OK)]
		[SwaggerOperation("Update site")]
		public async Task<IActionResult> UpdateSiteExtend(
			[FromRoute] Guid customerId,
			[FromRoute] Guid portfolioId,
			[FromRoute] Guid siteId,
			[FromBody] UpdateSiteRequest request)
		{
			await _siteExtendService.UpdateSite(customerId, portfolioId, siteId, request);
			return NoContent();
		}

		[HttpGet("sites/customer/{customerId}/extend")]
		[Authorize]
		[ProducesResponseType(typeof(List<Site>), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Gets all sites")]
		public async Task<IActionResult> GetSitesByCustomer(Guid customerId, [FromQuery] bool? isInspectionEnabled = null, [FromQuery] bool? isTicketingDisabled = null, [FromQuery] bool? isScheduledTicketsEnabled = null)
		{
			return Ok(await _siteExtendService.GetSitesByCustomer(customerId, isInspectionEnabled, isTicketingDisabled, isScheduledTicketsEnabled));
		}

		[HttpPut("sites/{siteId}/features")]
		[Authorize]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[SwaggerOperation("Update site features")]
		public async Task<IActionResult> UpdateSiteFeatures([FromRoute] Guid siteId, [FromBody] SiteFeatures siteFeatures)
		{
			await _siteExtendService.UpdateSiteFeatures(siteId, siteFeatures);
			return NoContent();
		}

        [Authorize]
        [HttpGet("scopes/{scopeId}/preferences")]
        [ProducesResponseType(typeof(SitePreferences), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get site preferences by scope")]
        public async Task<IActionResult> GetSitePreferencesByScope([FromRoute] string scopeId)
        {
            var sitePreferences = await _siteService.GetSitePreferencesByScope(scopeId);
            return Ok(sitePreferences);
        }

        [Authorize]
        [HttpPut("scopes/{scopeId}/preferences")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Create or update site preferences by scope")]
        public async Task<IActionResult> CreateOrUpdateSitePreferencesByScope(
            [FromRoute] string scopeId,
            [FromBody] SitePreferencesRequest sitePreferencesRequest)
        {
            await _siteService.CreateOrUpdateSitePreferencesByScope(scopeId, sitePreferencesRequest);
            return NoContent();
        }
    }
}
