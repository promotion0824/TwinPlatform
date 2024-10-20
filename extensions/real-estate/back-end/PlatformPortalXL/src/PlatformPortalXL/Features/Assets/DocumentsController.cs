using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Threading.Tasks;
using System.Linq;
using PlatformPortalXL.Helpers;
using System.Collections.Generic;
using PlatformPortalXL.Features.Pilot;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;
using DigitalTwinCore.DTO;

namespace PlatformPortalXL.Features.Assets
{
    public class DocumentsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IControllerHelper _controllerHelper;

        public DocumentsController(IAccessControlService accessControl, IDigitalTwinApiService digitalTwinApiService, IControllerHelper controllerHelper)
        { 
            _accessControl = accessControl;
            _digitalTwinApiService = digitalTwinApiService;
            _controllerHelper = controllerHelper;
        }

        /// <summary>
        /// Uploads documents
        /// </summary>
        /// <param name="siteId">The site id</param>
        /// <param name="data">Object containing json object and form files</param>
        /// <returns>Document information</returns>
        [HttpPost("sites/{siteId}/[Controller]")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DocumentTwinDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Uploads documents", Tags = new[] { "Documents" })]
        public async Task<ActionResult<List<DocumentTwinDto>>> PostAsync([FromRoute] Guid siteId, CreateDocumentRequest data)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            if (data.formFiles == null || !data.formFiles.Any() || data.formFiles.Any(f => f == null))
            {
                throw new ArgumentNullException("Multipart formdata files collection expected");
            }

            var response = await _digitalTwinApiService.UploadDocumentAsync(siteId, data);

            return Ok(response.Select(d => new DocumentTwinDto
            {
                DisplayName = d.DisplayName,
                Id = d.Id,
                UniqueId = d.UniqueId,
                ModelId = d.Metadata.ModelId,
                Url = d.CustomProperties.Url
            }));
        }

        /// <summary>
        /// Get document stream by document id
        /// </summary>
        /// <param name="siteId">the site id</param>
        /// <param name="documentId">the document id</param>
        /// <returns>File stream</returns>
        [HttpGet("/sites/{siteId}/[Controller]/{documentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Get document stream by document id", Tags = new[] { "Documents" })]
        public async Task<ActionResult> GetDocumentStream(
            [FromRoute] Guid siteId,
            [FromRoute] string documentId)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            var fileDownload = await _digitalTwinApiService.GetDocumentStreamAsync(siteId, documentId);

            var contentDisposition = new ContentDisposition
            {
                FileName = fileDownload.FileName
            };

            Response.Headers.Add("Content-Disposition", $"{contentDisposition}");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return File(fileDownload.Content, fileDownload.ContentType.MediaType);
        }

        /// <summary>
        /// Associates a document to a twin
        /// </summary>
        /// <param name="siteId">The site id</param>
        /// <param name="twinUniqueId">The source twin unique id</param>
        /// <param name="documentUniqueId">The document unique id</param>
        /// <returns>Relationship entity</returns>
        [HttpPost("/sites/{siteId}/[Controller]/addlink")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<RelationshipDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Associates a document to a twin", Tags = new[] { "Documents" })]
        public async Task<ActionResult<List<RelationshipDto>>> LinkDocumentToTwin(
            [FromRoute] Guid siteId,
            [FromQuery] Guid twinUniqueId,
            [FromQuery] Guid documentUniqueId)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            if (twinUniqueId == Guid.Empty)
            {
                throw new ArgumentNullException("Invalid Source Twin unique id");
            }

            if (documentUniqueId == Guid.Empty)
            {
                throw new ArgumentNullException("Invalid Document Twin unique id");
            }

            var response = await _digitalTwinApiService.LinkDocumentToTwinAsync(siteId, twinUniqueId, documentUniqueId);

            return Ok(response);
        }

        /// <summary>
        /// Removes link between a twin and a document
        /// </summary>
        /// <param name="siteId">The site id</param>
        /// <param name="twinUniqueId">The twin unique id</param>
        /// <param name="documentUniqueId">The document unique id</param>
        /// <returns>No content on success</returns>
        [HttpDelete("/sites/{siteId}/[Controller]/deletelink")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Removes link between a twin and a document", Tags = new[] { "Documents" })]
        public async Task<ActionResult> DeleteLinkDocumentToTwin(
            [FromRoute] Guid siteId,
            [FromQuery] Guid twinUniqueId,
            [FromQuery] Guid documentUniqueId)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            if(twinUniqueId == Guid.Empty)
            {
                throw new ArgumentNullException("Invalid Source Twin unique id");
            }

            if (documentUniqueId == Guid.Empty)
            {
                throw new ArgumentNullException("Invalid Document Twin unique id");
            }

            await _digitalTwinApiService.DeleteDocumentLinkAsync(siteId, twinUniqueId, documentUniqueId);

            return NoContent();
        }
    }
}
