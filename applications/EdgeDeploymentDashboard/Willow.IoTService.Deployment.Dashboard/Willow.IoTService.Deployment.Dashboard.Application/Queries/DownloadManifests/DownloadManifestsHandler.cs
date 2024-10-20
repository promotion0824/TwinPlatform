namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.DownloadManifests;

using System.IO.Compression;
using Azure;
using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.Deployment.ManifestStorage;

public class DownloadManifestsHandler(IManifestStorageService storageService, IModuleDataService moduleDataService)
    : IRequestHandler<DownloadManifestsQuery, Stream>
{
    private const string DeploymentErrorMessage =
        "Cannot download manifest for module id {0} name {1}, deployment id {2} name {3}: {4}";

    public async Task<Stream> Handle(DownloadManifestsQuery request, CancellationToken cancellationToken)
    {
        var requestedDeploymentIds = request.DeploymentIds.ToDictionary(x => x, _ => string.Empty);
        var requestId = Guid.NewGuid();
        var rootPath = Path.Combine(Path.GetTempPath(), "manifests");
        Directory.CreateDirectory(rootPath);
        var zipPath = Path.Combine(rootPath, $"{requestId}.zip");

        var fs = new FileStream(
            zipPath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None,
            4096,
            FileOptions.DeleteOnClose);
        using var zip = new ZipArchive(
            fs,
            ZipArchiveMode.Create,
            true);
        var paged = await moduleDataService.SearchAsync(
            new ModuleSearchInput(
                DeploymentIds: requestedDeploymentIds.Keys,
                Page: 1,
                PageSize: 10),
            cancellationToken);
        var errorMessages = new List<string>();
        foreach (var (module, deployments) in paged.Items)
        {
            var deploymentList = deployments?.ToList();
            if (deploymentList == null)
            {
                continue;
            }

            // remove the deployment id from the list of requested deployment ids
            // the left ones are the ones that are not in db
            foreach (var id in deploymentList.Select(x => x.Id))
            {
                requestedDeploymentIds.Remove(id);
            }

            if (string.IsNullOrWhiteSpace(module.Environment))
            {
                errorMessages.AddRange(
                    deploymentList.Select(
                        deployment => string.Format(
                            DeploymentErrorMessage,
                            module.Id,
                            module.Name,
                            deployment.Id,
                            deployment.Name,
                            "Environment string is null or empty")));

                continue;
            }

            var moduleErrors = await this.DownloadManifestsAsync(
                deploymentList,
                module,
                zip,
                cancellationToken);

            errorMessages.AddRange(moduleErrors);
        }

        errorMessages.AddRange(requestedDeploymentIds.Select(x => $"Deployment not found for id {x.Key}"));

        await WriteErrorLogAsync(errorMessages, zip);

        return fs;
    }

    private async Task<IEnumerable<string>> DownloadManifestsAsync(
        IEnumerable<DeploymentDto> deployments,
        ModuleDto module,
        ZipArchive zip,
        CancellationToken cancellationToken)
    {
        var errorMessages = new List<string>();
        foreach (var deployment in deployments)
        {
            Stream content;
            try
            {
                var (_, _, stream) = await storageService.DownloadManifestAsync(
                    deployment.Id,
                    cancellationToken);
                content = stream;
            }
            catch (Exception e) when (e is ManifestStorageServiceException or RequestFailedException)
            {
                // manifest not exist is included here
                errorMessages.Add(
                    string.Format(
                        DeploymentErrorMessage,
                        module.Id,
                        module.Name,
                        deployment.Id,
                        deployment.Name,
                        e.Message));
                continue;
            }

            var entry = zip.CreateEntry($"{deployment.Id}.json");
            await using var entryStream = entry.Open();
            await content.CopyToAsync(entryStream, cancellationToken);
            await content.DisposeAsync();
        }

        return errorMessages;
    }

    private static async Task WriteErrorLogAsync(List<string> errorMessages, ZipArchive zip)
    {
        if (errorMessages.Any())
        {
            var entry = zip.CreateEntry("errors.log");
            await using var entryStream = entry.Open();
            await using var writer = new StreamWriter(entryStream);
            foreach (var errorMessage in errorMessages)
            {
                await writer.WriteLineAsync(errorMessage);
            }

            await writer.FlushAsync();
        }
    }
}
