using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Pilot
{
    //TEMP FOR DEMO 2020-08-17
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AssetRelationshipsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssetRelationshipsController(IAccessControlService accessControl, IHttpClientFactory httpClientFactory)
        {
            _accessControl = accessControl;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Retrieves asset relationships by id
        /// </summary>
        [HttpGet("pilot/sites/{siteId}/assets/{assetId}/relationships")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation("Gets asset relationships", Tags = new [] { "Pilot" })]
        public async Task<ActionResult<AssetRelationshipsDto>> GetAssetRelationships([FromRoute] Guid siteId, [FromRoute] Guid assetId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.DigitalTwinCore))
            {
                var response = await client.GetAsync($"sites/{siteId}/assets/{assetId}/relationships");

                await response.EnsureSuccessStatusCode(ApiServiceNames.DigitalTwinCore);
                return await response.Content.ReadAsAsync<AssetRelationshipsDto>();
            }
        }
    }
}
