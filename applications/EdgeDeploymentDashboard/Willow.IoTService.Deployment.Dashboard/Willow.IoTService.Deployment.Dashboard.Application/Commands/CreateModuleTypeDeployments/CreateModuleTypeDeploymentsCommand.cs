namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleTypeDeployments;

using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;

public record CreateModuleTypeDeploymentsCommand(string ModuleType, string Version) : IRequest<CreateModuleTypeDeploymentsResponse>, IAuditLog;
