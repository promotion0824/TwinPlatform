namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeVersions;

using MediatR;

public record GetModuleTypeVersionsQuery(string ModuleType) : IRequest<IEnumerable<string>>;
