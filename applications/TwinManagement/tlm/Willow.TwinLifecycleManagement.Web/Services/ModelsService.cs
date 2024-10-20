using DTDLParser;
using DTDLParser.Models;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Models;

namespace Willow.TwinLifecycleManagement.Web.Services;

/// <summary>
/// Models Service Implementation.
/// </summary>
/// <param name="modelsClient">IModels Client.</param>
public class ModelsService(IModelsClient modelsClient) : IModelsService
{
    /// <summary>
    /// Get Model Interface Info by Id.
    /// </summary>
    /// <param name="modelId">Model Id.</param>
    /// <returns>DTInterfaceInfo.</returns>
    public async Task<DTInterfaceInfo> GetModelAsync(string modelId)
    {
        var response = await modelsClient.GetModelAsync(modelId, includeModelDefinitions: true);
        var parsedModel = ParseModels([response]);
        var interfaceModels = parsedModel.Select(dep => dep.Value).Where(m => m.EntityKind == DTEntityKind.Interface);
        return interfaceModels.FirstOrDefault() as DTInterfaceInfo;
    }

    /// <summary>
    /// Get the Model Family.
    /// </summary>
    /// <param name="rootModel">Root ModelId of the family.</param>
    /// <returns>Collection of InterfaceTwinsInfo.</returns>
    public async Task<List<InterfaceTwinsInfo>> GetModelFamilyAsync(string rootModel)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(rootModel);
        var models = await modelsClient.GetModelsAsync(rootModel: rootModel, includeModelDefinitions: false, includeTwinCount: false);
        var allModels = await GetModelsInterfaceInfoAsync();
        var modelFamily = new List<InterfaceTwinsInfo>();
        foreach (var model in models)
        {
            var matchModel = allModels.FirstOrDefault(m => m.Id == model.Id);
            if (matchModel is not null)
            {
                modelFamily.Add(matchModel);
            }
        }

        return modelFamily;
    }

    /// <summary>
    /// Get all model information.
    /// </summary>
    /// <param name="sourceType">Type Of Source to Fetch the information.</param>
    /// <returns>Collection of Models Twin Info.</returns>
    public async Task<List<ModelsTwinInfo>> GetModelsInfo(SourceType sourceType = SourceType.Adx)
    {
        var models = await modelsClient.GetModelsAsync(rootModel: null, includeModelDefinitions: true, includeTwinCount: true, sourceType: sourceType);
        return models.Select(model => new ModelsTwinInfo(model)).ToList();
    }

    /// <summary>
    /// Gets all the parsed models.
    /// </summary>
    /// <returns>IReadOnlyDictionary of Dtmi, DTEntityInfo.</returns>
    public async Task<IReadOnlyDictionary<Dtmi, DTEntityInfo>> GetParsedModelsAsync()
    {
        var models = await modelsClient.GetModelsAsync(rootModel: null, includeModelDefinitions: true, includeTwinCount: false);
        return ParseModels(models);
    }

    /// <summary>
    /// Get all models.
    /// </summary>
    /// <param name="sourceType">Type Of Source to Fetch the information.</param>
    /// <returns>Collection of Interface Twins Info.</returns>
    public async Task<List<InterfaceTwinsInfo>> GetModelsInterfaceInfoAsync(SourceType sourceType = SourceType.Adx)
    {
        var models = await modelsClient.GetModelsAsync(rootModel: null, includeModelDefinitions: true, includeTwinCount: true, sourceType: sourceType);

        var allModels = ParseModels(models);
        var interfaceModels = allModels.Select(dep => dep.Value).Where(m => m.EntityKind == DTEntityKind.Interface);

        List<InterfaceTwinsInfo> interfaceTwinsInfos = [];
        foreach (var entityModel in interfaceModels)
        {
            var modelResponse = models.FirstOrDefault(mr => mr.Id == entityModel.Id.AbsoluteUri);
            if (modelResponse != null)
            {
                interfaceTwinsInfos.Add(new InterfaceTwinsInfo(modelResponse, entityModel));
            }
        }

        return interfaceTwinsInfos;
    }

    /// <summary>
    /// Upgrade Models from Git.
    /// </summary>
    /// <param name="gitInfo">UpgradeModelsRepoRequest instance. </param>
    /// <param name="userData">string user data.</param>
    /// <param name="userId">string user id.</param>
    /// <returns>AdtBulkImportJob.</returns>
    public async Task<JobsEntry> PostModelsFromGitAsync(UpgradeModelsRepoRequest gitInfo, string userData, string userId)
    {
        var gitInfList = new List<UpgradeModelsRepoRequest>() { gitInfo };
        return await modelsClient.UpgradeFromReposAsync(gitInfList, userId, userData);
    }

    private static IReadOnlyDictionary<Dtmi, DTEntityInfo> ParseModels(IEnumerable<ModelResponse> modelsResponse)
    {
        // Parse call occasionally causes non-concurrent collection exception
        lock (typeof(ModelsService))
        {
            var modelJsons = modelsResponse
                                .Select(mr => mr.Model)
                                .ToList();
            var parser = new ModelParser();

            // ReSharper disable once MethodHasAsyncOverload
            return parser.Parse(modelJsons);
        }
    }
}
