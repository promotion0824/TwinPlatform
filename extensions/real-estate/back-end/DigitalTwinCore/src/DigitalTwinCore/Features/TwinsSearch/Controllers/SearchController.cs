using System.Threading;
using System.Threading.Tasks;
using DigitalTwinCore.Features.TwinsSearch.Dtos;
using DigitalTwinCore.Features.TwinsSearch.Models;
using DigitalTwinCore.Features.TwinsSearch.Services;
using Kusto.Cloud.Platform.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Features.TwinsSearch.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _service;

        public SearchController(ISearchService service)
        {
            _service = service;
        }

        /// <summary>
        /// Search Twins across multiple sites
        /// </summary>
        /// <param name="request" cref="SearchRequest">Search parameters</param>
        /// <returns>List of twins and pagination controls</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<SearchResponse>> Get([FromQuery] SearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.Search(request, cancellationToken);
            if (result == null)
            {
                return NotFound("No Azure Digital Twin databases were found for the required site ids.");
            }

            result.Twins.ForEach(t => t.RawTwin = t.Raw.ToString(Formatting.None));

            return Ok(result);
        }

        /// <summary>
        /// Retrieve all twins from a saved query
        /// </summary>
        /// <param name="request" cref="BulkQueryRequest">Query parameters</param>
        /// <returns>List of Twins</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SearchTwin[]>> BulkQuery([FromBody] BulkQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.BulkQuery(request, cancellationToken);
            result.ForEach(t => t.RawTwin = JObject.FromObject(new
            {
                Raw = t.Raw.ToString(Formatting.None),
                Contents = t.Raw.SelectToken("customProperties")?.ToString(Formatting.None),
                Metadata = t.Raw.SelectToken("metadata")?.ToString(Formatting.None)
            })
            .ToString(Formatting.None));

            return Ok(result);
        }

        /// <summary>
        /// Retrieve all twins for a cognitive search
        /// </summary>
        /// <param name="request" cref="CognitiveSearchRequest">Query parameters</param>
        /// <returns>List of Twins</returns>
        [HttpPost("cognitiveSearch")]
        [Authorize]
        public async Task<ActionResult<SearchTwin[]>> GetCognitiveSearchTwins([FromBody] CognitiveSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.GetCognitiveSearchTwins(request, cancellationToken);

            if (request.SensorSearchEnabled)
            {
                result.ForEach(t => t.RawTwin = t.Raw.ToString(Formatting.None));
            }
            else
            {
                result.ForEach(t => t.RawTwin = JObject.FromObject(new
                {
                    Raw = t.Raw.ToString(Formatting.None),
                    Contents = t.Raw.SelectToken("customProperties")?.ToString(Formatting.None),
                    Metadata = t.Raw.SelectToken("metadata")?.ToString(Formatting.None)
                })
                .ToString(Formatting.None));
            }

            return Ok(result);
        }
    }
}
