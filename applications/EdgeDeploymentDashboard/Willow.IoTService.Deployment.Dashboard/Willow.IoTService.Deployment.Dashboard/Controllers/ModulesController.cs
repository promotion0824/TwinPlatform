namespace Willow.IoTService.Deployment.Dashboard.Controllers;

using System.Net;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModule;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.UpdateModuleConfig;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModule;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModules;
using Willow.IoTService.Deployment.DataAccess.Services;

[ServiceToServiceOrDefaultAuthorize("CanReadEdgeDeployments")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ModulesController(ISender mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ModuleDto))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetModule([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetModuleQuery(id), cancellationToken);
        return this.Ok(response);
    }

    [HttpGet("search")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(PagedResult<ModuleDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SearchModules([FromQuery] SearchModulesQuery request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Ok(response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, type: typeof(ModuleDto))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateModule([FromBody] CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Created($"~/api/{this.GetApiPath()}/{response.Id}", response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPut("deployment-configs")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(ModuleDto))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateModuleConfig([FromBody] UpdateModuleConfigCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Ok(response);
    }
}
