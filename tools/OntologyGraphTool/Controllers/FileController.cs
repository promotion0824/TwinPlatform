using Microsoft.AspNetCore.Mvc;
using Willow.Rules.Model;
using OntologyGraphTool.Services;
using OntologyGraphTool.Controllers;
using OntologyGraphTool.Models;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly MappingService mappingService;
    private readonly ILogger<MappingController> _logger;

    public FileController(
        MappingService mappingService,
        ILogger<MappingController> logger)
    {
        this.mappingService = mappingService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Route("download")]
    // Swagger: [FileResultContentType("text/csv")]
    [ProducesResponseType(200, Type = typeof(IList<Mapping>))]
    public IActionResult Download()
    {
        var mappings = this.mappingService.Mappings;

        var fields = mappings.Select(x => new
        {
            source = x.Source.id,
            destination = x.Destination.id,
            namescore = x.NameScore,
            ancestorscore = x.AncestorScore,
            index = x.Index
        });

        string result = JsonConvert.SerializeObject(fields, Formatting.Indented);
        string fileName = $"mapping-{DateTimeOffset.Now.ToString("u")}.json";

        //        Response.ContentType = "application/json";
        Response.Headers.Add("pragma", "no-cache, public");
        Response.Headers.Add("cache-control", "private, nocache, must-revalidate, maxage=3600");
        Response.Headers.Add("content-disposition", "inline;filename=" + fileName);

        return Ok(result);
    }

}
