using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Helpers.Adapters;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Git Import Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GitImportController : ControllerBase
{
    private readonly IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest> _requestAdapter;
    private readonly IGitImporterService _importerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitImportController"/> class.
    /// </summary>
    /// <param name="importerService">Implementation of Importer Service.</param>
    /// <param name="requestAdapter">Request Adapter.</param>
    public GitImportController(
        IGitImporterService importerService,
        IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest> requestAdapter)
    {
        ArgumentNullException.ThrowIfNull(importerService, nameof(importerService));
        ArgumentNullException.ThrowIfNull(requestAdapter, nameof(requestAdapter));
        _requestAdapter = requestAdapter;
        _importerService = importerService;
    }

    /// <summary>
    /// Imports Models from Git repository.
    /// </summary>
    /// <param name="request">Request containing chosen Ontology repository, Commit SHA and Import reason optional comment.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///    "requestPath": "wil-dev-lda-cu1-eu21-adt.api.wus2.digitaltwins.azure.net/import/requests/user@willowinc.com.Models.2022.10.03.09.57.25",
    ///    "jobId": "user@willowinc.com.Models.2022.10.03.09.57.25",
    ///    "details": {
    ///        "status": "Queued"
    ///    },
    ///    "createTime": "2022-10-03T09:57:25.1261765Z",
    ///    "lastUpdateTime": "2022-10-03T09:57:29.6903558Z",
    ///    "userId": "user@willowinc.com",
    ///    "target": [
    ///        "Models"
    ///    ]
    /// }.
    /// </returns>
    [HttpPost("models")]
    [Authorize(Policy = AppPermissions.CanImportModelsFromGit)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
    [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobsEntry>> ModelsFromRepoAsync([FromBody] GitRepoRequest request) =>
        Ok(await _importerService.ImportAsync(
                _requestAdapter.AdaptData(request),
                request.UserInfo,
                request.UserId));
}
