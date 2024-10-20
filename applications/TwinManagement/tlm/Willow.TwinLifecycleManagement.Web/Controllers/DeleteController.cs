using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Delete Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DeleteController : ControllerBase
{
    private readonly IDeletionService _deletionService;
    private readonly ILogger<DeleteController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteController"/> class.
    /// </summary>
    /// <param name="deletionService">Deletion Service.</param>
    /// <param name="logger">Instance of ILogger.</param>
    public DeleteController(IDeletionService deletionService, ILogger<DeleteController> logger)
    {
        ArgumentNullException.ThrowIfNull(nameof(deletionService));
        ArgumentNullException.ThrowIfNull(nameof(logger));
        _deletionService = deletionService;
        _logger = logger;
    }

    /// <summary>
    /// Deletes all models.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <param name="includeDependencies">Boolean set to False.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///    "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Models.2022.10.03.11.11.33",
    ///    "delete": true,
    ///    "jobId": "user@willowinc.com.Models.2022.10.03.11.11.33",
    ///    "details": {
    ///        "status": "Queued"
    ///    },
    ///    "createTime": "2022-10-03T11:11:33.1009991Z",
    ///    "lastUpdateTime": "2022-10-03T11:11:33.3944003Z",
    ///    "userId": "user@willowinc.com",
    ///    "target": [
    ///        "Models"
    ///    ]
    /// }.
    /// </returns>
    [HttpDelete("models")]
    [Authorize(Policy = AppPermissions.CanDeleteModels)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> AllModels(
        [FromHeader(Name = "User-Id")][Required] string userId,
        [FromHeader(Name = "User-Data")] string userData = null,
        [FromQuery] bool includeDependencies = false)
    {
        _logger.LogInformation("Sending delete all models request");
        JobsEntry response = await _deletionService.DeleteAllModels(userId, includeDependencies, userData);
        _logger.LogInformation("All models deletion job was created successfully");
        return Ok(response);
    }

    /// <summary>
    /// Deletes all Twin files.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <param name="deleteOnlyRelationships">True if only relationship;else false.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///    "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Twins.2022.10.03.11.12.07",
    ///    "delete": true,
    ///    "jobId": "user@willowinc.com.Twins.2022.10.03.11.12.07",
    ///    "details": {
    ///        "status": "Queued"
    ///    },
    ///    "createTime": "2022-10-03T11:12:07.3642013Z",
    ///    "lastUpdateTime": "2022-10-03T11:12:07.6489966Z",
    ///    "userId": "user@willowinc.com",
    ///    "target": [
    ///        "Twins"
    ///    ]
    /// }.
    /// </returns>
    [HttpDelete("twins")]
    [Authorize(Policy = AppPermissions.CanDeleteAllTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> AllTwins(
        [FromHeader(Name = "User-Id")][Required] string userId,
        [FromHeader(Name = "User-Data")] string userData = null,
        [FromQuery] bool deleteOnlyRelationships = false)
    {
        _logger.LogInformation("Sending delete all twins request.");
        JobsEntry response = await _deletionService.DeleteAllTwins(userId, deleteOnlyRelationships, userData);
        _logger.LogInformation("All twins have been deleted.");
        return Ok(response);
    }

    /// <summary>
    /// Deletes all Twin files by Site ID provided by the user.
    /// </summary>
    /// <param name="siteId">Site ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <param name="isRelationshipsRequest">True if relationship request; else false.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///    "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Twins.2022.10.03.11.12.50",
    ///    "delete": true,
    ///    "jobId": "user@willowinc.com.Twins.2022.10.03.11.12.50",
    ///    "details": {
    ///        "status": "Queued"
    ///    },
    ///    "createTime": "2022-10-03T11:12:50.3565154Z",
    ///    "lastUpdateTime": "2022-10-03T11:12:50.6648902Z",
    ///    "userId": "user@willowinc.com",
    ///    "target": [
    ///        "Twins"
    ///    ]
    /// }.
    /// </returns>
    [HttpDelete("twins/{siteId}")]
    [Authorize(Policy = AppPermissions.CanDeleteTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> TwinsBasedOnSiteId(
        string siteId,
        [FromHeader(Name = "User-Id")][Required] string userId,
        [FromHeader(Name = "User-Data")] string userData = null,
        [FromHeader(Name = "IsRelationshipsRequest")] bool isRelationshipsRequest = false)
    {
        _logger.LogInformation("Sending delete twins based on site id request");
        JobsEntry response = await _deletionService.DeleteSiteIdTwins(siteId, userId, userData, isRelationshipsRequest);
        _logger.LogInformation("Specific twin deletion job was created successfully");
        return Ok(response);
    }

    /// <summary>
    /// Deletes specific Twin file provided by the user.
    /// </summary>
    /// <param name="formFiles">Files added by user to upload.</param>
    /// <param name="userData">Additional user comments.</param>
    /// <param name="deleteOnlyRelationships">Delete only relationships.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///    "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Twins.2022.10.03.11.13.21",
    ///    "delete": true,
    ///    "jobId": "user@willowinc.com.Twins.2022.10.03.11.13.21",
    ///    "details": {
    ///        "status": "Queued"
    ///    },
    ///    "createTime": "2022-10-03T11:13:21.2285106Z",
    ///    "lastUpdateTime": "2022-10-03T11:13:21.5604565Z",
    ///    "userId": "user@willowinc.com",
    ///    "target": [
    ///        "Twins"
    ///    ]
    /// }.
    /// </returns>
    [HttpDelete("twinsOrRelationshipsBasedOnFile")]
    [Authorize(Policy = AppPermissions.CanDeleteTwinsorRelationshipByFile)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> TwinsOrRelationshipsBasedOnFile(
        [FromForm] List<IFormFile> formFiles,
        [FromForm] string userData = null,
        [FromForm] bool deleteOnlyRelationships = false)
    {
        if (formFiles.Count == 0)
        {
            return BadRequest("Empty formFiles");
        }

        _logger.LogInformation("Sending delete twins file request");
        JobsEntry response = await _deletionService.DeleteTwinsOrRelationshipsByFile(formFiles, deleteOnlyRelationships, userData);
        _logger.LogInformation("File twin deletion job was created successfully");
        return Ok(response);
    }
}
