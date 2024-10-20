using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.Cacheless;
using Json.Patch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Controllers.Admin
{
    /// <summary>
    /// Twins controller manages Azure Digital Twins
    /// </summary>
    [Route("admin/sites/{siteId}/[controller]")]
    [ApiController]
    public class TwinsController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;
        private readonly IAssetService _assetService;

        public TwinsController(IDigitalTwinServiceProvider digitalTwinServiceFactory, IAssetService assetService)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
            _assetService = assetService;
        }

        private async Task<IDigitalTwinService> GetDigitalTwinServiceAsync(Guid siteId)
        {
            return await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
        }

        [HttpGet("queries")]
        [Authorize]
        public async Task<ActionResult<Page<TwinDto>>> GetQueries([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            var queries = (service as CachelessAdtService).GetLatestExecutedQueries();
            return Ok(queries);
        }

        [HttpGet("paged")]
        [Authorize]
        public async Task<ActionResult<Page<TwinDto>>> GetPagedListAsync([FromRoute] Guid siteId, [FromHeader] string continuationToken)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            // TODO: It's debatable what to do here -- the issue is what to do with twins that have no site
            // TODO: Should have Customer or Portfolio root?
            // If we have an ADT instance, per customer, it shoud be ok to return *all* twins as we do below
            //    var twinEntities = await service.GetSiteTwinsAsync();

            var twinEntities = await service.GetTwinsAsync(continuationToken: HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);
            return Ok(new Page<TwinDto> { Content = twinEntities.Content.Select(x => TwinDto.MapFrom(x)), ContinuationToken = twinEntities.ContinuationToken });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TwinDto>>> GetListAsync([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            var twinEntities = await service.GetTwinsAsync(siteId);
            return Ok(twinEntities.Select(x => TwinDto.MapFrom(x)));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> GetAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.GetTwinByIdAsync(id, false);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(TwinDto.MapFrom(entity));
        }

        [HttpGet("{id}/nocache")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> GetUncachedAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            return await GetAsync(siteId, id);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TwinDto>> PostAsync([FromRoute] Guid siteId, [FromBody] TwinDto value, [FromQuery] bool isSyncRequired = true)
        {
            var output = await AddOrUpdateTwinAsync(siteId, Twin.MapFrom(value), isSyncRequired, value.UserId);
            return Created($"/admin/sites/{siteId}/twins/{output.Id}", output);
        }

        private async Task<TwinDto> AddOrUpdateTwinAsync(Guid siteId, Twin twin, bool isSyncRequired = true, string userId = null)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.AddOrUpdateTwinAsync(twin, isSyncRequired, userId);
            return TwinDto.MapFrom(entity);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> PutAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string id, 
            [FromBody] TwinDto value)
        {
            if (value == null)
            {
                throw new BadRequestException("Request body not provided, or invalid.");
            }
            if (id != value?.Id)
            {
                throw new BadRequestException("Id of the twin in the request body must match id in request URI.");
            }
            var output = await AddOrUpdateTwinAsync(siteId, Twin.MapFrom(value), userId:value.UserId);
            return Ok(output);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<ActionResult> PatchAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string id, 
            [FromBody] JsonPatch jsonPatch,
            [FromHeader(Name = "If-Match")] string ifMatch,
            [FromHeader(Name = "UserId")] string userId)
        {
            var etag = new Azure.ETag(ifMatch);
            var jsonPatchDocument = new JsonPatchDocument();

            foreach (var patchOperation in jsonPatch.Operations)
            {
                var operation = new Operation
                {
                    op = patchOperation.Op.ToString(),
                    path = patchOperation.Path.ToString(),
                };

                if (operation.ShouldSerializevalue())
                {
                    operation.value = patchOperation.Value;
                    operation.from = patchOperation.From.ToString();
                }

                jsonPatchDocument.Operations.Add(operation);
            }
            var service = await GetDigitalTwinServiceAsync(siteId);

            await service.PatchTwin(id, jsonPatchDocument, etag, userId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            await service.DeleteTwinAsync(id);
            return NoContent();
        }

        // Get relationships of twin from adt intance directly, only for verifying the relationships in importer
        [HttpGet("{id}/relationships/nocache")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RelationshipDto>>> GetRelationshipsUncachedAsync(
            [FromRoute] Guid siteId,
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entities = await service.GetRelationships(id); // Uncached in CachelessService.
            if (entities == null)
            {
                return NotFound();
            }
            return Ok(RelationshipDto.MapFrom(entities));
        }

        // GET: api/<RelationshipsController>
        [HttpGet("{id}/relationships")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RelationshipDto>>> GetRelationshipsAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var twin = await service.GetTwinByIdAsync(id);
            if (twin == null)
            {
                return UnprocessableEntity();
            }
            return Ok(RelationshipDto.MapFrom(twin.Relationships));
        }

        // GET: api/<RelationshipsController>
        [HttpGet("{id}/relationships/incoming")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<IncomingRelationshipDto>>> GetIncomingRelationships(
            [FromRoute] Guid siteId, 
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entities = await service.GetBasicIncomingRelationshipsAsync(id);
            if (entities == null)
            {
                return NotFound();
            }
            return Ok(entities.Select(IncomingRelationshipDto.MapFrom));
        }

        [HttpGet("withrelationships")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TwinWithRelationshipsDto>>> GetTwinsWithRelationshipsAsync([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            var twins = await service.GetTwinsWithRelationshipsAsync(siteId);

            return Ok(twins.Select(x => TwinWithRelationshipsDto.MapFrom(x)));
        }

        [HttpPost("withrelationships")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TwinWithRelationshipsDto>>> GetTwinsWithRelationshipsAsync([FromRoute] Guid siteId, [FromBody] List<string> twinIds)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            var twins = await service.GetTwinsWithRelationshipsAsync(siteId, twinIds);

            return Ok(twins.Select(x => TwinWithRelationshipsDto.MapFrom(x)));
        }

        [HttpGet("{id}/relationships/query")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RelationshipDto>>> GetRelationshipsByQueryAsync([FromRoute] Guid siteId,
                                                                                    [FromRoute] string id,
                                                                                    [FromQuery] string[] relationshipNames,
                                                                                    [FromQuery] string[] targetModels,
                                                                                    [FromQuery] int hops,
                                                                                    [FromQuery] string sourceDirection, 
                                                                                    [FromQuery] string targetDirection)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var targetRelationships = await service.GetTwinRelationshipsByQuery(id, relationshipNames, targetModels, hops, sourceDirection, targetDirection);

            return Ok(RelationshipDto.MapFrom(targetRelationships));
        }

        [HttpGet("{id}/incomingrelationships")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RelationshipDto>>> GetIncomingRelationshipsAsync(
            [FromRoute] Guid siteId,
            [FromRoute] string id,
            [FromQuery] string[] excludingRelationshipNames)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var incomingRelationships = await service.GetIncomingRelationshipsAsync(id);

            if (excludingRelationshipNames?.Any() == true)
                incomingRelationships.RemoveAll(r => excludingRelationshipNames.Contains(r.Name));
                
            return Ok(RelationshipDto.MapFrom(incomingRelationships));
        }

        #region Query based endpoints

        //
        // adtSqlQyery endpoints are meant to be temporary for dev/testing purposes only
        //

        private async Task<List<TwinWithRelationships>> queryTwinsAsync(Guid siteId, TwinAdtSqlQuery query)
        {
            // If the query doesn't start with SELECT we treat it as an inner query and wrap
            //  it in an outer form that just returns the Id of each twin returned 
            var adtQuery = query.Query.StartsWith("SELECT") 
                ? query.Query
                : $"SELECT T.$dtId FROM DIGITALTWINS T WHERE ({query.Query}) AND IS_PRIMITIVE(T.$dtId)";

            var service = await GetDigitalTwinServiceAsync(siteId);
            return await service.GetTwinsByQueryAsync(adtQuery);
        }

        [HttpPost("query/adt")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> PostAdtSqlQueryAsync(
            [FromRoute] Guid siteId, 
            [FromBody] TwinAdtSqlQuery query,
            [FromQuery] bool nocache = false)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var adtTwins = await queryTwinsAsync(siteId, query);
            var returnTwins = adtTwins;
            if (!nocache)
            {
               returnTwins = adtTwins.Select(t =>
               {
                   // TODO: if we keep this, use Task.WhenAll
                   var cachedTwin = service.GetTwinByIdAsync(t.Id).Result;
                   return cachedTwin;
               }).ToList();
            }

            return Ok( TwinDto.MapFrom(returnTwins));
        }

        // Return the count of twins matching the query
        // Note: This doesn't use count() 
        [HttpPost("query/adt/count")]
        [Authorize]
        public async Task<ActionResult<int>> PostAdtSqlQueryCountAsync(
            [FromRoute] Guid siteId, 
            [FromBody] TwinAdtSqlQuery query)
        {
            var twins = await queryTwinsAsync(siteId, query);
            return twins.Count;
        }

        // Delete all twins (which deletes all relationsips first) matching the query

        [HttpDelete("query/adt")]
        [Authorize]
        public async Task<ActionResult<int>>DeleteAdtSqlQueryAsync(
            [FromRoute] Guid siteId, 
            [FromBody] TwinAdtSqlQuery query)
        {
            var adtTwins = await queryTwinsAsync(siteId, query);
            var service = await GetDigitalTwinServiceAsync(siteId);
            var twinIds = adtTwins.Select(t => t.Id).ToList();
            await service.DeleteTwinsAndRelationshipsAsync(siteId, twinIds);
            
            return twinIds.Count;
        }

        // Note that at the moment we are only exposing the RealEstate queries, not the domain-independent base queries 
        //    although Floors is the only domain-specific part of the query, which can go away if we support it as part of a general relationship query
        [HttpPost("query/realestate/paged")]
        [Authorize]
        public async Task<ActionResult<Page<TwinDto>>> PostRealestateQueryAsync(
            [FromRoute] Guid siteId, 
            [FromBody] TwinSimpleRealEstateQuery query,
            [FromHeader] string continuationToken)
            
        {
            var page = await GetTwinsByModelsAsync(siteId, query, continuationToken);

            return Ok(new Page<TwinDto> { Content = TwinDto.MapFrom(page.Content), ContinuationToken = page.ContinuationToken });
        }

        [HttpPost("query/realestate")]
        [Authorize]
        public async Task<ActionResult<TwinDto[]>> PostRealestateQueryAsync(
            [FromRoute] Guid siteId,
            [FromBody] TwinSimpleRealEstateQuery query)

        {
            var page = await GetTwinsByModelsAsync(siteId, query, null);

            var allTwins = await page.FetchAll(x => GetTwinsByModelsAsync(siteId, query, x));

            return Ok(TwinDto.MapFrom(allTwins));
        }

        [HttpPost("query/realestate/ids/paged")]
        [Authorize]
        public async Task<ActionResult<Page<TwinRealestateIdDto>>> PostRealestateQueryIdsAsync(
            [FromRoute] Guid siteId, 
            [FromBody] TwinSimpleRealEstateQuery query,
            [FromHeader] string continuationToken)
            
        {
            var page = await GetTwinsByModelsAsync(siteId, query, continuationToken);

            return Ok(new Page<TwinRealestateIdDto> { Content = TwinRealestateIdDto.IdsMapFrom(page.Content), ContinuationToken = page.ContinuationToken });
        }

        [HttpPost("query/realestate/ids")]
        [Authorize]
        public async Task<ActionResult<TwinRealestateIdDto[]>> PostRealestateQueryIdsAsync(
            [FromRoute] Guid siteId,
            [FromBody] TwinSimpleRealEstateQuery query)

        {
            var page = await GetTwinsByModelsAsync(siteId, query, null);

            var allTwins = await page.FetchAll(x => GetTwinsByModelsAsync(siteId, query, x));

            return Ok(TwinRealestateIdDto.IdsMapFrom(page.Content));
        }

        [HttpGet("byUniqueId/{uniqueId}")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> GetByUniqueId([FromRoute] Guid siteId, [FromRoute] Guid uniqueId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.GetTwinByUniqueIdAsync(uniqueId, true);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(TwinDto.MapFrom(entity));
        }

        private async Task<Page<Twin>> GetTwinsByModelsAsync(Guid siteId, TwinSimpleRealEstateQuery query, string continuationToken)
        {
            if (query.Floors?.Count() > 0)
                throw new BadRequestException("Floor search not supported");

            if (query.Relationships != null)
                throw new BadRequestException("Relationships search not supported");

            var service = await GetDigitalTwinServiceAsync(siteId);

            var models = Enumerable.Empty<string>();
            if (query.RootModels?.Count() > 0)
            {
                models = query.RootModels.Select(DigitalTwinModelParser.QualifyModelName).ToList();
                var parser = await service.GetModelParserAsync();
                var badModels = models.Where(m => !parser.IsModelKnown(m)).ToList();
                if (badModels.Any())
                {
                    throw new BadRequestException($"Unknown models: '{string.Join(", ", badModels)}' ");
                }
            }

            return await service.GetTwinsByModelsAsync(siteId, models, query.RestrictToSite, continuationToken);
        }
		

		#endregion

		/// <summary>
		/// Retrieves the list of twins' Id for the provided unique ids
		/// </summary>
		/// <param name="siteId">the unique identifier for the site</param>
		/// <param name="uniqueIds">List of the twins' unique Ids</param>
		/// <returns>List of Twins' id for the associated unique ids</returns>
		[HttpGet("byUniqueId/batch")]
		[Authorize]
		public async Task<ActionResult<List<TwinIdDto>>> GetByUniqueIdsBatch([FromRoute] Guid siteId, [FromQuery] List<Guid> uniqueIds)
		{

			var service = await GetDigitalTwinServiceAsync(siteId);
			var twins = await service.GetTwinIdsByUniqueIdsAsync(uniqueIds);
			if (twins == null || !twins.Any())
			{
				return NotFound();
			}
			return Ok(twins);
		}
		/// <summary>
		/// Retrieves the points for an twin
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet("{id}/points")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getTwinPoints", Tags = new[] { "Twins" })]
        public async Task<ActionResult<List<PointDto>>> GetTwinPointsAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid id)
        {
            List<Point> output = await _assetService.GetAssetPointsAsync(siteId, id);

            if (output == null)
                return NotFound();

            List<PointDto> result = PointDto.MapFrom(output, true, false);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all the revisions of the specified twin.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/history")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getTwinHistory", Tags = new[] { "Twins" })]
        public async Task<ActionResult<List<TwinHistoryDto>>> GetTwinHistory([FromRoute] Guid siteId, [FromRoute] string id)
        {
            TwinHistoryDto twinHistory = await _assetService.GetTwinHistory(siteId, id);

            if (twinHistory == null)
                return NotFound();

            return Ok(twinHistory);
        }

        /// <summary>
        /// Retrieves information about the fields of the specified twin.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/fields")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getTwinFields", Tags = new[] { "Twins" })]
        public async Task<ActionResult<TwinFieldsDto>> GetTwinFieldsAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var twinFields = await _assetService.GetTwinFields(siteId, id);
            if (twinFields == null)
                return NotFound();

            return Ok(twinFields);
        }

        /// <summary>
		/// Retrieves the points by twin ids
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="twinIds"></param>
		/// <returns></returns>
		[HttpPost("twinIds/points")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getPointsByTwinIds", Tags = new[] { "Twins" })]
        public async Task<ActionResult<List<PointTwinDto>>> GetPointsByTwinIds(
            [FromRoute] Guid siteId,
            [FromBody] List<string> twinIds)
        {
            List<PointTwinDto> result = await _assetService.GetPointsByTwinIdsAsync(siteId, twinIds);
           
            return Ok(result);
        }

        /// <summary>
        /// For the given twins, resolve the closest twin that has the specified custom property present, by traversing the specified relationships
        /// </summary>
        /// <returns></returns>
        [HttpPost("closestWithCustomProperty")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<TwinMatchDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TwinMatchDto>>> FindClosestWithCustomProperty([FromRoute] Guid siteId, [FromBody] ClosestWithCustomPropertyQuery query)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);

            var result = await service.FindClosestWithCustomProperty(query);

            return Ok(result);
        }
    }
}
