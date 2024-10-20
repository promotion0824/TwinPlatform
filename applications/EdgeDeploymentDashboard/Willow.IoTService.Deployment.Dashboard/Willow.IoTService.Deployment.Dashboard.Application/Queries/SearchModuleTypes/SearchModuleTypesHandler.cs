namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModuleTypes;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public class SearchModuleTypesHandler(IModuleDataService dataService)
    : IRequestHandler<SearchModuleTypesQuery, PagedResult<SearchModuleTypeResponse>>
{
    public async Task<PagedResult<SearchModuleTypeResponse>> Handle(
        SearchModuleTypesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await dataService.GetModuleTypesAsync(
            new ModuleTypesSearchInput(
                request.ModuleType,
                request.Page,
                request.PageSize),
            cancellationToken);
        return new PagedResult<SearchModuleTypeResponse>
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new SearchModuleTypeResponse(x.moduleType, x.latestVersion)),
        };
    }
}
