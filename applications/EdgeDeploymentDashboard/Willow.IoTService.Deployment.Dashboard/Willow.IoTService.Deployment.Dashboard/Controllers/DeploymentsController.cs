namespace Willow.IoTService.Deployment.Dashboard.Controllers;

using System.Net;
using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateBatchDeployments;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleTypeDeployments;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.DownloadManifests;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.GetDeployment;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchDeployments;
using Willow.IoTService.Deployment.DataAccess.Services;

[Authorize(Policy = "CanReadEdgeDeployments")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DeploymentsController(ISender mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(DeploymentDto))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetDeployment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetDeploymentQuery(id), cancellationToken);
        return this.Ok(response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPost]
    [SwaggerResponse((int)HttpStatusCode.Created, type: typeof(DeploymentDto))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> CreateDeployment([FromBody] CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);

        return this.Created($"~/api/{this.GetApiPath()}/{response.Id}", response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPost("batch")]
    [SwaggerResponse((int)HttpStatusCode.Created, type: typeof(IEnumerable<Guid>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateBatchDeployment([FromBody] CreateBatchDeploymentsCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        var queryString = response.Aggregate("ids=", (current, id) => current + $"{id},");
        return this.Created($"~/api/{this.GetApiPath()}?{queryString}", response);
    }

    [Authorize(Policy = "CanWriteEdgeDeployments")]
    [HttpPost("byModuleType")]
    [SwaggerResponse((int)HttpStatusCode.Created, type: typeof(CreateModuleTypeDeploymentsResponse))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> CreateModuleTypeDeployments([FromBody] CreateModuleTypeDeploymentsCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Created($"~/api/{this.GetApiPath()}", response);
    }

    [HttpGet("search")]
    [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(PagedResult<DeploymentDto>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SearchDeployments([FromQuery] SearchDeploymentsQuery request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return this.Ok(response);
    }

    [HttpGet("manifests")]
    [SwaggerResponse((int)HttpStatusCode.OK)]
    [SwaggerResponse((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Download([FromQuery] DownloadManifestsQuery request, CancellationToken cancellationToken)
    {
        var stream = await mediator.Send(request, cancellationToken);
        stream.Position = 0;
        return this.File(
                         stream,
                         MediaTypeNames.Application.Zip,
                         "manifests.zip");
    }
}
