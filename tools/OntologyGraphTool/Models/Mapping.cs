using OntologyGraphTool.Controllers;
using Willow.Rules.Model;

namespace OntologyGraphTool.Models;

public class Mapping
{
    public string id => Source.id + "_" + Destination.id;
    public double Score { get; set; }
    /// <summary>
    /// Order within same source value
    /// </summary>
    public int Index { get; set; }
    public double NameScore { get; set; }
    public double AncestorScore { get; set; }
    public EnhancedModel Source { get; }
    public EnhancedModel Destination { get; }

    public Mapping(EnhancedModel source, EnhancedModel destination, double NameScore, int index)
    {
        this.Source = source;
        this.Destination = destination;
        this.Score = NameScore;
        this.Index = index;
        this.NameScore = NameScore;
        this.AncestorScore = 0.0;
    }
}
