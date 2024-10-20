using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Sites;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Management
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class WorkgroupsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly ISiteService _siteService;

        public WorkgroupsController(
            IAccessControlService accessControl,
            IWorkflowApiService workflowApi,
            ISiteApiService siteApi,
            ISiteService siteService)
        {
            _accessControl = accessControl;
            _workflowApi = workflowApi;
            _siteApi = siteApi;
            _siteService = siteService;
        }

        [HttpGet("sites/{siteId}/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupSimpleDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get site workgroup", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetSiteWorkgroups([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewUsers, siteId);

            var workgroups = await _workflowApi.GetWorkgroups(siteId);
            return Ok(WorkgroupSimpleDto.MapFromModels(workgroups));
        }

        [HttpGet("management/sites/{siteId}/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get site workgroups", Tags = new[] { "Management" })]
        public async Task<IActionResult> GetWorkgroups([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewUsers, siteId);

            var workgroups = await _workflowApi.GetWorkgroups(siteId);
            return Ok(WorkgroupDetailDto.MapFromModels(workgroups));
        }

        [HttpGet("workgroups/all/{siteName}")]
        [HttpGet("workgroups/all")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupSimpleDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get site workgroup", Tags = new[] { "Sites" })]
        public async Task<IActionResult> GetWorkgroups([FromRoute] string siteName)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId()); 

            var workgroups = await _workflowApi.GetWorkgroups(siteName);
            return Ok(WorkgroupSimpleDto.MapFromModels(workgroups));
        }


        /// <summary>
        /// Retrieves all work groups or for a specific site.
        /// </summary>
        /// <param name="siteName">The name of the site.</param>
        /// <param name="isFromUserManagement">Indicates if the workgroup details are retrieved from user management only.</param>
        /// <returns>workgroup details.</returns>
        [HttpGet("management/workgroups/all/{siteName}")]
        [HttpGet("management/workgroups/all")]
        [Authorize]
        [ProducesResponseType(typeof(List<WorkgroupDetailDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get workgroups with details", Tags = new[] { "Management" })]
        public async Task<IActionResult> GetWorkgroupsDetails([FromRoute] string siteName, [FromQuery] bool isFromUserManagement = false)
        {
            var authorizedSiteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId());
            var workGroupDetailsList = await _accessControl.GetWorkgroups(siteName, isFromUserManagement);

            return Ok(workGroupDetailsList.Where(x => x.SiteId == default || authorizedSiteIds.Contains(x.SiteId)).ToList());

        }

        [HttpPost("management/sites/{siteId}/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(WorkgroupDetailDto), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Create a workgroup", Tags = new[] { "Management" })]
        public async Task<IActionResult> CreateWorkgroup([FromRoute] Guid siteId, [FromBody] CreateWorkgroupRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var error = new ValidationError();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Name is required"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            var workgroup = await _workflowApi.CreateWorkgroup(siteId, request);
            return Ok(WorkgroupDetailDto.MapFromModel(workgroup));
        }

        [HttpPut("management/sites/{siteId}/workgroups/{workgroupId}")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(WorkgroupDetailDto), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Updates the specified workgroup", Tags = new[] { "Management" })]
        public async Task<ActionResult> UpdateWorkgroup([FromRoute] Guid siteId, [FromRoute] Guid workgroupId, [FromBody] UpdateWorkgroupRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            var error = new ValidationError();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Name is required"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var workgroup = await _workflowApi.UpdateWorkgroup(siteId, workgroupId, request);
            return Ok(WorkgroupDetailDto.MapFromModel(workgroup));
        }

        [HttpDelete("management/sites/{siteId}/workgroups/{workgroupId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Delete a workgroup", Tags = new[] { "Management" })]
        public async Task<IActionResult> DeleteWorkgroup([FromRoute] Guid siteId, [FromRoute] Guid workgroupId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageUsers, siteId);

            await _workflowApi.DeleteWorkgroup(siteId, workgroupId);
            return NoContent();
        }
    }
}
