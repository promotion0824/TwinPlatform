using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.ExceptionHandling.Exceptions;


namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ModuleGroupsController : Controller
    {
        private readonly IModuleGroupsService _moduleGroupsService;

        public ModuleGroupsController(IModuleGroupsService moduleGroupsService)
        {
            _moduleGroupsService = moduleGroupsService;
        }

        [HttpPut("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Updates a module group")]
        public async Task<ActionResult<ModuleGroupDto>> UpdateModuleGroup([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] ModuleGroupRequest updateModuleGroupRequest)
        {
            var moduleGroup = await _moduleGroupsService.GetModuleGroupAsync(id);

            if (moduleGroup == null)
                throw new NotFoundException(new { SiteId = siteId, ModuleGroupId = id });

            var existingModuleGroup = await _moduleGroupsService.GetModuleGroupByNameAsync(siteId, updateModuleGroupRequest.Name);
            if (existingModuleGroup != null && existingModuleGroup.Id != id)
                throw new ArgumentException("Duplicated name");

            moduleGroup = await _moduleGroupsService.UpdateModuleGroupAsync(new ModuleGroup
            {
                Id = id,
                SiteId = siteId,
                Name = updateModuleGroupRequest.Name,
                SortOrder = updateModuleGroupRequest.SortOrder
            });

            return Ok(new ModuleGroupDto {
                Id = moduleGroup.Id,
                Name = moduleGroup.Name,
                SortOrder = moduleGroup.SortOrder,
                SiteId = moduleGroup.SiteId
            });
        }
    }
}
