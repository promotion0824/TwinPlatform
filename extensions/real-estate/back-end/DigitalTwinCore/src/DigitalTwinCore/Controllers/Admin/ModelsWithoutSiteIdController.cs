using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Cacheless;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DigitalTwinCore.Controllers.Admin
{
    [ApiController]
    public class ModelsWithoutSiteIdController : ControllerBase
    {
        private readonly IDigitalTwinService _digitalTwinService;

        public ModelsWithoutSiteIdController(
            IDigitalTwinService digitalTwinService)
        {
            _digitalTwinService = digitalTwinService;
        }

        [HttpGet("admin/models/{id}/properties")]
        [Authorize]
        public async Task<IActionResult> GetModelProperties([FromRoute] string id)
        {
            var service = (_digitalTwinService as CachelessAdtService);
            var modelProps = await service.GetModelProps(id);

            return Ok(modelProps);
        }
    }
}
