namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeTemplate;

using MediatR;
using Willow.IoTService.Deployment.ManifestStorage;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class GetModuleTypeTemplateHandler(IManifestStorageService manifestStorageService) : IRequestHandler<GetModuleTypeTemplateQuery, (string FileName, Stream Content)>
{
    public async Task<(string FileName, Stream Content)> Handle(GetModuleTypeTemplateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (containerName, blobName, content) = await manifestStorageService.DownloadTemplateAsync(request.ModuleType,
                                                                                                        request.Version,
                                                                                                        cancellationToken);
            return ($"{containerName}-{blobName}", content);
        }
        catch (TemplateNotFoundException e)
        {
            throw new NotFoundException($"Template not found for module type {request.ModuleType} and version {request.Version}", e);
        }
    }
}
