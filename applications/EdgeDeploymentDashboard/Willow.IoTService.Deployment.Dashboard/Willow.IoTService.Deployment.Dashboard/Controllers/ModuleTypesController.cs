namespace Willow.IoTService.Deployment.Dashboard.Controllers;

using System.Net;
using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleType;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeTemplate;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeVersions;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModuleTypes;
using Willow.IoTService.Deployment.DataAccess.Services;

[Authorize(Policy = "CanReadEdgeDeployments")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ModuleTypesController(ISender mediator) : ControllerBase
{
    [HttpGet("search")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(PagedResult<SearchModuleTypeResponse>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SearchModuleTypes([FromQuery] SearchModuleTypesQuery request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Ok(response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateTemplate([FromForm] CreateModuleTypeCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
        return this.Created(
                            $"~/api/{this.GetApiPath()}?moduleType={request.ModuleType}&version={request.Version}",
                            new
                            {
                                request.ModuleType,
                                request.Version,
                            });
    }

    [HttpGet]
    [SwaggerResponse((int)HttpStatusCode.OK)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DownloadTemplate([FromQuery] GetModuleTypeTemplateQuery request, CancellationToken cancellationToken)
    {
        var (fileName, stream) = await mediator.Send(request, cancellationToken);
        return this.File(
                         stream,
                         MediaTypeNames.Application.Json,
                         fileName);
    }

    [HttpGet("versions")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<string>))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetModuleTypeVersions([FromQuery] GetModuleTypeVersionsQuery request, CancellationToken cancellationToken)
    {
        var items = await mediator.Send(request, cancellationToken);
        return this.Ok(items);
    }
}
