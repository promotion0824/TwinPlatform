namespace Willow.IoTService.Deployment.DataAccess.Services;

public record DeploymentSearchInput(
    IEnumerable<Guid>? Ids = null,
    Guid? ModuleId = null,
    string? DeviceName = null,
    int Page = 1,
    int PageSize = 50) : PagedQuery(Page, PageSize);
