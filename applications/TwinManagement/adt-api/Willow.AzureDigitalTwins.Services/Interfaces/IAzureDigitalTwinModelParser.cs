using Azure.DigitalTwins.Core;
using DTDLParser.Models;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IAzureDigitalTwinModelParser
{
    IReadOnlyDictionary<string, DTInterfaceInfo> GetInterfaceDescendants(IEnumerable<string> rootModelIds);
    bool IsDescendantOf(string rootModelId, string modelId);
    bool IsDescendantOfAny(IEnumerable<string> rootModelIds, string modelId);
    Task<IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>>> TopologicalSort(IEnumerable<DigitalTwinsModelBasicData> models);
    void EnsureRequiredFields(BasicDigitalTwin twin);
    DTInterfaceInfo GetInterfaceInfo(string modelId);
    public HashSet<string> GetAllAncestors(string modelId);
}
