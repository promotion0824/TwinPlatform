namespace Willow.IoTService.Deployment.DataAccess.Entities;

public abstract class BaseEntity
{
    public string CreatedBy { get; set; } = "";
    public string UpdatedBy { get; set; } = "";
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
