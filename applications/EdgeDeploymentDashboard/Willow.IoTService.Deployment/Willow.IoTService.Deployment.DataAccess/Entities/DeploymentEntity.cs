using JetBrains.Annotations;

namespace Willow.IoTService.Deployment.DataAccess.Entities;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DeploymentEntity : BaseEntity
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string AssignedBy { get; init; } = "";
    public string Status { get; set; } = "";
    public string StatusMessage { get; set; } = "";
    public DateTimeOffset DateTimeApplied { get; set; }

    public ModuleEntity Module { get; set; } = null!;
}
