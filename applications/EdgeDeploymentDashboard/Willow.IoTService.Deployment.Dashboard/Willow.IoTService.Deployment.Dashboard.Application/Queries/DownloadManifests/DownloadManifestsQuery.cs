namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.DownloadManifests;

using JetBrains.Annotations;
using MediatR;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record DownloadManifestsQuery : IRequest<Stream>
{
    /// <summary>
    ///     Gets the list of Deployment Ids. length should be 1 to 10.
    /// </summary>
    public IEnumerable<Guid> DeploymentIds { get; init; } = new List<Guid>(0);
}
