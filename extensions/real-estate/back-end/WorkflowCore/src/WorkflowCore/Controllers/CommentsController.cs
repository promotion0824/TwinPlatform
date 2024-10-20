using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly ICommentsService _commentsServices;

        public CommentsController(IWorkflowService workflowService, ICommentsService commentsServices)
        {
            _workflowService = workflowService;
            _commentsServices = commentsServices;
        }

        [HttpPost("sites/{siteId}/tickets/{ticketId}/comments")]
        [Authorize]
        [ProducesResponseType(typeof(List<CommentDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateComment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromBody] CreateCommentRequest request)
        {
            var ticket = await _workflowService.GetTicket(ticketId, false, false);
            if (ticket == null)
            {
                throw new NotFoundException(new { TicketId = ticketId });
            }

            if (ticket.SiteId != siteId)
            {
                throw new ArgumentException().WithData(new { TicketId = ticketId, SiteId = siteId });
            }

            var comment = await _commentsServices.CreateComment(siteId, ticketId, request);
            return Ok(CommentDto.MapFromModel(comment));
        }

        [HttpDelete("sites/{siteId}/tickets/{ticketId}/comments/{commentId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteComment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromRoute] Guid commentId)
        {
            var ticket = await _workflowService.GetTicket(ticketId, false, false);
            if (ticket == null)
            {
                throw new NotFoundException(new { TicketId = ticketId });
            }

            if (ticket.SiteId != siteId)
            {
                throw new ArgumentException().WithData(new { TicketId = ticketId, SiteId = siteId });
            }

            var result = await _commentsServices.DeleteComment(siteId, ticketId, commentId);
            if (!result)
            {
                throw new NotFoundException(new { CommentId = commentId });
            }
            return NoContent();
        }

    }
}
