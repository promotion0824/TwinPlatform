using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.ArcGis;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Proxy;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Linq;
using PlatformPortalXL.Filters.ArcGis;

namespace PlatformPortalXL.Features.ArcGis
{
	[ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ArcGisController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
		private readonly IArcGisService _arcGisService;

		public ArcGisController(IAccessControlService accessControl,
			IArcGisService arcGisService)
        {
            _accessControl = accessControl;
            _arcGisService = arcGisService;
        }

        [Authorize]
        [HttpGet("sites/{siteId}/arcGisToken")]
		[ServiceFilter(typeof(CustomerValidationFilter))]
		[ServiceFilter(typeof(ArcGisValidationFilter))]
		[ProducesResponseType(typeof(ArcGisDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ArcGisDto), StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Get ArcGis token", Tags = new[] { "ArcGis" })]
        public async Task<IActionResult> GetToken([FromRoute] Guid siteId, [FromQuery] string referer)
        {
			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

			return Ok(await _arcGisService.GetToken(referer));
		}

		/// <summary>
		/// Proxy request to ArcGis
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="arcGisUrl"></param>
		[Authorize]
		[HttpGet("sites/{siteId}/arcGisProxy")]
		[ServiceFilter(typeof(CustomerValidationFilter))]
		[ServiceFilter(typeof(ArcGisValidationFilter))]
		[SwaggerOperation("Proxy ArcGis request", Tags = new[] { "ArcGis" })]
		public async Task Proxy([FromRoute] Guid siteId, [FromQuery] string arcGisUrl)
		{
			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

			if (string.IsNullOrEmpty(arcGisUrl))
			{
				throw new ArgumentNullException($"Invalid arcGisUrl");
			}

			await this.ProxyToDownstreamService(ApiServiceNames.ArcGis, HttpUtility.UrlEncode(arcGisUrl));
		}

		[Authorize]
		[HttpGet("sites/{siteId}/arcGisLayers")]
		[ServiceFilter(typeof(CustomerValidationFilter))]
		[ServiceFilter(typeof(ArcGisValidationFilter))]
		[ProducesResponseType(typeof(ArcGisLayersDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ArcGisLayersDto), StatusCodes.Status400BadRequest)]
		[SwaggerOperation("Get ArcGis Layers", Tags = new[] { "ArcGis" })]
		public async Task<IActionResult> GetLayers([FromRoute] Guid siteId)
		{
			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

			return Ok(await _arcGisService.GetArcGisLayers());
		}

		[Authorize]
		[HttpGet("sites/{siteId}/arcGisMaps")]
		[ServiceFilter(typeof(CustomerValidationFilter))]
		[ProducesResponseType(typeof(ArcGisMapsDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ArcGisMapsDto), StatusCodes.Status400BadRequest)]
		[SwaggerOperation("Get ArcGis Maps", Tags = new[] { "ArcGis" })]
		public async Task<IActionResult> GetMaps([FromRoute] Guid siteId)
		{
			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

			var token = await _arcGisService.GetToken();
			var json = await _arcGisService.GetArcGisMapsJson(token.Token);
			var obj = JObject.Parse(json);
			var results = (JArray)obj["results"];
			var arcGisMapsDto = new ArcGisMapsDto()
			{
				Maps = results.Select(r => new ArcGisMapDto()
				{
					Id = (string)r["id"],
					Title = (string)r["title"]
				}).ToList()
			};

			return Ok(arcGisMapsDto);
		}
	}
}
