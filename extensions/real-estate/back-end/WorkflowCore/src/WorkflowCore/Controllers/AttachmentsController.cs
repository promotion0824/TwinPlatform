using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using Willow.Common;
using Willow.Data;
using WorkflowCore.Controllers.Request;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IReadRepository<Guid, Site> _siteRepo;
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IWorkflowService _workflowService;
        private readonly IAttachmentsServices _attachmentsServices;
		private readonly ISessionService _sessionService;

		public AttachmentsController(IImagePathHelper imagePathHelper, IWorkflowService workflowService, IAttachmentsServices attachmentsServices, IReadRepository<Guid, Site> siteRepo, ISessionService sessionService)
		{
			_siteRepo = siteRepo;
			_imagePathHelper = imagePathHelper;
			_workflowService = workflowService;
			_attachmentsServices = attachmentsServices;
			_sessionService = sessionService;
		}

		[HttpPost("sites/{siteId}/tickets/{ticketId}/attachments")]
        [Authorize]
        [ProducesResponseType(typeof(AttachmentDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateTicketAttachment([FromRoute] Guid siteId,
																[FromRoute] Guid ticketId,
																[FromForm] IFormFile attachmentFile,
																[FromForm] CreateTicketAttachmentRequest request)
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
			_sessionService.SetSessionData(request?.SourceType, request?.SourceId);
			byte[] content;
            using (var memoryStream = new MemoryStream())
            using (var imageStream = attachmentFile.OpenReadStream())
            {
                imageStream.CopyTo(memoryStream);
                content = memoryStream.ToArray();
            }

            var attachment = await _attachmentsServices.CreateTicketAttachment(ticket.CustomerId, ticket.SiteId, ticket.Id, attachmentFile.FileName, content);
            return Ok(AttachmentDto.MapFromTicketModel(attachment, _imagePathHelper, ticket));
        }

        [HttpDelete("sites/{siteId}/tickets/{ticketId}/attachments/{attachmentId}")]
        [Authorize]
        [ProducesResponseType(typeof(AttachmentDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteTicketAttachment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromRoute] Guid attachmentId)
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

            await _attachmentsServices.DeleteTicketAttachment(ticket.CustomerId, ticket.SiteId, ticket.Id, attachmentId);
            return NoContent();
        }

        [HttpPost("sites/{siteId}/checkRecords/{checkRecordId}/attachments")]
        [Authorize]
        [ProducesResponseType(typeof(AttachmentDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCheckRecordAttachment(
            [FromRoute] Guid siteId,
            [FromRoute] Guid checkRecordId,
            [FromForm] IFormFile attachmentFile)
        {
            var site = await _siteRepo.Get(siteId);
            var checkRecord = await _workflowService.GetCheckRecord(checkRecordId);
            if (checkRecord == null)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }

            byte[] content;
            using (var memoryStream = new MemoryStream())
            using (var imageStream = attachmentFile.OpenReadStream())
            {
                imageStream.CopyTo(memoryStream);
                content = memoryStream.ToArray();
            }

            var attachment = await _attachmentsServices.CreateCheckRecordAttachment(site.CustomerId, siteId, checkRecordId, attachmentFile.FileName, content);
            return Ok(AttachmentDto.MapFromCheckRecordModel(attachment, _imagePathHelper, site.CustomerId, siteId, checkRecordId));
        }

        [HttpDelete("sites/{siteId}/checkRecords/{checkRecordId}/attachments/{attachmentId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteCheckRecordAttachment(
            [FromRoute] Guid siteId,
            [FromRoute] Guid checkRecordId,
            [FromRoute] Guid attachmentId)
        {
            var site = await _siteRepo.Get(siteId);
            var checkRecord = await _workflowService.GetCheckRecord(checkRecordId);
            if (checkRecord == null)
            {
                throw new NotFoundException(new { CheckRecordId = checkRecordId });
            }

            await _attachmentsServices.DeleteCheckRecordAttachment(site.CustomerId, siteId, checkRecordId, attachmentId);
            return NoContent();
        }
    }
}
