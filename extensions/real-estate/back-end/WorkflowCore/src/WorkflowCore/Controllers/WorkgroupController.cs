using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class WorkgroupController : ControllerBase
    {
        private readonly IWorkgroupService _workgroupService;

        public WorkgroupController(IWorkgroupService workgroupService)
        {
            _workgroupService = workgroupService;
        }

        [HttpPost("sites/{siteId}/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateWorkgroup([FromRoute] Guid siteId, [FromBody]CreateWorkgroupRequest request)
        {
            var workgroup = await _workgroupService.CreateWorkgroup(siteId, request);
            return Ok(WorkgroupDto.MapFromModel(workgroup));
        }

        [HttpPut("sites/{siteId}/workgroups/{workgroupId}")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateWorkgroup ([FromRoute] Guid siteId, [FromRoute] Guid workgroupId, [FromBody] UpdateWorkgroupRequest request)
        {
            await _workgroupService.UpdateWorkgroup(siteId, workgroupId, request);
            var workgroup = await _workgroupService.GetWorkgroup(siteId, workgroupId, true);
            return Ok(WorkgroupDto.MapFromModel(workgroup));
        }

        [HttpGet("sites/{siteId}/workgroups/{workgroupId}")]
        [Authorize]
        [ProducesResponseType(typeof(WorkgroupDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWorkgroup ([FromRoute] Guid siteId, [FromRoute] Guid workgroupId)
        {
            var workgroup = await _workgroupService.GetWorkgroup(siteId, workgroupId, false);
            return Ok(WorkgroupDto.MapFromModel(workgroup));
        }

        [HttpGet("sites/{siteId}/workgroups")]
        [Authorize]
        [ProducesResponseType(typeof(List<WorkgroupDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWorkgroups([FromRoute] Guid siteId)
        {
            var workgroups = await _workgroupService.GetWorkgroups(siteId, true);
            return Ok(WorkgroupDto.MapFromModels(workgroups));
        }

        [HttpGet("workgroups/all/{siteName}")]
        [HttpGet("workgroups/all")]
        [Authorize]
        [ProducesResponseType(typeof(List<WorkgroupDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetWorkgroups([FromRoute] string siteName)
        {
            var workgroups = await _workgroupService.GetWorkgroups(siteName, true);
            return Ok(WorkgroupDto.MapFromModels(workgroups));
        }

        [HttpDelete("sites/{siteId}/workgroups/{workgroupId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteWorkgroup([FromRoute] Guid siteId, [FromRoute] Guid workgroupId)
        {
            var success = await _workgroupService.DeleteWorkgroup(siteId, workgroupId);
            if (!success)
            {
                throw new NotFoundException( new { WorkgroupId = workgroupId });
            }
            return NoContent();
        }
    }
}
