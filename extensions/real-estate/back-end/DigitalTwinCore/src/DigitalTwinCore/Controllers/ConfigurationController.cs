using System;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Controllers
{
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;

        public ConfigurationController(IDigitalTwinServiceProvider digitalTwinServiceFactory)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
        }

        private async Task<IDigitalTwinService> GetDigitalTwinServiceAsync(Guid siteId)
        {
            return await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<SiteConfigurationDto>> GetAsync([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            return new SiteConfigurationDto
            {
                SiteCodeForModelId = service.SiteAdtSettings.SiteCodeForModelId,
                AdtInstanceUri = service.SiteAdtSettings.InstanceSettings.InstanceUri.ToString()
            };
        }

        [HttpGet]
        [Route("statistics")]
        [Authorize]
        public async Task<ActionResult<AdtSiteStatsDto>> GetSiteStatsAsync([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            return await service.GenerateADTInstanceStats();
        }

        [HttpPost]
        [Route("reloadFromAdt")]
        [Authorize]
        public async Task ReloadFromAdt([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            await service.StartReloadFromAdtAsync();
        }

    }
}
