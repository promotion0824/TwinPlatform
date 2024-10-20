namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeTemplate;

using MediatR;

public record GetModuleTypeTemplateQuery(string ModuleType, string Version) : IRequest<(string FileName, Stream Content)>;
