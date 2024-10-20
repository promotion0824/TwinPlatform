using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DigitalTwinCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using DigitalTwinCore.Dto;
using Willow.Infrastructure.Exceptions;
using System.Linq;
using DigitalTwinCore.DTO;

namespace DigitalTwinCore.Controllers
{
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IDocumentsService _documentsService;

        public DocumentsController(ILogger<DocumentsController> logger, IDocumentsService documentsService)
        {
            _logger = logger;
            _documentsService = documentsService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult<List<TwinWithRelationships>>> PostAsync([FromRoute] Guid siteId, CreateDocumentRequest data, [FromQuery] bool isSyncRequired = true)
        {
            if (data.formFiles == null || !data.formFiles.Any())
            {
                throw new BadRequestException("Files collection expected");
            }

            if (!string.IsNullOrEmpty(data.Id) && data.formFiles.Count > 1)
                throw new BadRequestException("Multiple files are not allowed when editing an existing document");

            var createdEntities = new List<TwinWithRelationships>();
            foreach (var formFile in data.formFiles)
            {
                var blobName = await _documentsService.UploadFile(data.FileMimeType, formFile, data.ShareStorageForSameFile);

                var entity = await _documentsService.CreateFileTwin(siteId, data, formFile, blobName, isSyncRequired);
                createdEntities.Add(entity);
            }

            return Created(HttpContext.Request.Path, createdEntities);
        }    
        
        [HttpPost("/sites/{siteId}/[Controller]/addlink")]
        [Authorize]
        public async Task<ActionResult<List<RelationshipDto>>> LinkDocumentToTwin(
            [FromRoute] Guid siteId,
            [FromQuery] Guid twinUniqueId,
            [FromQuery] Guid documentUniqueId)
        {
            try
            {
                var twin = await _documentsService.GetTwinByUniqueIdAsync(siteId, twinUniqueId);
                if (twin == null)
                {
                    return BadRequest("Source Twin not found");
                }

                var document = await _documentsService.GetTwinByUniqueIdAsync(siteId, documentUniqueId);
                if (document == null)
                {
                    return BadRequest("Document twin not found");
                }

                var relationship = await _documentsService.AddRelationshipAsync(siteId, twin.Id, document.Id);

                return Created(HttpContext.Request.Path, relationship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not link document to twin -  SiteId:{SiteId}, ExistingDocumentUniqueId: {DocumentUniqueId}, TwinUniqueId: {TwinUniqueId}",
                    siteId, documentUniqueId, twinUniqueId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Could not link document to twin");
            }
        }

        [HttpDelete("/sites/{siteId}/[Controller]/deletelink")]
        [Authorize]
        public async Task<ActionResult> DeleteLinkDocumentToTwin(
            [FromRoute] Guid siteId,
            [FromQuery] Guid twinUniqueId,
            [FromQuery] Guid documentUniqueId)
        {
            try
            {
                var twin = await _documentsService.GetTwinByUniqueIdAsync(siteId, twinUniqueId);
                if (twin == null)
                {
                    return BadRequest("Source Twin not found");
                }

                var document = await _documentsService.GetTwinByUniqueIdAsync(siteId, documentUniqueId);
                if (document == null)
                {
                    return BadRequest("Document Twin not found");
                }

                await _documentsService.DeleteRelationshipAsync(siteId, twin.Id, document.Id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not delete link between document and twin -  SiteId:{SiteId}, ExistingDocumentUniqueId: {DocumentUniqueId}, TwinUniqueId: {TwinUniqueId}",
                    siteId, documentUniqueId, twinUniqueId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Could not delete link between document and twin");
            }
        }
    }
}
