using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;
using Willow.ExceptionHandling.Exceptions;


namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ModuleTypesController : Controller
    {
        private readonly IModuleTypesService _moduleTypesService;

        public ModuleTypesController(IModuleTypesService moduleTypesService)
        {
            _moduleTypesService = moduleTypesService;
        }

        [HttpGet("[Controller]")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Gets a list of defined module types")]
        public async Task<ActionResult<List<ModuleTypeDto>>> GetModuleTypes()
        {
            var moduleTypes = await _moduleTypesService.GetModuleTypesAsync();
            return Ok(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [HttpGet("sites/{siteId}/[Controller]")]
        [Authorize]
        [SwaggerOperation("Gets a list of defined module types for site")]
        public async Task<ActionResult<List<ModuleTypeDto>>> GetSiteModuleTypes([FromRoute] Guid siteId)
        {
            var moduleTypes = await _moduleTypesService.GetModuleTypesAsync(siteId);
            return Ok(ModuleTypeDto.MapFrom(moduleTypes));
        }

        [HttpPost("sites/{siteId}/[Controller]/default")]
        [Authorize]
        [SwaggerOperation("Creates the list of default module types for a site")]
        public async Task<ActionResult<List<ModuleTypeDto>>> CreateDefaults([FromRoute] Guid siteId)
        {
            var result = await _moduleTypesService.CreateDefaultModuleTypesAsync(siteId);
            return Ok(ModuleTypeDto.MapFrom(result));
        }

        [HttpPost("sites/{siteId}/[Controller]")]
        [Authorize]
        [SwaggerOperation("Creates a module type for a site")]
        public async Task<ActionResult<ModuleTypeDto>> CreateModuleType([FromRoute] Guid siteId, [FromBody] ModuleTypeRequest createModuleTypeRequest)
        {
            if (!await _moduleTypesService.IsValidPrefix(siteId, createModuleTypeRequest.Prefix, createModuleTypeRequest.Is3D))
                throw new ArgumentException("Duplicated prefix");

            var moduleType = await _moduleTypesService.CreateModuleTypeAsync(siteId,
                new ModuleType
                {
                    CanBeDeleted = createModuleTypeRequest.CanBeDeleted,
                    Is3D = createModuleTypeRequest.Is3D,
                    ModuleGroup = createModuleTypeRequest.ModuleGroup,
                    Name = createModuleTypeRequest.Name,
                    Prefix = createModuleTypeRequest.Prefix,
                    SortOrder = createModuleTypeRequest.SortOrder,
                    IsDefault = createModuleTypeRequest.IsDefault
                });

            return Ok(ModuleTypeDto.MapFrom(moduleType));
        }

        [HttpPut("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Updates a module type")]
        public async Task<ActionResult<ModuleTypeDto>> UpdateModuleType([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] ModuleTypeRequest updateModuleTypeRequest)
        {
            var moduleType = await _moduleTypesService.GetModuleTypeAsync(id);

            if (moduleType == null)
                throw new NotFoundException(  new { SiteId = siteId, ModuleTypeId = id });

            if (!await _moduleTypesService.IsValidPrefix(siteId, updateModuleTypeRequest.Prefix, updateModuleTypeRequest.Is3D, id))
                throw new ArgumentException("Duplicated prefix");

            moduleType = await _moduleTypesService.UpdateModuleTypeAsync(siteId, id,
                new ModuleType
                {
                    CanBeDeleted = updateModuleTypeRequest.CanBeDeleted,
                    Is3D = updateModuleTypeRequest.Is3D,
                    ModuleGroup = updateModuleTypeRequest.ModuleGroup,
                    Name = updateModuleTypeRequest.Name,
                    Prefix = updateModuleTypeRequest.Prefix,
                    SortOrder = updateModuleTypeRequest.SortOrder,
                    IsDefault = updateModuleTypeRequest.IsDefault
                });

            return Ok(ModuleTypeDto.MapFrom(moduleType));
        }

        [HttpDelete("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Delete module type by module type id")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var moduleType = await _moduleTypesService.GetModuleTypeAsync(id);

            if (moduleType == null)
                throw new NotFoundException(new { MmoduleTypeId = id });

            if (moduleType.HasModuleAssignments)
                throw new ArgumentException("Module type can not be deleted, it has module assignments");

            await _moduleTypesService.DeleteModuleTypeAsync(id);
            return Ok();
        }
    }
}
