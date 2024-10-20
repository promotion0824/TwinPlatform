using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Web.Controllers;

/// <summary>
/// Application Controller.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApplicationController(IApplicationManager applicationManager) : ControllerBase
{
    [Authorize(AppPermissions.CanReadApplication)]
    [HttpGet]
    public async Task<IEnumerable<ApplicationModel>> GetApplications([FromQuery] FilterPropertyModel filterModel)
    {
        return await applicationManager.GetApplications(filterModel);
    }

    [Authorize(AppPermissions.CanReadApplication)]
    [HttpGet("{name}")]
    public async Task<ActionResult<ApplicationModel>> GetApplicationByName(string name)
    {
        var response = await applicationManager.GetApplicationByName(name);

        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return response.Result;
    }

    [Authorize(AppPermissions.CanReadApplicationClient)]
    [HttpGet("{applicationName}/clients")]
    public async Task<ActionResult<List<ApplicationClientModel>>> GetApplicationClients(string applicationName, [FromQuery] FilterPropertyModel filter)
    {
        var response = await applicationManager.GetClientsByApplicationName(applicationName, filter);
        return response;
    }

    [Authorize(AppPermissions.CanCreateApplicationClient)]
    [HttpPost("clients")]
    public async Task<ActionResult<ApplicationClientModel>> AddApplicationClient(ApplicationClientModel clientModel)
    {
        var response = await applicationManager.AddApplicationClient(clientModel);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok(response.Result);
    }

    [Authorize(AppPermissions.CanEditApplicationClient)]
    [HttpPut("clients")]
    public async Task<ActionResult<ApplicationClientModel>> EditApplicationClient(ApplicationClientModel clientModel)
    {
        var response = await applicationManager.UpdateApplicationClient(clientModel);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok(response.Result);
    }

    [Authorize(AppPermissions.CanEditApplicationClient)]
    [HttpPost("{applicationName}/clients/{clientName}")]
    public async Task<ActionResult<ClientAppPasswordCredential>> GenerateClientSecret(string applicationName, string clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            return ValidationProblem("Client Name cannot be empty.");
        }
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            return ValidationProblem("Application Name cannot be empty.");
        }
        var response = await applicationManager.RegenerateClientSecret(applicationName, clientName);

        if (response.FailedAuthorization)
        {
            return Unauthorized(response);
        }

        if (response.Result == null)
        {
            return NotFound();
        }

        return response.Result;
    }


    [Authorize(AppPermissions.CanDeleteApplicationClient)]
    [HttpDelete("clients/{applicationClientId}")]
    public async Task<ActionResult> DeleteApplicationClient([FromRoute] string applicationClientId)
    {
        await applicationManager.DeleteApplicationClient(applicationClientId);
        return NoContent();
    }

    [Authorize(AppPermissions.CanReadApplicationClient)]
    [HttpGet("clients/credentials")]
    public async Task<ActionResult<Dictionary<string, ClientAppPasswordCredential?>>> GetClientCredentials([FromQuery] List<string> clientIds)
    {
        return await applicationManager.GetActiveCredentialsByClientIds(clientIds);
    }
}
