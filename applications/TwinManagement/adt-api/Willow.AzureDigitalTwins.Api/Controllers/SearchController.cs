using Azure.Search.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.CognitiveSearch;
using Willow.Model.Response;

namespace Willow.AzureDigitalTwins.Api.Controllers;

/// <summary>
/// Action Endpoint for Azure Cognitive Search
/// </summary>
[Route("[controller]")]
[ApiController]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IAcsService _acsService;

    public SearchController(IAcsService acsService)
    {
        _acsService = acsService;
    }

    /// <summary>
    /// Execute Search against Azure Cognitive Service 
    /// </summary>
    /// <param name="query">Query Expression in string format.
    /// The query expression string use Lucene Query Syntax to Search ACS.</param>
    /// <param name="options">Search Options.</param>
    /// <returns>List of instances of <see cref="SearchResult{T}"/></returns>
    /// <remarks>
    ///To learn more about LQS, Visit <seealso href="https://learn.microsoft.com/en-us/azure/search/query-lucene-syntax#example-full-syntax"/>
    /// </remarks>
    /// <example>
    /// 
    /// Sample Request http://{adtapiurl}/search?query=*
    /// Body:
    /// {
    ///     options:{
    ///       "filter": "Type eq 'twin'",
    ///       "highlightFields": ["Id"],
    ///       "searchFields": ["Id", "Type"],
    ///       "select": ["Id", "Category"],
    ///       "top": 10,
    ///       "orderby": ["Id asc"],
    ///       "IncludeTotalResultCount": true,
    ///       "facets": ["Category"],
    ///       "scoringParameters": ["rules"]
    ///     }
    /// }
    ///
    /// Sample Response
    /// [
    ///    {
    ///        "score": int,
    ///        "document": {
    ///            "key": "aW5zaWdodE1TLUFZLUFZUy1MMTAtT1otMTBTLjEwMUFfYWNjdW11bGF0ZS10ZXN0",
    ///            "id": "string",
    ///            "ids": [
    ///                "string",
    ///                "string
    ///           ],
    ///            "siteId": "",
    ///            "externalId": "",
    ///            "modelIds": [
    ///                "dtmi:com:willowinc:OccupancyZone;1",
    ///                "OccupancyZone"
    ///            ],
    ///            "primaryModelId": "",
    ///            "type": "insight",
    ///            "tags": [
    ///                "insight",
    ///                "Testing"
    ///            ],
    ///            "category": "Testing",
    ///            "importance": 40,
    ///            "names": [
    ///                "Accumulate Test"
    ///            ],
    ///            "secondaryNames": [
    ///                "Total People Count"
    ///            ],
    ///            "location": [
    ///                "string",
    ///                "string",
    ///                "string",
    ///                "string",
    ///                "string",
    ///                "string"
    ///            ],
    ///            "fedBy": [],
    ///            "feeds": [],
    ///            "tenant": []
    ///         }
    ///     }
    /// ]
    /// 
    /// 
    /// 
    /// </example>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SearchResult<UnifiedItemDto>>>> QueryUnifiedIndexAsync([FromQuery] string query, [FromBody] SearchOptions options)
    {
        return await _acsService.QueryRawUnifiedIndexAsync(query, options);
    }

    public async Task<DocumentSearchResponse> QueryDocumentIndex(string query, DocumentSearchMode documentSearchMode, int skip, int take)
    {
        return await _acsService.QueryDocumentIndexAsync(query, documentSearchMode, skip, take);
    }
}
