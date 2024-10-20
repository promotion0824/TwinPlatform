namespace OntologyGraphTool.Models;

/// <summary>
/// Loads the WillowOntology from resources
/// </summary>
public class WillowOntology : Ontology
{
    public WillowOntology() : base("OntologyGraphTool.Willow.Ontology.DTDLv3.jsonld")
    {
    }
}
