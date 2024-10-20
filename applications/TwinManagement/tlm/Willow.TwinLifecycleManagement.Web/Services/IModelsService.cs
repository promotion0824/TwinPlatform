using DTDLParser;
using DTDLParser.Models;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Models;

namespace Willow.TwinLifecycleManagement.Web.Services;

/// <summary>
/// Models Service Contract.
/// </summary>
public interface IModelsService
{
    /// <summary>
    /// Get Model Interface Info by Id.
    /// </summary>
    /// <param name="modelId">Model Id.</param>
    /// <returns>DTInterfaceInfo.</returns>
    Task<DTInterfaceInfo> GetModelAsync(string modelId);

    /// <summary>
    /// Get the Model Family.
    /// </summary>
    /// <param name="rootModel">Root ModelId of the family.</param>
    /// <returns>Collection of InterfaceTwinsInfo.</returns>
    Task<List<InterfaceTwinsInfo>> GetModelFamilyAsync(string rootModel);

    /// <summary>
    /// Get all model information.
    /// </summary>
    /// <param name="sourceType">Type Of Source to Fetch the information.</param>
    /// <returns>Collection of Models Twin Info.</returns>
    Task<List<ModelsTwinInfo>> GetModelsInfo(SourceType sourceType = SourceType.Adx);

    /// <summary>
    /// Gets all the parsed models.
    /// </summary>
    /// <returns>IReadOnlyDictionary of Dtmi, DTEntityInfo.</returns>
    Task<IReadOnlyDictionary<Dtmi, DTEntityInfo>> GetParsedModelsAsync();

    /// <summary>
    /// Get all models.
    /// </summary>
    /// <param name="sourceType">Type Of Source to Fetch the information.</param>
    /// <returns>Collection of Interface Twins Info.</returns>
    Task<List<InterfaceTwinsInfo>> GetModelsInterfaceInfoAsync(SourceType sourceType = SourceType.Adx);

    Task<JobsEntry> PostModelsFromGitAsync(UpgradeModelsRepoRequest gitInfo, string userData, string userId);
}
