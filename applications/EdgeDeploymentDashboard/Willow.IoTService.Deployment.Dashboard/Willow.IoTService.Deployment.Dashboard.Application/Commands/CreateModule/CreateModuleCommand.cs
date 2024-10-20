namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModule;

using System.ComponentModel;
using JetBrains.Annotations;
using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.DataAccess.Services;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record CreateModuleCommand : IRequest<ModuleDto>, IAuditLog
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Gets application type. It can be ignored and can be empty when IsBaseModule is true. Otherwise, must not be empty.
    /// </summary>
    public string? ApplicationType { get; init; }

    [DefaultValue(false)]
    public bool IsBaseModule { get; init; }

    public void Deconstruct(
        out string name,
        out string? applicationType,
        out bool isBaseModule)
    {
        name = this.Name;
        applicationType = this.ApplicationType;
        isBaseModule = this.IsBaseModule;
    }
}
