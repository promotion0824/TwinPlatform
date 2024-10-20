namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Ardalis.GuardClauses;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Willow.IoTService.Deployment.Common.Messages;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.Deployment.ManifestStorage;

/// <inheritdoc />
public class ModuleConfigContentService(
    ILogger<ModuleConfigContentService> logger,
    IManifestStorageService manifestStorageService,
    IDefaultTransformer defaultTransformer,
    IEdgeConnectorTransformer edgeConnectorTransformer,
    IBaseModuleTransformer baseModuleTransformer,
    IOptions<WillowContainerRegistry> options)
    : IModuleConfigContentService
{
    private const string CrUsernamePlaceholder = "${ACR_USER}";
    private const string CrPasswordPlaceholder = "${ACR_PASSWORD}";
    private const string CrAddressPlaceholder = "${ACR_ADDRESS}";
    private const string CrUsernamePlaceholder2 = "${ACR_USER_2}";
    private const string CrPasswordPlaceholder2 = "${ACR_PASSWORD_2}";
    private const string CrAddressPlaceholder2 = "${ACR_ADDRESS_2}";
    private const string BaseModuleVersionPlaceholder = "${IOTEDGE_VERSION}";
    private const string BaseTemplateFileName = "deployment.template.base.json";
    private readonly WillowContainerRegistry registryOptions = options.Value;

    /// <inheritdoc />
    public async Task<ConfigurationContent> GetContent(GetConfigContentRequest request, CancellationToken cancellationToken)
    {
        var content = request.IsBaseDeployment switch
        {
            true => await this.GetBaseManifestAsync(request.Version, cancellationToken),
            false => await this.GetLayeredManifestAsync(request, cancellationToken),
        };

        return content;
    }

    private async Task<ConfigurationContent> GetBaseManifestAsync(string version, CancellationToken cancellationToken)
    {
        var templateString = await File.ReadAllTextAsync(BaseTemplateFileName, cancellationToken);
        var template = templateString.Replace(CrUsernamePlaceholder, this.registryOptions.Username)
                                     .Replace(CrPasswordPlaceholder, this.registryOptions.Password)
                                     .Replace(CrAddressPlaceholder, this.registryOptions.Address)
                                     .Replace(CrUsernamePlaceholder2, this.registryOptions.Username2)
                                     .Replace(CrPasswordPlaceholder2, this.registryOptions.Password2)
                                     .Replace(CrAddressPlaceholder2, this.registryOptions.Address2)
                                     .Replace(BaseModuleVersionPlaceholder, version);

        var content = JsonConvert.DeserializeObject<ConfigurationContent>(template) ?? throw new JsonException("Failed to deserialize content");
        baseModuleTransformer.Transform(content);

        return content;
    }

    private async Task<ConfigurationContent> GetLayeredManifestAsync(GetConfigContentRequest request, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(request.Module.Platform?.ToString());
        logger.LogDebug("Downloading template then applying transformation");

        // download deployment template
        var templateType = request.IsBaseDeployment
                               ? manifestStorageService.BaseDeploymentTemplateName
                               : request.Module.ModuleType;

        var (_, _, stream) = await manifestStorageService.DownloadTemplateAsync(
                                                                                templateType,
                                                                                request.Version,
                                                                                cancellationToken);

        await using var templateStream = stream;

        // we need to use Newtonsoft.Json because the Device library is using it
        using var sr = new StreamReader(templateStream);
        var templateString = await sr.ReadToEndAsync(cancellationToken);
        var jObject = JsonConvert.DeserializeObject<JObject>(templateString) ?? throw new JsonException("Failed to deserialize content");
        if (!jObject.TryGetValue("content", out var jToken))
        {
            jToken = jObject;
        }

        var content = jToken.ToObject<ConfigurationContent>() ?? throw new JsonException("Failed to deserialize content");

        if (edgeConnectorTransformer.CanTransform(request.Module.ModuleType))
        {
            await edgeConnectorTransformer.TransformAsync(
                                                          new EdgeConnectorTransformConfig(
                                                                                           content,
                                                                                           request.Module.Id.ToString(),
                                                                                           request.Module.Name,
                                                                                           request.Module.Platform.ToString()!,
                                                                                           request.Module.ModuleType,
                                                                                           request.Module.Environment,
                                                                                           request.ContainerConfigs ?? new Dictionary<string, IContainerConfiguration>()),
                                                          cancellationToken);
        }
        else
        {
            defaultTransformer.Transform(content, request.ContainerConfigs);
        }

        return content;
    }
}

/// <summary>
///     Request object for getting the module configuration content.
/// </summary>
/// <param name="Module">Deployment Module.</param>
/// <param name="Version">Deployment Module Template Version.</param>
public record GetConfigContentRequest(ModuleDto Module, string Version)
{
    /// <summary>
    ///     Gets a value indicating whether gets if the request is for a base deployment.
    /// </summary>
    /// <remarks>True indicates base deployment; False indicates custom module deployment.</remarks>
    public bool IsBaseDeployment { get; init; }

    /// <summary>
    ///     Gets a dictionary of container configurations for the deployment.
    /// </summary>
    public IReadOnlyDictionary<string, IContainerConfiguration>? ContainerConfigs { get; init; }
}
