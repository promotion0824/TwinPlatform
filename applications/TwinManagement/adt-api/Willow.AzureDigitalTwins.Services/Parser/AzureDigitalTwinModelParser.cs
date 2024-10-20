using Azure.DigitalTwins.Core;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Parser;

public class AzureDigitalTwinModelParser : IAzureDigitalTwinModelParser
{
    private readonly ILogger<AzureDigitalTwinModelParser> _logger;

    protected IAzureDigitalTwinCacheProvider AzureDigitalTwinCacheProvider { get; }

    protected IAzureDigitalTwinCache AzureDigitalTwinCache => AzureDigitalTwinCacheProvider.GetOrCreateCache();

    public AzureDigitalTwinModelParser(IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
                                       ILogger<AzureDigitalTwinModelParser> logger)
    {
        AzureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
        _logger = logger;
    }

    public Task<IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>>> TopologicalSort(IEnumerable<DigitalTwinsModelBasicData> models)
    {
        var parser = new ModelParser();
        var modelData = parser.Parse(models.Select(x => x.DtdlModel));

        var interfaces = new List<DTInterfaceInfo>();
        var interfacesById = (from entity in modelData.Values
                              where entity.EntityKind == DTEntityKind.Interface
                              select entity as DTInterfaceInfo).ToDictionary(x => x.Id, x => x);

        interfaces.AddRange(interfacesById.Values);
        var processedIds = new List<string>();
        var modelsMap = new Dictionary<DigitalTwinsModelBasicData, IEnumerable<string>>();

        // Ensure all interfaces are processed, while loop is meant to process all dependencies
        while (interfacesById.Count > 0)
        {
            foreach (DTInterfaceInfo dtInterface in interfaces.Where(x => !processedIds.Contains(x.Id.ToString())))
            {
                var extendDependenciesSatisfied = !dtInterface.Extends.Any() || dtInterface.Extends.All(x => processedIds.Contains(x.Id.ToString()));
                var components = from content in dtInterface.Contents.Values
                                 where content.EntityKind == DTEntityKind.Component
                                 select content as DTComponentInfo;

                var componentDependenciesSatisfied = !components.Any() || components.All(x => processedIds.Contains(x.Schema.Id.ToString()));

                if (componentDependenciesSatisfied && extendDependenciesSatisfied)
                {
                    modelsMap.Add(models.Single(x => x.Id == dtInterface.Id.ToString()), new List<string>(dtInterface.Extends.Select(x => x.Id.ToString()).Union(components.Select(x => x.Schema.Id.ToString()))));
                    interfacesById.Remove(dtInterface.Id);
                    processedIds.Add(dtInterface.Id.ToString());
                }
            }
        }

        return Task.FromResult<IDictionary<DigitalTwinsModelBasicData, IEnumerable<string>>>(modelsMap);
    }

    public void EnsureRequiredFields(BasicDigitalTwin twin)
    {
        if (twin is null || string.IsNullOrEmpty(twin.Metadata?.ModelId))
            return;

        if (!AzureDigitalTwinCache.ModelCache.ModelInfos.Any(x => x.Key == twin.Metadata.ModelId))
            return;

        var modelInfo = AzureDigitalTwinCache.ModelCache.ModelInfos.FirstOrDefault(x => x.Key == twin.Metadata.ModelId);

        // Initialize required components
        var components = modelInfo.Value.Item1.Contents.Where(x => x.Value.EntityKind == DTEntityKind.Component);
        foreach (var component in components)
            if (!twin.Contents.ContainsKey(component.Key))
                twin.Contents.Add(component.Key, new Dictionary<string, object>() { { "$metadata", new { } } });
    }

    public DTInterfaceInfo GetInterfaceInfo(string modelId)
    {
        if (AzureDigitalTwinCache.ModelCache.ModelInfos.TryGetValue(modelId, out var info))
        {
            return info.Item1;
        }

        return null;
    }

    public IReadOnlyDictionary<string, DTInterfaceInfo> GetInterfaceDescendants(IEnumerable<string> rootModelIds)
    {
        var modelsHierarchy = new Dictionary<string, DTInterfaceInfo>();

        foreach (var modelId in rootModelIds.Where(modelId => AzureDigitalTwinCache.ModelCache.ModelInterfaceDescendants.ContainsKey(modelId)))
        {
            var descInfo = AzureDigitalTwinCache.ModelCache.ModelInterfaceDescendants[modelId].Values;
            foreach (var m in descInfo)
                if (!modelsHierarchy.ContainsKey(m.Id.AbsoluteUri))
                    modelsHierarchy.Add(m.Id.AbsoluteUri, m);
        }

        return modelsHierarchy;
    }


    private static string ReturnKnownInterfaceOrDefault(string dtmi, IDictionary<string, (DTInterfaceInfo, DateTimeOffset?)> modelInfos)
    {
        if (!modelInfos.ContainsKey(dtmi))
        {
            return null;
        }
        return dtmi;
    }

    public bool IsDescendantOf(string rootModelId, string modelId)
    {
        var rootModelDtmi = new Dtmi(rootModelId);

        if (rootModelId == modelId)
        {
            return true;
        }

        if (null == ReturnKnownInterfaceOrDefault(modelId, AzureDigitalTwinCache.ModelCache.ModelInfos))
        {
            return false;
        }

        var interfaceInfos = new List<DTInterfaceInfo> { AzureDigitalTwinCache.ModelCache.ModelInfos[modelId].Item1 };

        while (interfaceInfos.Count > 0)
        {
            var parentInterfaceInfos = interfaceInfos.SelectMany(i => i.Extends).ToList();

            if (parentInterfaceInfos.Any(p => p.Id == rootModelDtmi))
            {
                return true;
            }

            interfaceInfos = parentInterfaceInfos;
        }
        return false;
    }

    public bool IsDescendantOfAny(IEnumerable<string> rootModelIds, string modelId)
    {
        return rootModelIds.Any(id => IsDescendantOf(id, modelId));
    }


    /// <summary>
    /// Get ids of a given model's ancestors, and including the given model's id itself.   
    /// </summary>
    /// <param name="modelId">DTDL model id</param>
    public HashSet<string> GetAllAncestors(string modelId)
    {
        HashSet<string> ancestors = new HashSet<string>();

        // Traverse DAG by DFS approach
        void GetAncestors(string modelId)
        {
            if (AzureDigitalTwinCache.ModelCache.ModelInfos.TryGetValue(modelId, out var modelInfo))
            {
                ancestors.Add(modelId);

                DTInterfaceInfo interfaceInfo = modelInfo.Item1;

                foreach (DTInterfaceInfo parentInterfaceInfo in interfaceInfo.Extends)
                {
                    var parentId = parentInterfaceInfo.Id.ToString();

                    if (!ancestors.Contains(parentId))
                    {
                        GetAncestors(parentId);
                    }
                }
            }
            else
            {
                _logger.LogError("Model Id: {ModelId} could not be found in model cache during GetAllAncestors method", modelId);
            }

        }

        GetAncestors(modelId);

        return ancestors;
    }


}
