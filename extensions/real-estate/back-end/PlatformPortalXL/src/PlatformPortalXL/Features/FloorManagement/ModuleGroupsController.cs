using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PlatformPortalXL.Features.FloorManagement
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ModuleGroupsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IControllerHelper _controllerHelper;
        private readonly IModuleGroupsService _moduleGroupsService;

        public ModuleGroupsController(IAccessControlService accessControl, IControllerHelper controllerHelper, IModuleGroupsService moduleGroupsService)
        {
            _accessControl = accessControl;
            _controllerHelper = controllerHelper;
            _moduleGroupsService = moduleGroupsService;
        }

        [HttpPut("sites/{siteId}/[Controller]/{id}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Updates a module group")]
        public async Task<ActionResult<ModuleGroupDto>> UpdateModuleGroup([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] ModuleGroupRequest updateModuleGroupRequest)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ManageSites, siteId);

            var moduleGroup = await _moduleGroupsService.UpdateModuleGroupAsync(siteId, id, updateModuleGroupRequest);
            return Ok(moduleGroup);
        }
    }
}
