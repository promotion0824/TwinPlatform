namespace Willow.IoTService.Deployment.DataAccess.Entities;

public class ModuleConfigEntity : BaseEntity
{
    public Guid ModuleId { get; init; }
    public bool IsAutoDeployment { get; set; }
    public string DeviceName { get; set; } = "";
    public string IoTHubName { get; set; } = "";
    public string Environment { get; set; } = "";
    public Platforms Platform { get; set; }

    public ModuleEntity Module { get; set; } = null!;
}

public enum Platforms
{
    arm64v8,
    arm32v7,
    amd64
}
