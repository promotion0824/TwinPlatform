using DigitalTwinCore.Models;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinCore.Services
{
    public interface IDigitalTwinModelParser
    {
        InterfaceInfo GetInterfaceHierarchy(string rootModelId);
        IReadOnlyDictionary<string, DTInterfaceInfo> GetInterfaceDescendants(string[] rootModelIds);
        bool IsDescendantOfAny(string[] rootModelIds, string modelId);
        bool IsDescendantOf(string rootModelIds, string modelId);
        DTInterfaceInfo GetInterface(string modelId);
        public bool IsModelKnown(string model);
    }

    public class DigitalTwinModelParser : IDigitalTwinModelParser
    {
        private IReadOnlyDictionary<Dtmi, DTInterfaceInfo> ModelInfos { get; set; }
        private ILogger<IDigitalTwinService> _logger;

        private DigitalTwinModelParser(IReadOnlyDictionary<Dtmi, DTEntityInfo> modelInfos, ILogger<IDigitalTwinService> logger)
        {
            var interfaceDictionary = modelInfos.Values.OfType<DTInterfaceInfo>().ToDictionary(i => i.Id);
            _logger = logger;
            ModelInfos = new ReadOnlyDictionary<Dtmi, DTInterfaceInfo>(interfaceDictionary);
        }
        
        public IReadOnlyDictionary<string, DTInterfaceInfo> GetInterfaceDescendants(string[] rootModelIds)
        {
            
            // Make a lookup of all entities that are extended from
            var modelExtensions = ModelInfos.Values
                .SelectMany(i => i.Extends
                    .Select(e => (InterfaceInfo:i.Id, Extends:e.Id)))
                .ToLookup(i => i.Extends, tuple => tuple.InterfaceInfo);
            
            IEnumerable<Dtmi> FlattenModelTree(ILookup<Dtmi, Dtmi> lookup, Dtmi parent) => 
                lookup[parent].SelectMany(c => FlattenModelTree(lookup, c)).Concat(new[] { parent });
            
            var results = new List<Dtmi>();
            // Flatten the tree of models from all the root model ids we care about and return a distinct list
            foreach (var rootModelId in rootModelIds)
            {    
                var rootModelDtmi = new Dtmi(rootModelId);
                if (!ModelInfos.ContainsKey(rootModelDtmi))
                {
                    // TODO: should this result in a BadRequest?
                    _logger?.LogWarning("GetInterfaceDecendants called for unknown model: {model}", rootModelDtmi);
                    continue;
                }
                
                results.AddRange(FlattenModelTree(modelExtensions, rootModelDtmi));
            }

            var output = results.Distinct()
                .Select(i => ModelInfos[i])
                .ToDictionary(i => i.Id.AbsoluteUri, i => i);
                
            return new ReadOnlyDictionary<string, DTInterfaceInfo>(output);

        }

        public InterfaceInfo GetInterfaceHierarchy(string rootModelId)
        {
            var rootModelDtmi = new Dtmi(rootModelId);

            if (ModelInfos.ContainsKey(rootModelDtmi))
            {
                if (null == ReturnKnownInterfaceOrDefault(rootModelDtmi))
                {
                    return null;
                }
                var rootModelInfo = ModelInfos[rootModelDtmi];

                return new InterfaceInfo { Model = rootModelInfo, Children = GetChildrenOfInterface(rootModelInfo) };
            }
            return null;
        }

        private List<InterfaceInfo> GetChildrenOfInterface(DTInterfaceInfo rootModelInfo)
        {
            return ModelInfos.Values
                .Where(i => i.Extends.Any(c => c.Id == rootModelInfo.Id)).
                Select(i => new InterfaceInfo { Model = i, Children = GetChildrenOfInterface(i) })
                .ToList();
        }

        public bool IsModelKnown(string model)
        {
            return null != ReturnKnownInterfaceOrDefault(new Dtmi(model));
        }

        private Dtmi ReturnKnownInterfaceOrDefault(Dtmi dtmi)
        {
            if (!ModelInfos.ContainsKey(dtmi))
            {
                // This happens because we have old Twins pointing to old :willow: models
                //   mixed in with new ones at the moment
                // TODO: Remove the above check when old models are absent
                return null;
            }
            return dtmi;
        }

        public bool IsDescendantOf(string rootModelId, string modelId)
        {
            var modelDtmi = new Dtmi(modelId);
            var rootModelDtmi = new Dtmi(rootModelId);

            if (rootModelId == modelId)
            {
                return true;
            }

            if (null == ReturnKnownInterfaceOrDefault(modelDtmi))
            {
                return false;
            }

            var interfaceInfos = new List<DTInterfaceInfo> { ModelInfos[modelDtmi] };

            while (interfaceInfos.Any())
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

        public bool IsDescendantOfAny(string[] rootModelIds, string modelId)
        {
            return rootModelIds.Any(id => IsDescendantOf(id, modelId));
        }

        public static async Task<DigitalTwinModelParser> CreateAsync(IList<Model> models, ILogger<IDigitalTwinService> logger)
        {
            var modelInfos = await new ModelParser().ParseAsync(models.Select(m => m.ModelDefinition).ToAsyncEnumerable());
            return new DigitalTwinModelParser(modelInfos, logger);
        }

        public DTInterfaceInfo GetInterface(string modelId)
        {
            var dtmi = new Dtmi(modelId);
            if (!ModelInfos.ContainsKey(dtmi))
            {
                _logger.LogWarning("Interface not found: {ModelId}", modelId);
            }

            return ModelInfos[dtmi];
        }

        // Simple helper to qualify name of model: "Document" -> "com:dtmi:willow:Document;1"
        public static string QualifyModelName(string modelName)
        {
            if ( ! modelName.Contains(':'))
                modelName = Constants.Dtdl.DtmiWillowPrefix + modelName;

            if ( ! modelName.Contains(';'))
                modelName += ";1";

            return modelName;
        }
    }
}
