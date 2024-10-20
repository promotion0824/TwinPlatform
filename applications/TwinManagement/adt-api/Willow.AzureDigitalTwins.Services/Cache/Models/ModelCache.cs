using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.Extensions.Logging;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Cache.Models;

public interface IModelCache
{
    bool IsModelsLoaded { get; set; }
    IDictionary<string, (DTInterfaceInfo, DateTimeOffset?)> ModelInfos { get; }
    ConcurrentDictionary<string, IDictionary<string, DTInterfaceInfo>> ModelInterfaceDescendants { get; }
    IDictionary<string, List<UnitInfo>> UnitInfos { get; }
    void TryCreateOrReplaceModel(IEnumerable<DigitalTwinsModelBasicData> models);
    bool TryRemoveModel(string id);
}
public record UnitInfo(string UnitProperty, string AnnotatedProperty, string unit);
public class ModelCache : IModelCache
{
    public bool IsModelsLoaded { get; set; }
    public IDictionary<string, (DTInterfaceInfo, DateTimeOffset?)> ModelInfos { get; private set; }

    public ConcurrentDictionary<string, IDictionary<string, DTInterfaceInfo>> ModelInterfaceDescendants { get; private set; } = new ConcurrentDictionary<string, IDictionary<string, DTInterfaceInfo>>();

    public IDictionary<string, List<UnitInfo>> UnitInfos { get; private set; }

    private readonly Dtmi overrideTypeId = new("dtmi:dtdl:extension:overriding:v1:Override");

    private readonly Dtmi unitPropId = new("dtmi:dtdl:extension:quantitativeTypes:v1:property:unit");

    private readonly ILogger _logger;


    public ModelCache(ConcurrentDictionary<string, DigitalTwinsModelBasicData> models, ILogger logger)
    {
        _logger = logger;
        IsModelsLoaded = !models.IsEmpty;

        MeasureExecutionTime.ExecuteTimed(() =>
        {
            ProcessModels(models.Select(s => s.Value));
            return Task.FromResult(true);
        },
        (res, ms) =>
        {
            _logger.LogInformation("Time taken to process Models in Cache: {TimeTaken} ms", ms);
        }
        );

    }

    public void TryCreateOrReplaceModel(IEnumerable<DigitalTwinsModelBasicData> models)
    {
        var allModels = ModelInfos.Select(x => x.Value.ToModelBasicData()).UnionBy(models, k => k.Id);

        ProcessModels(allModels);
    }

    public bool TryRemoveModel(string id)
    {
        return ModelInfos.Remove(id) ||
        UnitInfos.Remove(id) ||
        ModelInterfaceDescendants.Remove(id, out _);
    }

    // Sample UnitInfo data after parsing looks like:
    //For the model "dtmi:com:willowinc:HVACBalancingValve;1", the dictionary holds the following:
    //Key: "dtmi:com:willowinc:HVACBalancingValve;1"

