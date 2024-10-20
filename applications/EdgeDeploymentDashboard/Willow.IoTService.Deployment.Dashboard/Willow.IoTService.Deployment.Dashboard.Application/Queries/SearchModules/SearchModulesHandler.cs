namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModules;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public class SearchModulesHandler(IModuleDataService moduleService)
    : IRequestHandler<SearchModulesQuery, PagedResult<ModuleDto>>
{
    public async Task<PagedResult<ModuleDto>> Handle(SearchModulesQuery request, CancellationToken cancellationToken)
    {
        var result = await moduleService.SearchAsync(
            new ModuleSearchInput(
                request.Name,
                request.ModuleType,
                request.DeploymentIds,
                request.DeviceName,
                request.IsArchived,
                request.Page,
                request.PageSize),
            cancellationToken);

        var response = new PagedResult<ModuleDto>
        {
            Items = result.Items.Select(x => x.Module),
            TotalCount = result.TotalCount,
        };

        return response;
    }
}
