using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions;
using Willow.Model.Async;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Services;
using BlobUploadInfo = Willow.AzureDigitalTwins.SDK.Client.BlobUploadInfo;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// File Import Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileImportController : ControllerBase
{
    private readonly IFileImporterService _importerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportController"/> class.
    /// </summary>
    /// <param name="importerService">Implementation of Importer Service.</param>
    public FileImportController(IFileImporterService importerService)
    {
        ArgumentNullException.ThrowIfNull(importerService, nameof(importerService));
        _importerService = importerService;
    }

    /// <summary>
    /// Imports Twin files with Site ID provided by the user.
    /// </summary>
    /// <param name="formFiles">Files added by user to upload.</param>
    /// <param name="siteId">Site ID.</param>
    /// <param name="includeRelationships">Process and import relationships in the file.</param>
    /// <param name="includeTwinProperties">Process and import twin properties in the file.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///  "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Twins.2022.10.03.11.08.50",
    ///  "jobId": "user@willowinc.com.Twins.2022.10.03.11.08.50",
    ///  "details": {
    ///    "status": "Queued"
    ///  },
    ///  "createTime": "2022-10-03T11:08:50.7590456Z",
    ///  "lastUpdateTime": "2022-10-03T11:08:51.0370046Z",
    ///  "userId": "user@willowinc.com",
    ///  "target": [
    ///    "Twins"
    ///  ]
    /// }.
    /// </returns>
    [HttpPost("ImportTwins")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [Authorize(Policy = AppPermissions.CanImportTwins)]

    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> TwinsAndRelationshipsAsync(
        [FromForm] IEnumerable<IFormFile> formFiles,
        [FromForm] string siteId,
        [FromForm] bool includeRelationships,
        [FromForm] bool includeTwinProperties,
        [FromForm] string userData = null)
    {
        return Ok(await _importerService.ImportAsync(formFiles, siteId, includeRelationships, userData, includeTwinProperties));
    }

    /// <summary>
    /// Import historical time series from sas url by the user.
    /// </summary>
    /// <param name="sasUrl">SAS URL of historical time series data.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///  "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Twins.2022.10.03.11.08.50",
    ///  "jobId": "user@willowinc.com.Twins.2022.10.03.11.08.50",
    ///  "details": {
    ///    "status": "Queued"
    ///  },
    ///  "createTime": "2022-10-03T11:08:50.7590456Z",
    ///  "lastUpdateTime": "2022-10-03T11:08:51.0370046Z",
    ///  "userId": "user@willowinc.com",
    /// }.
    /// </returns>
    [HttpPost("ImportTimeSeriesWithSasUrl")]
    [DisableRequestSizeLimit]
    [Authorize(Policy = AppPermissions.CanImportTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(string))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> TimeSeriesSasUrlAsync(
        [FromForm] string sasUrl,
        [FromForm] string userData = null)
    {
        var jobId = await _importerService.ImportTimeSeriesFromBlobAsync(sasUrl, userData);
        return jobId;
    }

    /// <summary>
    /// Import time series from the request files.
    /// </summary>
    /// <param name="request">A time series request to import.</param>
    /// <returns><see cref="string"/>The job id.</returns>
    [HttpPost("clientUploadTimeSeries")]
    [Authorize(Policy = AppPermissions.CanImportTwins)]
    [Consumes("application/json")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(string))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> ClientUploadTimeSeries([Required][FromBody] ImportTimeSeriesHistoricalRequest request)
    {
        var jobId = await _importerService.ClientCreateFileTimeSeriesAsync(request);
        return jobId;
    }

    /// <summary>
    /// Create Document File Twin.
    /// </summary>
    /// <param name="createDocumentRequest">Create Document Request.</param>
    /// <returns><see cref="CreateDocumentResponse"/>.</returns>
    [HttpPost("documents")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [Authorize(Policy = AppPermissions.CanImportDocuments)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<CreateDocumentResponse>))]
    [ProducesResponseType(typeof(IEnumerable<CreateDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CreateDocumentResponse>>> DocumentsAsync(
        [FromForm] Models.CreateDocumentRequest createDocumentRequest)
    {
        return Ok(await _importerService.CreateFileTwinsAsync(createDocumentRequest));
    }

    /// <summary>
    /// Update Document.
    /// </summary>
    /// <param name="twinId">Twin Id.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="documentType">Type of the document.</param>
    /// <returns><see cref="UpdateDocumentResponse"/>.</returns>
    [HttpPut("document", Name = "PutDocument")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(UpdateDocumentResponse))]
    [ProducesResponseType(typeof(UpdateDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateDocumentResponse>> UpdateDocumentsAsync(
        [FromForm] string twinId, string fileName, string documentType)
    {
        return Ok(await _importerService.UpdateDocumentType(twinId, fileName, documentType));
    }

    /// <summary>
    /// List Documents.
    /// </summary>
    /// <returns>List of Documents.</returns>
    [HttpGet("documents")]
    [Authorize(Policy = AppPermissions.CanReadDocuments)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<Document>))]
    [ProducesResponseType(typeof(IEnumerable<Document>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsAsync()
    {
        return Ok(await _importerService.GetDocumentsAsync());
    }

    /// <summary>
    /// Get new blob upload info for new document.
    /// </summary>
    /// <param name="fileNames">Name of the file.</param>
    /// <returns>Blob upload information.</returns>
    [HttpGet("getBlobUploadInfo")]
    [Authorize(Policy = AppPermissions.CanImportDocuments)]
    [ProducesResponseType(typeof(BlobUploadInfo), StatusCodes.Status200OK)]
    public async Task<BlobUploadInfo> GetBlobUploadInfo([FromQuery] string[] fileNames)
    {
        return await _importerService.GetBlobUploadInfoAsync(fileNames);
    }

    /// <summary>
    /// Get new blob upload info for new time series data.
    /// </summary>
    /// <param name="fileNames">Name of the file.</param>
    /// <returns>Blob upload information.</returns>
    [HttpGet("getTimeSeriesBlobUploadInfo")]
    [Authorize(Policy = AppPermissions.CanImportDocuments)]
    [ProducesResponseType(typeof(BlobUploadInfo), StatusCodes.Status200OK)]
    public async Task<BlobUploadInfo> GetTimeSeriesBlobUploadInfo([FromQuery] string[] fileNames)
    {
        return await _importerService.GetTimeSeriesBlobUploadInfoAsync(fileNames);
    }

    /// <summary>
    /// Upload document from client.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="blobPath">Blob file path.</param>
    /// <param name="siteId">Site Id.</param>
    /// <returns><see cref="CreateDocumentResponse"/>.</returns>
    [HttpPost("clientUploadDocument")]
    [Consumes("application/x-www-form-urlencoded")]
    [Authorize(Policy = AppPermissions.CanImportDocuments)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(CreateDocumentResponse))]
    [ProducesResponseType(typeof(CreateDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateDocumentResponse>> ClientUploadDocument(
       [Required][FromForm] string fileName,
       [Required][FromForm] string blobPath,
       [FromForm] string siteId)
    {
        return Ok(await _importerService.ClientCreateFileTwinAsync(fileName, blobPath, siteId));
    }
}