    //UnitProperty, AnnotatedProperty, Unit =>  "weightUnit", "weight", "kilogram"
    //UnitProperty, AnnotatedProperty, Unit =>  "pressureCapacityUnit", "pressureCapacity", "bar"
    //UnitProperty, AnnotatedProperty, Unit =>  "flowCapacityUnit", "flowCapacity", "litrePerSecond"
    private void ProcessModels(IEnumerable<DigitalTwinsModelBasicData> models)
    {
        if (models == null || !models.Any())
        {
            ModelInfos = new Dictionary<string, (DTInterfaceInfo, DateTimeOffset?)>();
            ModelInterfaceDescendants = new();
        }

        // PROCESS Model INFO
        var parser = new ModelParser();
        var modelInfos = parser.Parse(models.Select(x => x.DtdlModel));
        ModelInfos = modelInfos.Values.OfType<DTInterfaceInfo>().ToDictionary(i => i.Id.AbsoluteUri, i => (i, models.Single(x => x.Id == i.Id.AbsoluteUri).UploadedOn));

        // PROCESS Units INFO
        //Parsing the models to filter the models having annotated Unit property
        UnitInfos = new Dictionary<string, List<UnitInfo>>();
        foreach (KeyValuePair<Dtmi, DTEntityInfo> elt in modelInfos)
        {
            string modelName = elt.Value.Id.AbsoluteUri;
            if (elt.Value.EntityKind == DTEntityKind.Interface)
            {
                var unitInfoList = ParseUnitsInModels(elt);
                if (unitInfoList.Count > 0 && !UnitInfos.ContainsKey(modelName))
                    UnitInfos.TryAdd(modelName, unitInfoList);
            }
        }
        _logger.LogInformation("UnitInfos cache model count: {Count} ", UnitInfos.Count);

        //PROCESS MODEL INTERFACE DESCENDANTS
        MeasureExecutionTime.ExecuteTimed(() =>
        {
            UpdateInterfaceDescendants(models);
            return Task.FromResult(true);
        },
        (res, ms) =>
        {
            _logger.LogInformation("Time taken to update Interface descendants: {TimeTaken} ms", ms);
        }
        );


    }

    // Parse for unit properties
    private List<UnitInfo> ParseUnitsInModels(KeyValuePair<Dtmi, DTEntityInfo> elt)
    {
        var oneModel = new List<UnitInfo>();
        foreach (var content in ((DTInterfaceInfo)elt.Value).Contents)
        {
            try
            {
                if (content.Value.EntityKind == DTEntityKind.Property && content.Value.SupplementalTypes.Contains(overrideTypeId) &&
        (Dtmi)content.Value.SupplementalProperties["dtmi:dtdl:extension:overriding:v1:Override:overrides"] == unitPropId)
                {
                    string annotatedProp = (string)content.Value.SupplementalProperties["dtmi:dtdl:extension:annotation:v1:ValueAnnotation:annotates"];
                    if (annotatedProp == null)
                    {
                        continue;
                    }

                    DTInterfaceInfo dtInterfaceInfo = (DTInterfaceInfo)elt.Value;
                    if (!dtInterfaceInfo.Contents.TryGetValue(annotatedProp, out var dtContentInfo))
                    {
                        continue;
                    }

                    if (!dtContentInfo.SupplementalProperties.TryGetValue(unitPropId.ToString(), out var unitSupplementalProperty))
                    {
                        continue;
                    }
                    var unit = ((DTEnumValueInfo)unitSupplementalProperty).Name;
                    oneModel.Add(new UnitInfo(content.Key, annotatedProp, unit));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing model property {PropertyName}", elt.Value.DisplayName);
            }
        }
        return oneModel;
    }

    //Adds descendants of a model to cache
    private void UpdateInterfaceDescendants(IEnumerable<DigitalTwinsModelBasicData> models)
    {
        if (models == null || !models.Any()) return;
        ModelInterfaceDescendants.Clear();

        foreach (var modelId in models.Select(s => s.Id))
        {
            try
            {
                var modelsHierarchy = new Dictionary<string, DTInterfaceInfo>();
                var rootModelInfo = ModelInfos[modelId].Item1;
                var newInterfaceInfos = new List<DTInterfaceInfo> { rootModelInfo };
                var maxLoops = ModelInfos.Count;
                while (newInterfaceInfos.Any())
                {
                    if (maxLoops-- <= 0)
                        break;

                    foreach (var i in newInterfaceInfos)
                    {
                        modelsHierarchy.TryAdd(i.Id.AbsoluteUri, i);
                    }

                    newInterfaceInfos = ModelInfos.Values
                        .Where(i => i.Item1.Extends.Any(c =>
                            newInterfaceInfos.Select(n => n.Id).Contains(c.Id)))
                        .Select(x => x.Item1)
                        .ToList();
                }

                ModelInterfaceDescendants[modelId] = modelsHierarchy;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating interface descendants for model :{ModelID}", modelId);
            }
        }
    }
}
