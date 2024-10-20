namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeVersions;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public class GetModuleTypeVersionsHandler(IModuleDataService moduleDataService) : IRequestHandler<GetModuleTypeVersionsQuery, IEnumerable<string>>
{
    public Task<IEnumerable<string>> Handle(GetModuleTypeVersionsQuery request, CancellationToken cancellationToken)
    {
        return moduleDataService.GetModuleTypeVersionsAsync(request.ModuleType, cancellationToken);
    }
}
