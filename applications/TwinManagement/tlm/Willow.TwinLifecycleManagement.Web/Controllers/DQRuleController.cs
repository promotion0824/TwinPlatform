using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.DataQuality.Model.Responses;
using Willow.Exceptions;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// DQ Rule Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DQRuleController : ControllerBase
{
    private readonly IDataQualityService _dataQualityService;
    private readonly IDQRuleService _dataQualityRuleService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DQRuleController"/> class.
    /// </summary>
    /// <param name="dataQualityService">Implementation of IDataQualityService.</param>
    /// <param name="dataQualityRuleService">Implementation of IDataQualityRuleService.</param>
    public DQRuleController(IDataQualityService dataQualityService, IDQRuleService dataQualityRuleService)
    {
        ArgumentNullException.ThrowIfNull(dataQualityService, nameof(dataQualityService));
        _dataQualityService = dataQualityService;
        ArgumentNullException.ThrowIfNull(dataQualityRuleService, nameof(dataQualityRuleService));
        _dataQualityRuleService = dataQualityRuleService;
    }

    /// <summary>
    /// Upload Rule files provided by the user.
    /// </summary>
    /// <param name="formFiles">Files added by user to upload.</param>
    /// <returns>
    /// Sample response:
    /// {
    ///  "fileUploaded": {
    ///    "AirHandlingUnit.json": null,
    ///    "AirHandlingUnitMaxValueParseFailed.json": "minValue/maxValue type mismatched. Property name: nominalCoolingCapacty, minValue: 0, maxValue: 10a",
    ///    "Asset.json": null
    ///  }
    /// }.
    /// </returns>
    [HttpPost("upload")]
    [Authorize(Policy = AppPermissions.CanUploadDQRules)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(RuleFileUploadResponse))]
    [ProducesResponseType(typeof(RuleFileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RuleFileUploadResponse>> UploadRuleFiles([FromForm] IEnumerable<IFormFile> formFiles)
        => await _dataQualityService.UploadRuleFilesAsync(formFiles);

    /// <summary>
    /// Get Data Quality Rules.
    /// </summary>
    /// <returns><see cref="GetRulesResponse"/>.</returns>
    [HttpGet("rules", Name = "getDQRules")]
    [Authorize(Policy = AppPermissions.CanReadDQRules)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(GetRulesResponse))]
    [ProducesResponseType(typeof(GetRulesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetRulesResponse>> GetDataQualityRules() =>
             await _dataQualityService.GetDataQualityRules();

    /// <summary>
    /// Delete all dataquality rules.
    /// </summary>
    /// <returns><see cref="OkResult"/>.</returns>
    [HttpDelete("DeleteAllDQRules", Name = "deleteAllDQRules")]
    [Authorize(Policy = AppPermissions.CanUploadDQRules)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllRules()
    {
        await _dataQualityRuleService.DeleteAllRules();

        return Ok();
    }
}
