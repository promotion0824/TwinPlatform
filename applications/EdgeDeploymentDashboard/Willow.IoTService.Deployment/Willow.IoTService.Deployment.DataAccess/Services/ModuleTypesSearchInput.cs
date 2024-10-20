namespace Willow.IoTService.Deployment.DataAccess.Services;

public record ModuleTypesSearchInput(
    string? ModuleType = null,
    int Page = 1,
    int PageSize = 50) : PagedQuery(Page, PageSize);
