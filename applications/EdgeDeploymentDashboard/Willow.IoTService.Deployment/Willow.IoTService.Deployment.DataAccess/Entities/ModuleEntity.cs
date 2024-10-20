using JetBrains.Annotations;

namespace Willow.IoTService.Deployment.DataAccess.Entities;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class ModuleEntity : BaseEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = "";
    public string ModuleType { get; set; } = "";
    public bool IsArchived { get; set; }
    public bool IsSynced { get; set; }

    public ModuleConfigEntity? Config { get; set; }
    public IEnumerable<DeploymentEntity>? Deployments { get; set; }
}
