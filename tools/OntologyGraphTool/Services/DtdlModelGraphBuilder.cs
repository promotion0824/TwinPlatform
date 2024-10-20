using Abodit.Graph;
using Abodit.Mutable;
using OntologyGraphTool.Models;

namespace OntologyGraphTool.Services;

/// <summary>
/// Builds the inheritance graph
/// </summary>
public class DtdlModelGraphBuilder
{
    private Graph<string, Relation> graph = new Graph<string, Relation>();

    private Dictionary<string, DtdlModel> allModels = new Dictionary<string, DtdlModel>();

    public void AddRoot(DtdlModel root)
    {
        allModels[root.id] = root;
    }

    public void AddParent(DtdlModel child, string parentId)
    {
        this.graph.AddStatement(child.id, Relation.RDFSType, parentId);
        allModels[child.id] = child;
    }

    public Graph<DtdlModel, Relation> GetGraph()
    {
        var result = new Graph<DtdlModel, Relation>();
        foreach (var edge in this.graph.Edges)
        {
            var start = allModels[edge.Start];
            var end = allModels[edge.End];
            result.AddStatement(start, edge.Predicate, end);
        }
        return result;
    }

}
