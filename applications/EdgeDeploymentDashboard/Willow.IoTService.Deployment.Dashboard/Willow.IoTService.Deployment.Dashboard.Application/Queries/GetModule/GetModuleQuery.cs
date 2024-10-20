namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModule;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public record GetModuleQuery(Guid Id) : IRequest<ModuleDto>;
