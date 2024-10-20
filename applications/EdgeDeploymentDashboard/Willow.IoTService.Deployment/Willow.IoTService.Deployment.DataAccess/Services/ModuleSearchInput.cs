namespace Willow.IoTService.Deployment.DataAccess.Services;

public record ModuleSearchInput(
    string? Name = null,
    string? ModuleType = null,
    IEnumerable<Guid>? DeploymentIds = null,
    string? DeviceName = null,
    bool? IsArchived = null,
    int Page = 1,
    int PageSize = 50) : PagedQuery(Page, PageSize);
