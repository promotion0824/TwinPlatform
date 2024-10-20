using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Web.DTO;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for search
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> logger;
    private readonly ISearchService searchService;

    /// <summary>
    /// Creates a new <see cref="ModelController"/>
    /// </summary>
    public SearchController(ILogger<SearchController> logger,
        ISearchService searchService
        )
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    /// <summary>
    /// Gets the search results in order by category
    /// </summary>
    private async IAsyncEnumerable<SearchLineDto> GetAllInternal(string query)
    {
        using var disp = logger.BeginScope(new Dictionary<string, object> { ["Query"] = query });

        var serviceResults = searchService.Search(query);

        await foreach (var result in serviceResults)
        {
            var doc = result.Document;
            double score = result.Score;
            var locations = doc.Location
                .Zip(doc.LocationNames)
                .Select((entry) => new TwinLocation(entry.First, entry.Second, ""))
                .ToArray();

            yield return new SearchLineDto
            {
                Type = doc.Type,
                Id = doc.Key,
                LinkId = doc.Id ?? doc.Ids.FirstOrDefault() ?? Guid.NewGuid().ToString(),  // bug bug, ID was not set here
                Description = doc.Names.FirstOrDefault() ?? "",
                Score = score,
                Locations = locations
            };
        }
    }

    /// <summary>
    /// Get search results for a query string
    /// </summary>
    /// <remarks>
    /// Searches models, twins, rules and more
    /// </remarks>
    [HttpGet("Search", Name = "Search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResultDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetSearchResults(string query, int offset = 0, int take = 50)
    {
        try
        {
            List<SearchLineDto> lines = await GetAllInternal(query).Skip(offset).Take(take).ToListAsync();

            var result = new SearchResultDto
            {
                Query = query,
                Results = lines.ToArray()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in search");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Exports search results
    /// </summary>
    [HttpPost("ExportSearch", Name = "ExportSearch")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportSearch(string query, int take = 1000)
    {
        List<SearchLineDto> lines = await GetAllInternal(query).Take(take).ToListAsync();

        var result = new SearchResultDto
        {
            Query = query,
            Results = lines.ToArray()
        };

        return WebExtensions.CsvResult(result.Results.Select(v =>
        {
            dynamic expando = new ExpandoObject();

            expando.Type = v.Type;
            expando.Link = v.Id;
            expando.Description = v.Description;
            //no model ids so can't do column split
            expando.Location = v.Locations.Any() ? $"[{string.Join("].[", v.Locations.Select(v => v.Name))}]" : "";

            return expando;
        }), "SearchResults.csv");
    }
}
