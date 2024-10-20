using Microsoft.AspNetCore.Mvc;
using Willow.Rules.Model;
using OntologyGraphTool.Services;
using OntologyGraphTool.Controllers;
using OntologyGraphTool.Models;

[ApiController]
[Route("api/[controller]")]
public class MappingController : ControllerBase
{
    private readonly MappingService mappingService;
    private readonly ILogger<MappingController> _logger;

    public MappingController(
        MappingService mappingService,
        ILogger<MappingController> logger)
    {
        this.mappingService = mappingService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Route("get-mappings")]
    [ProducesResponseType(200, Type = typeof(Batch<Mapping>))]
    public IActionResult GetMappings(BatchRequestDto request)
    {
        var mappings = this.mappingService.Mappings;

        int skip = ((request.Page ?? 1) - 1) * (request.PageSize ?? 10);
        int take = request.PageSize ?? 10;

        IEnumerable<Mapping> dtdls = mappings;

        foreach (var filter in request.FilterSpecifications)
        {
            var val = filter.value?.ToString() ?? "";
            if (filter.field == "source")
            {
                if (filter.@operator == "contains")
                {
                    dtdls = dtdls
                        .Where(d => d.Source.id.Contains(val) ||
                            d.Source.displayName.en.Contains(val) ||
                            d.Destination.id.Contains(val) ||
                            d.Destination.displayName.en.Contains(val)
                            );
                }
            }
        }

        int total = dtdls.Count();

        foreach (var sort in request.SortSpecifications)
        {
            if (sort.field == "source" && sort.sort == "asc")
            {
                dtdls = dtdls.OrderBy(x => x.Source.id);
            }
            else if (sort.field == "source" && sort.sort == "desc")
            {
                dtdls = dtdls.OrderByDescending(x => x.Source.id);
            }
            else if (sort.field == "destination" && sort.sort == "asc")
            {
                dtdls = dtdls.OrderBy(x => x.Destination.id);
            }
            else if (sort.field == "destination" && sort.sort == "desc")
            {
                dtdls = dtdls.OrderByDescending(x => x.Destination.id);
            }
            else if (sort.field == "score" && sort.sort == "asc")
            {
                dtdls = dtdls
                    .OrderByDescending(x => x.Score)
                    // Sorted to keep groups together
                    .GroupBy(x => x.Source.id)
                    .OrderBy(x => x.Max(y => y.Score))
                    .SelectMany(x => x);
            }
            else if (sort.field == "score" && sort.sort == "desc")
            {
                dtdls = dtdls
                    .OrderByDescending(x => x.Score)
                    // Sorted to keep groups together
                    .GroupBy(x => x.Source.id)
                    .OrderByDescending(x => x.Max(y => y.Score))
                    .SelectMany(x => x);
            }
        }

        var pruned = dtdls
            .Skip(skip)
            .Take(take)
            .ToList();

        var batch = new Batch<Mapping>(skip, total, pruned);

        return Ok(batch);
    }

    [HttpGet]
    [Route("get-mapping/{id}")]
    [ProducesResponseType(200, Type = typeof(Mapping))]
    [ProducesResponseType(404, Type = typeof(string))]
    public IActionResult GetMapping(string id)
    {
        var mappings = this.mappingService.Mappings;
        var mapping = mappings.FirstOrDefault(x => x.id == id);

        if (mapping is null) return NotFound(id);
        return Ok(mapping);
    }

}
