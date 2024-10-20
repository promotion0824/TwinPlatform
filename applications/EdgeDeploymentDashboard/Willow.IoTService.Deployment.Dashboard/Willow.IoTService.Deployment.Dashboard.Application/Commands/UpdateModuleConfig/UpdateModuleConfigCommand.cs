namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.UpdateModuleConfig;

using System.ComponentModel;
using JetBrains.Annotations;
using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.DataAccess.Entities;
using Willow.IoTService.Deployment.DataAccess.Services;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class UpdateModuleConfigCommand : IRequest<ModuleDto>, IAuditLog
{
    public Guid Id { get; init; }

    public string? DeviceName { get; init; }

    public string? IoTHubName { get; init; }

    public Platforms? Platform { get; init; }

    [DefaultValue(false)]
    public bool? IsAutoDeploy { get; init; }

    [DefaultValue("{}")]
    public string? Environment { get; init; }
}
