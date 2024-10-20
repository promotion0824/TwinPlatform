using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Requests;
using Willow.Model.Responses;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Creates a document twin and uploads file
        /// </summary>
        /// <param name="data">Data containing twin information and file</param>
        /// <returns>Created twin</returns>
        /// <response code="200">Document twin information</response>
        /// <response code="400">Twin with an invalid document model</response>
        [HttpPost]
        [DisableRequestSizeLimit,
            RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
            ValueLengthLimit = int.MaxValue)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BasicDigitalTwin>> CreateDocument([FromForm][Required] CreateDocumentRequest data)
        {
            if (!_documentService.IsValidDocumentTwin(data?.Twin))
                return BadRequest(new ValidationProblemDetails
                {
                    Detail = $"Invalid twin model. Must inherit from {DocumentService.DocumentModelId}."
                });

            return await _documentService.CreateDocument(data);
        }

        /// <summary>
        /// Update a document type
        /// </summary>
        /// <param name="id">The document twin id</param>
        /// <param name="documentType">The document type</param>
        /// <remarks>
        /// <response code="404">Twin not found</response>
        /// <response code="200">Twin patched</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateDocumentType([FromRoute][Required(AllowEmptyStrings = false)] string id, [FromQuery][Required(AllowEmptyStrings = false)] string documentType)
        {
            var docTwin = await _documentService.GetDocumentTwin(id);
            if (docTwin == null)
            {
                return NotFound(new ProblemDetails { Detail = $"Document {id} not found" });
            }

            if (!_documentService.IsValidDocumentModelType(documentType))
            {
                return BadRequest($"'{documentType}' does not inherit from '{DocumentService.DocumentModelId}'");
            }

            await _documentService.UpdateDocumentType(docTwin, documentType);
            return Ok();
        }

        /// <summary>
        /// Gets document stream
        /// </summary>
        /// <param name="id">Document id</param>
        /// <returns>Document stream</returns>
        /// <response code="200">Document stream</response>
        /// <response code="422">Twin does not contain url</response>
        /// <response code="404">Target documen not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UnprocessableEntityObjectResult), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentStream([FromRoute][Required(AllowEmptyStrings = false)] string id)
        {
            var docTwin = await _documentService.GetDocumentTwin(id);
            if (docTwin == null)
            {
                return NotFound(new ProblemDetails { Detail = $"Document {id} not found" });
            }

            string documentUrl = _documentService.GetDocumentUrl(docTwin);
            var stream = await _documentService.GetDocumentStream(docTwin);
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(documentUrl, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(stream, contentType);
        }

        /// <summary>
        /// Associates document to a twin
        /// </summary>
        /// <param name="twinId">Twin id</param>
        /// <param name="documentId">Document id</param>
        /// <returns>Created relationship</returns>
        /// <response code="404">If target twin or document not found</response>
        /// <response code="200">Association created</response>
        [HttpPost("/twins/{twinId}/document/{documentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BasicRelationship>> LinkDocumentToTwin(
            [FromRoute][Required(AllowEmptyStrings = false)] string twinId,
            [FromRoute][Required(AllowEmptyStrings = false)] string documentId)
        {
            return await _documentService.LinkDocumentToTwin(twinId, documentId);
        }

        /// <summary>
        /// Delete document association to twin
        /// </summary>
        /// <param name="twinId">Twin id</param>
        /// <param name="documentId">Document id</param>
        /// <response code="204">When association is deleted</response>
        /// <response code="404">if twin or relationship not found</response>
        [HttpDelete("/twins/{twinId}/document/{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UnLinkDocumentFromTwin(
            [FromRoute][Required(AllowEmptyStrings = false)] string twinId,
            [FromRoute][Required(AllowEmptyStrings = false)] string documentId)
        {
            await _documentService.UnLinkDocumentFromTwin(twinId, documentId);
            return NoContent();
        }

        /// <summary>
        /// Get new blob upload info for new document
        /// </summary>
        /// <returns>container sas token</returns>
        /// <response code="200">container sas token</response>
        [HttpGet("getBlobUploadInfo", Name = "getBlobUploadInfo")]
        [ProducesResponseType(typeof(BlobUploadInfo), StatusCodes.Status200OK)]
        public async Task<BlobUploadInfo> GetBlobUploadInfo([FromQuery] string[] fileNames)
        {
            return await _documentService.GetBlobUploadInfo(fileNames);
        }

        /// <summary>
        /// Creates a document twin - direct blob upload from client's browser approach.
        /// </summary>
        /// <param name="createDocumentTwinRequest">Instance of create document twin request.</param>
        /// <returns>Created twin</returns>
        /// <response code="200">Document twin information</response>
        [HttpPost("clientUploadDocTwin", Name = "clientUploadDocTwin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BasicDigitalTwin>> ClientCreateDocument([Required][FromBody] CreateDocumentTwinRequest createDocumentTwinRequest)
        {
            return await _documentService.ClientCreateDocument(createDocumentTwinRequest);
        }

        /// <summary>
        /// Get document blobs count
        /// </summary>
        /// <returns>Document blobs count</returns>
        /// <response code="200">Document blobs count</response>
        [HttpGet("getDocumentBlobsCount", Name = "getDocumentBlobsCount")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<int> GetDocumentBlobsCount()
        {
            return await _documentService.GetDocumentBlobsCount();
        }
    }
}
