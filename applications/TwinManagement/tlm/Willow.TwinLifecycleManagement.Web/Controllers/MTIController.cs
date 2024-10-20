using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Async;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// MTI Controller.
/// </summary>
/// <param name="mappingService">Implementation of Mapping Service.</param>
/// <param name="mtiService">Implementation of MTI Service.</param>
[ApiController]
[Route("api/[controller]")]
public class MTIController(IMappingService mappingService, IMtiService mtiService, ILogger<MTIController> logger) : ControllerBase
{
    /// <summary>
    ///  Synchronizes the organization data from Mapped to Azure Digital Twins.
    /// </summary>
    /// <param name="autoApprove">Flag to auto approve</param>
    /// <returns>An asynchronous task.</returns>
    [HttpPost("SyncOrganization")]
    [Authorize(Policy = AppPermissions.CanSyncToMapped)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncOrganization(bool autoApprove)
    {
        return await HandleRequestWithTryCatch(mtiService.SyncOrganization, autoApprove);
    }

    /// <summary>
    /// Synchronizes the spatial data from Mapped to Azure Digital Twins for a building.
    /// </summary>
    /// <param name="buildingIds">Array of Mapped building identifier.</param>
    /// <param name="autoApprove">Flag to auto approve</param>
    /// <returns>An asynchronous task.</returns>
    [HttpPost("SyncSpatial")]
    [Authorize(Policy = AppPermissions.CanSyncToMapped)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncSpatial([FromBody] string[] buildingIds, bool autoApprove)
    {
        return await HandleRequestWithTryCatch(mtiService.SyncSpatial, buildingIds, autoApprove);
    }

    /// <summary>
    /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
    /// </summary>
    /// <param name="buildingIds">Array of Mapped building identifier.</param>
    /// <param name="connectorId">The Mapped connector identifier.</param>
    /// <param name="autoApprove">Flag to auto approve</param>
    /// <returns>An asynchronous task.</returns>
    [HttpPost("SyncAssets")]
    [Authorize(Policy = AppPermissions.CanSyncToMapped)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncAssets([FromBody] string[] buildingIds, string connectorId, bool autoApprove)
    {
        return await HandleRequestWithTryCatch(mtiService.SyncAssets, buildingIds, connectorId, autoApprove);
    }

    /// <summary>
    /// Synchronizes the asset data from Mapped to Azure Digital Twins for a building and connector.
    /// </summary>
    /// <param name="buildingIds">Array of Mapped building identifier.</param>
    /// <param name="connectorId">The Mapped connector identifier.</param>
    /// <param name="autoApprove">Flag to auto approve</param>
    /// <param name="matchStdPntList">Flag to filter to match Standard Points List</param>
    /// <returns>An asynchronous task.</returns>
    [HttpPost("SyncCapabilities")]
    [Authorize(Policy = AppPermissions.CanSyncToMapped)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncCapabilities([FromBody] string[] buildingIds, string connectorId, bool autoApprove, bool matchStdPntList)
    {
        return await HandleRequestWithTryCatch(mtiService.SyncCapabilities, buildingIds, connectorId, autoApprove, matchStdPntList);
    }

    /// <summary>
    /// Find MTI Async Job.
    /// </summary>
    /// <param name="jobId">Id of the Job.</param>
    /// <param name="status">Status of the Job.</param>
    /// <returns><see cref="MtiAsyncJob"/>.</returns>
    [HttpGet("FindMtiAsyncJob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MtiAsyncJob>>> FindMtiAsyncJob(
        [FromQuery] string jobId,
        [FromQuery] AsyncJobStatus? status = null)
    {
        logger.LogInformation("FindMtiAsyncJob called with jobId: {jobId}, status: {status}", jobId, status);
        var jobs = await mappingService.FindMtiAsyncJobs(jobId, status);

        var ret = jobs.ToList();
        return ret;
    }

    /// <summary>
    /// Get Latest MTI Job.
    /// </summary>
    /// <param name="status">Async Job Status.</param>
    /// <returns><see cref="MtiAsyncJob"/>.</returns>
    [HttpGet("getLatestMtiAsyncJob")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MtiAsyncJob>> GetLatestMtiAsyncJob(
        [FromQuery] AsyncJobStatus? status = null)
    {
        var job = await mappingService.GetLatestMtiAsyncJob(status);

        return job;
    }

    /// <summary>
    /// Create a MTI async job and store in storage account.
    /// </summary>
    /// <param name="request"><see cref="MtiAsyncJobRequest"/>.</param>
    /// <returns><see cref="MtiAsyncJob"/>.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("CreateMtiAsyncJob")]
    public async Task<ActionResult<MtiAsyncJob>> CreateMtiAsyncJob([FromBody] MtiAsyncJobRequest request)
    {
        var mtiJob = await mappingService.CreateMtiAsyncJob(request);

        return mtiJob;
    }

    private async Task<IActionResult> HandleRequestWithTryCatch(Func<string, object[], Task<HttpResponseMessage>> requestToExecute, params object[] parameters)
    {
        var currentUserEmailClaim = User.FindFirst(claim => claim.Type == "emails" || claim.Type == ClaimTypes.Email).Value;
        try
        {
            HttpResponseMessage response = await requestToExecute(currentUserEmailClaim, parameters);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return Ok(responseData);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
