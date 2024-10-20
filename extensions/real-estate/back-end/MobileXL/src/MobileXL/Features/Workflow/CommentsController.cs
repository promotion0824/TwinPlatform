using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Dto;
using MobileXL.Models;
using MobileXL.Services;
using MobileXL.Services.Apis.WorkflowApi;
using MobileXL.Services.Apis.WorkflowApi.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace MobileXL.Features.Workflow
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IWorkflowApiService _workflowApi;

        public CommentsController(IAccessControlService accessControl, IWorkflowApiService workflowApi)
        {
            _accessControl = accessControl;
            _workflowApi = workflowApi;
        }

        [HttpPost("sites/{siteId}/tickets/{ticketId}/comments")]
        [Authorize]
        [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Creates a comment", Tags = new [] { "Workflow" })]
        public async Task<IActionResult> CreateComment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromBody] CreateCommentRequest request)
        {
            var currentUserId = this.GetCurrentUserId();
            var currentUserType = this.GetCurrentUserType();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var comment = await _workflowApi.CreateComment(
                siteId,
                ticketId,
                new WorkflowCreateCommentRequest
                {
                    Text = request.Text,
                    CreatorType = CommentCreatorType.CustomerUser,
                    CreatorId = currentUserId
                }) ;
            return Ok(CommentDto.MapFromModel(comment));
        }

        [HttpDelete("sites/{siteId}/tickets/{ticketId}/comments/{commentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteComment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromRoute] Guid commentId)
        {
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), currentUserId, siteId);

            await _workflowApi.DeleteComment(siteId, ticketId, commentId);
            return NoContent();
        }

    }
}
