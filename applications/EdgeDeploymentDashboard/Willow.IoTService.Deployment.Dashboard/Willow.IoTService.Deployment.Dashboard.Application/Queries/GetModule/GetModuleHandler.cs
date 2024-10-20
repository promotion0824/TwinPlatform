namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModule;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class GetModuleHandler(IModuleDataService moduleDataService) : IRequestHandler<GetModuleQuery, ModuleDto>
{
    public async Task<ModuleDto> Handle(GetModuleQuery request, CancellationToken cancellationToken)
    {
        return await moduleDataService.GetAsync(request.Id, cancellationToken) ?? throw new NotFoundException("Module not found");
    }
}
