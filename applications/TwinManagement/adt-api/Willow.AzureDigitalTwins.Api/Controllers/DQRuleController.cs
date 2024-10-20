using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.DataQuality.Model.Responses;

namespace Willow.AzureDigitalTwins.DataQuality.Api.Controllers;

/// <summary>
/// Data Quality controller to upload and download rule files
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class DQRuleController : Controller
{
    private readonly IDQRuleService _ruleService;

    public DQRuleController(IDQRuleService ruleService)
    {
        _ruleService = ruleService;
    }


    /// <summary>
    /// Download file by name
    /// </summary>
    [HttpGet("download", Name = "DownloadRuleFileAsync")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    // TODO: This should probably be keyed off of the ID, not the name - or allow either option
    public async Task<IActionResult> DownloadRuleFile([FromQuery] string name)
    {
        var stream = await _ruleService.DownloadRuleFileAsync(name);

        if (stream == null)
        {
            return NotFound(new ProblemDetails { Detail = "File not found" });
        }

        return File(stream, "application/json");
    }


    /// <summary>
    /// Delete file by Rule Id
    /// Sample request: https://localhost:8001/dqrule/delete?ruleId=Asset-Validation
    /// </summary>
    [HttpDelete("delete", Name = "DeleteRuleFileAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRuleFile([FromQuery] string ruleId)
    {
        var deleted = await _ruleService.DeleteRuleFileAsync(ruleId);

        if (!deleted)
        {
            return NotFound(new ProblemDetails { Detail = "Rule file not found" });
        }

        return Ok();
    }

    /// <summary>
    /// Delete all Rules files
    /// Sample request: https://localhost:8001/dqrule/deleteall
    /// </summary>
    [HttpDelete("deleteall", Name = "DeleteAllRulesFileAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllRules()
    {
        await _ruleService.DeleteAllRulesFileAsync();

        return Ok();
    }

    /// <summary>
    /// Upload one or more rules in JSON HTTP form files.
    /// Rules are stored by the "Id" specified in the rule-template, not by the filename that contais the rule 
    ///   it's up to the caller to make sure that multiple files don't redefine the same rule -
    ///   the last one in wins.
    /// </summary>
    [HttpPost("upload", Name = "UploadRuleFiles")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleFileUploadResponse))]
    public async Task<ActionResult<RuleFileUploadResponse>> UploadRuleFiles([Required] IEnumerable<IFormFile> files)
    {
        var response = new RuleFileUploadResponse();
        foreach (var file in files.Where(v => v.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            var loadedResult = await _ruleService.UploadRuleFileAsync(file.FileName, stream);
            // FileUploaded string: null==OK, any other string is an error message
            response.FileUploaded[file.FileName] = loadedResult;
        }

        return response;
    }

    /// <summary>
    /// Get all the data quality rules. Note that these are pre-loaded so the results
    ///   will be returned immediately.
    /// </summary>
    /// <returns>A list of data quality rules</returns>
    [HttpGet("rules", Name = "GetDataQualityRules")]
    [ProducesResponseType(typeof(GetRulesResponse), StatusCodes.Status200OK)]
    // Note that we don't have a corresponding way to PUT DQ rules directly - we only upload by file stream -
    //   we can add if TLM implements its own rule editor
    public async Task<ActionResult<GetRulesResponse>> GetDataQualityRules()
    {
        return new GetRulesResponse
        {
            Rules = await _ruleService.GetValidationRules()
        };
    }

}

