using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformPortalXL.Features.SiteStructure
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ModuleTypesController : Controller
    {
        private readonly IModuleTypesService _moduleTypesService;
        private readonly IAccessControlService _accessControl;
        private readonly IControllerHelper _controllerHelper;

        public ModuleTypesController(IAccessControlService accessControl, IModuleTypesService moduleTypesService, IControllerHelper controllerHelper)
        {
            _moduleTypesService = moduleTypesService;
            _accessControl = accessControl;
            _controllerHelper = controllerHelper;
        }

        [HttpGet("sites/{siteId}/[Controller]")]
        [Authorize]
        [SwaggerOperation("Gets a list of defined module types for site")]
        public async Task<ActionResult<List<ModuleTypeDto>>> GetSiteModuleTypes([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            var moduleTypes = await _moduleTypesService.GetModuleTypesAsync(siteId);
            return Ok(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [HttpPost("sites/{siteId}/[Controller]/default")]
        [Authorize]
        [SwaggerOperation("Creates default module types for site")]
        public async Task<ActionResult<List<ModuleTypeDto>>> CreateDefaultModuleTypes([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ManageSites, siteId);

            var moduleTypes = await _moduleTypesService.CreateDefaultModuleTypesAsync(siteId);
            return Ok(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [HttpPost("sites/{siteId}/[Controller]")]
        [Authorize]
        [SwaggerOperation("Creates module type for site")]
        public async Task<ActionResult<ModuleTypeDto>> CreateModuleType([FromRoute] Guid siteId, [FromBody] ModuleTypeRequest createModuleTypeRequest)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ManageSites, siteId);

            var moduleType = await _moduleTypesService.CreateModuleTypeAsync(siteId, createModuleTypeRequest);
            return Ok(ModuleTypeDto.MapFrom(moduleType));
        }

        [HttpPut("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [SwaggerOperation("Updates module type for site")]
        public async Task<ActionResult<ModuleTypeDto>> UpdateModuleType([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] ModuleTypeRequest updateModuleTypeRequest)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ManageSites, siteId);

            var moduleType = await _moduleTypesService.UpdateModuleTypeAsync(siteId, id, updateModuleTypeRequest);
            return Ok(ModuleTypeDto.MapFrom(moduleType));
        }

        [HttpDelete("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [SwaggerOperation("Deletes module type for site")]
        public async Task<ActionResult> DeleteModuleType([FromRoute] Guid siteId, [FromRoute] Guid id)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ManageSites, siteId);

            await _moduleTypesService.DeleteModuleTypeAsync(siteId, id);
            return Ok();
        }
    }
}
