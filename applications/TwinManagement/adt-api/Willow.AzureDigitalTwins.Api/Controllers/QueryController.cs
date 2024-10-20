using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class QueryController : Controller
    {
        private readonly ITwinsService _twinsService;

        public QueryController(ITwinsService twinsService)
        {
            _twinsService = twinsService;
        }

        /// <summary>
        /// Executes provided query to return twins and appends relationships [Use only when other endpoints do not have satisfy your needs]
        /// </summary>
        /// <param name="queryTwinsRequest">Query parameters</param>
        /// <param name="sourceType">Adx/AdtQuery</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="continuationToken">Continuation token</param>
        /// <remarks>
        /// Sample request
        ///		POST twins for ADT
        ///		{
        ///			"query": "select * from digitaltwins where IS_OF_MODEL('dtmi:com:willowinc:Building;1')",
        ///			"includeRelationships": false,
        ///			"idsOnly": false
        ///		}
        ///		POST twins for ADX
        ///		{
        ///		    "query": "ActiveTwins | where ModelId == 'dtmi:com:willowinc:Building;1'",
        ///			"includeRelationships": false,
        ///			"idsOnly": false,
        ///			"isAdx": false
        ///     }
        ///
        /// Sample response
        ///
        ///		{
        ///			"content": [
        ///			{
        ///					"twin": {
        ///						"$dtId": "BPY-1MW",
        ///						"$metadata": {
        ///							"$model": "dtmi:com:willowinc:Building;1
        ///						},
        ///						"type": "Commercial Office",
        ///						"coordinates": {
        ///							"latitude": 40.7528,
        ///							"longitude": -73.997934
        ///						},
        ///						"elevation": 34,
        ///						"height": 995,
        ///						"uniqueID": "4e5fc229-ffd9-462a-882b-16b4a63b2a8a",
        ///						"code": "1MX",
        ///						"name": "One Miami West",
        ///						"siteID": "4e5fc229-ffd9-462a-882b-16b4a63b2a8a",
        ///						"address": {
        ///							"region": "NY"
        ///						}
        ///					}
        ///				}
        ///			]
        ///		}
        /// </remarks>
        /// <returns>Twins with relationships</returns>
        /// <response code="200">With found twins</response>
        [HttpPost("twins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Page<TwinWithRelationships>>> QueryTwinsWithRelationships(
                [FromBody][Required] QueryTwinsRequest queryTwinsRequest,
                [FromQuery] SourceType sourceType = SourceType.AdtQuery,
                [FromQuery] int pageSize = 100,
                [FromHeader] string continuationToken = null)
        {
            var twinsWithRelationships = await _twinsService.QueryTwinsAsync(queryTwinsRequest, sourceType, pageSize, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return twinsWithRelationships;
        }

        /// <summary>
        /// Executes provided query and returns results without parsing [Use only when other endpoints do not have satisfy your needs]
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <param name="sourceType">Adx/AdtQuery</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="continuationToken">Continuation token</param>
        /// <remarks>
        /// Sample ADT request 
        ///		POST
        ///		{
        ///			"select count() from digitaltwins where IS_OF_MODEL('dtmi:com:willowinc:Building;1')"
        ///		}
        ///
        /// Sample ADX request 
        ///		POST
        ///		{
        ///			"ActiveTwins | where ModelId == 'dtmi:com:willowinc:Building;1' | count"
        ///		}
        ///
        /// Sample response
        ///
        ///		{
        ///			"content": [
        ///				{
        ///					"COUNT": 1
        ///				}
        ///			]
        ///		}
        /// </remarks>
        /// <returns>Query results</returns>
        /// <response code="200">Query results</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Page<JsonDocument>>> Query(
            [FromBody][Required] string query,
            [FromQuery] SourceType sourceType = SourceType.AdtQuery,
            [FromQuery] int pageSize = 100,
            [FromHeader] string continuationToken = null)
        {
            var results = await _twinsService.QueryAsync(query, sourceType, pageSize, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return results;
        }
    }
}
