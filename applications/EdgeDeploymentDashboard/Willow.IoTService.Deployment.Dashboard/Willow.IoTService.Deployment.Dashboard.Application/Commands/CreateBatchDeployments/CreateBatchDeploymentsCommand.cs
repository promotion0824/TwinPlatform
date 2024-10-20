namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateBatchDeployments;

using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;

public record CreateBatchDeploymentsCommand(IEnumerable<CreateDeploymentCommand> CreateDeploymentCommands) : IRequest<IEnumerable<Guid>>, IAuditLog;
