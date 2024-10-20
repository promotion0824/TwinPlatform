using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Services;
using MobileXL.Services.Apis.WorkflowApi;
using MobileXL.Dto;
using Swashbuckle.AspNetCore.Annotations;
using SixLabors.ImageSharp;
using MobileXL.Models;

namespace MobileXL.Features.Workflow
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly IWorkflowApiService _workflowApi;

        public AttachmentsController(IAccessControlService accessControl, IImageUrlHelper imageUrlHelper, IWorkflowApiService workflowApi)
        {
            _accessControl = accessControl;
            _imageUrlHelper = imageUrlHelper;
            _workflowApi = workflowApi;
        }

        [HttpPost("sites/{siteId}/tickets/{ticketId}/attachments")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(typeof(AttachmentDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Creates an attachment for the given ticket", Tags = new [] { "Workflow" })]
        public async Task<IActionResult> CreateAttachment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromForm] IFormFile attachmentFile)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            try
            {
                var stream = attachmentFile.OpenReadStream();
                Image.Load(stream);
            }
            catch (UnknownImageFormatException)
            {
                var error = new ValidationError();
                error.Items.Add(new ValidationErrorItem("attachmentFile", "Invalid or unsupported image file"));
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var createdAttachment = await _workflowApi.CreateAttachment(siteId, ticketId, attachmentFile.FileName, attachmentFile.OpenReadStream(), "ticket");
            return Ok(AttachmentDto.MapFromModel(createdAttachment, _imageUrlHelper));
        }

        [HttpDelete("sites/{siteId}/tickets/{ticketId}/attachments/{attachmentId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation("Deletes an attachment from the given ticket", Tags = new [] { "Workflow" })]
        public async Task<IActionResult> DeleteAttachment([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromRoute] Guid attachmentId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            await _workflowApi.DeleteAttachment(siteId, ticketId, attachmentId, "ticket");
            return NoContent();
        }
    
        [HttpPost("sites/{siteId}/checkRecords/{checkRecordId}/attachments")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AttachmentDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Creates an attachment for the given inspection check record", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> CreateInspectionAttachment(
            [FromRoute] Guid siteId,
            [FromRoute] Guid inspectionId,
            [FromRoute] Guid checkRecordId,
            [FromForm] IFormFile attachmentFile)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            try
            {
                var stream = attachmentFile.OpenReadStream();
                Image.Load(stream);
            }
            catch (UnknownImageFormatException)
            {
                var error = new ValidationError();
                error.Items.Add(new ValidationErrorItem("attachmentFile", "Invalid or unsupported image file"));
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var createdAttachment = await _workflowApi.CreateAttachment(siteId, checkRecordId, attachmentFile.FileName, attachmentFile.OpenReadStream(), "checkRecord");
            return Ok(AttachmentDto.MapFromModel(createdAttachment, _imageUrlHelper));
        }

        [HttpDelete("sites/{siteId}/checkRecords/{checkRecordId}/attachments/{attachmentId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation("Deletes an attachment from the given inspection check record", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> DeleteInspectionAttachment(
            [FromRoute] Guid siteId,
            [FromRoute] Guid checkRecordId,
            [FromRoute] Guid attachmentId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            await _workflowApi.DeleteAttachment(siteId, checkRecordId, attachmentId, "checkRecord");

            return NoContent();
        }
    }
}
