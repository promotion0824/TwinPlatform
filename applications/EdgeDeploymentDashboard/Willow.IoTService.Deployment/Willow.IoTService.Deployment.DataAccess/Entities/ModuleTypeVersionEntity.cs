namespace Willow.IoTService.Deployment.DataAccess.Entities;

public class ModuleTypeVersionEntity : BaseEntity
{
    public Guid Id { get; init; }
    public string ModuleType { get; init; } = "";
    public int? Major { get; init; }
    public int? Minor { get; init; }
    public int? Patch { get; init; }
}
