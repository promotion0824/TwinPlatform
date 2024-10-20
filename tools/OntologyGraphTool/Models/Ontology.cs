using System.Reflection;
using Abodit.Graph;
using Abodit.Mutable;
using Newtonsoft.Json;
using OntologyGraphTool.Services;

namespace OntologyGraphTool.Models;

/// <summary>
/// Loads an ontology from an embedded resource and returns the graph for it
/// with RDFSType relationships for the extends property
/// </summary>
public class Ontology
{
    public Ontology(string resourceName)
    {
        this.ontologyLazy = new Lazy<Graph<DtdlModel, Relation>>(() => Load(resourceName));
    }

    public Graph<DtdlModel, Relation> Models => ontologyLazy.Value;
    private readonly Lazy<Graph<DtdlModel, Relation>> ontologyLazy;

    private static Graph<DtdlModel, Relation> Load(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resource = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));
        var models = new List<DtdlModel>();

        using (Stream stream = assembly.GetManifestResourceStream(resource))
        {
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    models = JsonConvert.DeserializeObject<List<DtdlModel>>(result);
                }
            }
            else
            {
                throw new FileNotFoundException(resourceName);
            }
        }

        var graphBuilder = new DtdlModelGraphBuilder();
        foreach (var model in models)
        {
            if (model.extends is null) graphBuilder.AddRoot(model);
            else if (model.extends.Count == 0) graphBuilder.AddRoot(model);
            else
            {
                foreach (var mid in model.extends)
                {
                    graphBuilder.AddParent(model, mid);
                }
            }
        }
        var graph = graphBuilder.GetGraph();

        return graph;
    }
}
