using Microsoft.AspNetCore.Mvc;
using Willow.Rules.Model;
using Abodit.Graph;
using OntologyGraphTool.Models;

namespace OntologyGraphTool.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OntologyController : ControllerBase
{
    private readonly MappedOntology mappedOntology;
    private readonly Ontology willowOntology;
    private readonly ILogger<OntologyController> _logger;

    public OntologyController(
        MappedOntology mappedOntology,
        Ontology willowOntology,
        ILogger<OntologyController> logger)
    {
        this.mappedOntology = mappedOntology ?? throw new ArgumentNullException(nameof(mappedOntology));
        this.willowOntology = willowOntology ?? throw new ArgumentNullException(nameof(willowOntology));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Route("get-mapped-ontology")]
    [ProducesResponseType(200, Type = typeof(Batch<EnhancedModel>))]
    public IActionResult GetOntology(BatchRequestDto request)
    {
        int total = this.mappedOntology.Models.Nodes.Count();
        int skip = ((request.Page ?? 1) - 1) * (request.PageSize ?? 10);
        int take = request.PageSize ?? 10;

        var dtdls = this.mappedOntology.Models.Nodes
            .Skip(skip)
            .Take(take)
            .Select(x => EnhancedModel.FromModel(x, this.mappedOntology.Models))
            .ToList();

        var batch = new Batch<EnhancedModel>(skip, total, dtdls);

        return Ok(batch);
    }

    [HttpPost]
    [Route("get-willow-ontology")]
    [ProducesResponseType(200, Type = typeof(Batch<DtdlModel>))]
    public IActionResult GetWillowOntology(BatchRequestDto request)
    {
        var ontology = new Ontology("/Users/ian/opendigitaltwins-building/Ontology");
        int total = ontology.Models.Nodes.Count();
        int skip = ((request.Page ?? 1) - 1) * (request.PageSize ?? 10);
        int take = request.PageSize ?? 10;

        var dtdls = ontology.Models
            .Skip(skip)
            .Take(take).ToList();

        var batch = new Batch<DtdlModel>(skip, total, dtdls);

        return Ok(batch);
    }
}
