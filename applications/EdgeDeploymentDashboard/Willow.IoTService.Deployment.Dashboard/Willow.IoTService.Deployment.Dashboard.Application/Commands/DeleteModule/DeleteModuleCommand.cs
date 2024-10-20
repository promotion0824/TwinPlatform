namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.DeleteModule;

using JetBrains.Annotations;
using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record DeleteModuleCommand : IRequest<DeletedModuleResponse>, IAuditLog
{
    public Guid? Id { get; set; }
}
