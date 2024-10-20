using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class RelationshipsController : Controller
    {
        private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;

        public RelationshipsController(IAzureDigitalTwinReader azureDigitalTwinReader, IAzureDigitalTwinWriter azureDigitalTwinWriter)
        {
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _azureDigitalTwinWriter = azureDigitalTwinWriter;
        }

        /// <summary>
        /// Creates or replaces relationship
        /// </summary>
        /// <param name="relationship">Relationship data</param>
        /// <remarks>
        /// Sample request
        ///
        ///		PUT
        ///		{
        ///			"$relationshipId": "includedIn_Portfolio-BPY_54fcd904-44a7-4459-b43c-5f936b2717b0",
        ///			"$targetId": "Portfolio-BPY",
        ///			"$sourceId": "NYC-MW",
        ///			"$relationshipName": "includedIn"
        ///		}
        /// </remarks>
        /// <returns>Create or replaced relationship</returns>
        /// <response code="200">Updated relationship</response>
        /// <response code="400">If provided relationship is missing source id, target id or name</response>
        [HttpPut("[controller]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BasicRelationship>> UpsertRelationship([FromBody][Required] BasicRelationship relationship)
        {
            if (relationship.SourceId is null)
                return BadRequest(new ValidationProblemDetails { Detail = "Relationship source is required" });

            if (relationship.TargetId is null)
                return BadRequest(new ValidationProblemDetails { Detail = "Relationship target is required" });

            if (relationship.Name is null)
                return BadRequest(new ValidationProblemDetails { Detail = "Relationship name is required" });

            var entity = await _azureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship);
            return entity;
        }

        /// <summary>
        /// Deletes a twin relationship
        /// </summary>
        /// <param name="twinid">Source twin id</param>
        /// <param name="relationshipid">Relationship id</param>
        /// <response code="204">When target relationship is deleted</response>
        /// <response code="404">Relationship not found</response>
        [HttpDelete("twins/{twinid}/relationship/{relationshipid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteRelationship([FromRoute][Required(AllowEmptyStrings = false)] string twinid,
            [FromRoute][Required(AllowEmptyStrings = false)] string relationshipid)
        {
            var relationship = await _azureDigitalTwinReader.GetRelationshipAsync(relationshipid, twinid);
            if (relationship == null)
                return NotFound();

            await _azureDigitalTwinWriter.DeleteRelationshipAsync(twinid, relationshipid);

            return NoContent();
        }

        /// <summary>
        /// Gets a twin relationship
        /// </summary>
        /// <param name="twinid">Source twin id</param>
        /// <param name="relationshipid">Relationship id</param>
        /// <returns>Target relationship</returns>
        /// <response code="200">Target relationship retrieved</response>
        /// <response code="404">Relationship not found</response>
        [HttpGet]
        [Route("twins/{twinid}/relationship/{relationshipid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BasicRelationship>> GetRelationship([FromRoute][Required(AllowEmptyStrings = false)] string twinid,
            [FromRoute][Required(AllowEmptyStrings = false)] string relationshipid)
        {
            var relationship = await _azureDigitalTwinReader.GetRelationshipAsync(relationshipid, twinid);

            return relationship;
        }

        /// <summary>
        /// Gets outgoing relationships for a twin
        /// </summary>
        /// <param name="twinid">Source twin id</param>
        /// <param name="relationshipName">Target relationship name</param>
        /// <returns>Target relationships</returns>
        /// <response code="200">Target relationships retrieved</response>
        /// <response code="404">Twin not found</response>
        [HttpGet]
        [Route("twins/{twinid}/[controller]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BasicRelationship>>> GetRelationships(
            [FromRoute][Required(AllowEmptyStrings = false)] string twinid,
            string relationshipName = null)
        {
            var relationships = await _azureDigitalTwinReader.GetTwinRelationshipsAsync(twinid, relationshipName);

            return relationships.ToList();
        }

        /// <summary>
        /// Gets incoming relationships for a twin
        /// </summary>
        /// <param name="twinid">Source twin id</param>
        /// <returns>Target relationships</returns>
        /// <response code="200">Target relationships retrieved</response>
        /// <response code="404">Twin not found</response>
        [HttpGet]
        [Route("twins/{twinid}/[controller]/incoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BasicRelationship>>> GetIncomingRelationships([FromRoute][Required(AllowEmptyStrings = false)] string twinid)
        {
            var relationships = await _azureDigitalTwinReader.GetIncomingRelationshipsAsync(twinid);

            return relationships.ToList();
        }
    }
}
