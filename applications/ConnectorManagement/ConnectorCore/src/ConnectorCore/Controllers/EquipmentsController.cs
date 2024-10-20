namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Dtos;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Manage equipment.
    /// </summary>
    [ApiController]
    public class EquipmentsController : Controller
    {
        private readonly IEquipmentsService equipmentsService;

        internal EquipmentsController(IEquipmentsService equipmentsService)
        {
            this.equipmentsService = equipmentsService;
        }

        /// <summary>
        /// Gets or sets the list of all equipments on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("sites/{siteId}/allEquipments")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(typeof(List<EquipmentSimpleDto>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<ActionResult<GetEquipmentResult>> GetSiteEquipments([FromRoute] Guid siteId)
        {
            var equipments = await equipmentsService.GetSiteEquipmentsAsync(siteId);
            var dtos = EquipmentSimpleDto.Map(equipments);
            return Ok(dtos);
        }

        /// <summary>
        /// Gets or sets the list of all equipments with category on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("sites/{siteId}/allEquipmentsWithCategory")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(typeof(List<EquipmentWithCategoryDto>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<ActionResult<List<EquipmentWithCategoryDto>>> GetSiteEquipmentsWithEquipment([FromRoute] Guid siteId)
        {
            var equipments = await equipmentsService.GetListBySiteIdAsync(siteId);
            var dtos = EquipmentWithCategoryDto.Map(equipments);
            return Ok(dtos);
        }

        /// <summary>
        /// List all equipments on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="continuationToken">If provided fetches the next page of data.</param>
        /// <param name="pageSize">If provided overrides default page size.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("sites/{siteId}/[controller]/")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<GetEquipmentResult>> Get(Guid siteId, [FromQuery(Name = "continuationToken")] string continuationToken, [FromQuery(Name = "pageSize")] int? pageSize)
        {
            return await equipmentsService.GetListBySiteIdAsync(siteId, continuationToken, pageSize);
        }

        /// <summary>
        /// Get equipment by a list of their identifiers.
        /// </summary>
        /// <param name="equipmentIds">List of equipment identifiers, comma-separated.</param>
        /// <param name="includePoints">Pass true to include points for the equipment.</param>
        /// <param name="includePointTags">Include tags of equipment points into equipment tags list.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<EquipmentEntity>>> GetByIds([FromQuery] string equipmentIds, [FromQuery] bool? includePoints, [FromQuery] bool? includePointTags)
        {
            if (string.IsNullOrEmpty(equipmentIds))
            {
                return BadRequest("equipmentIds parameter is expected");
            }

            var equipmentIdsList = equipmentIds.Split(',').Select(Guid.Parse).ToList();
            return (await equipmentsService.GetByIdsAsync(equipmentIdsList, includePoints, includePointTags)).ToList();
        }

        /// <summary>
        /// Enforces equipment data cache refresh.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("/[controller]/cache/refresh")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshEquipmentCache()
        {
            await equipmentsService.RefreshEquipmentCacheAsync();
            return Ok();
        }

        /// <summary>
        /// List all equipment categories on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/[controller]/categories")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<CategoryEntity>>> GetCategoriesBySite(Guid siteId)
        {
            return (await equipmentsService.GetEquipmentCategoriesBySiteIdAsync(siteId)).ToList();
        }

        /// <summary>
        /// List all equipment categories on floor.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="floorId">Id of the floor.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/floors/{floorId}/[controller]/categories")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<CategoryEntity>>> GetCategoriesByFloor(Guid siteId, Guid floorId)
        {
            return (await equipmentsService.GetEquipmentCategoriesBySiteIdAndFloorIdAsync(siteId, floorId)).ToList();
        }

        /// <summary>
        /// List all equipment on site based on a given category.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="categoryId">equipment category.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/categories/{categoryId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<EquipmentEntity>>> GetEquipmentsByCategory(Guid siteId, Guid categoryId)
        {
            return (await equipmentsService.GetListBySiteIdAndCategoryAsync(siteId, categoryId)).ToList();
        }

        /// <summary>
        /// List all equipment on site based on a given category.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="floorId">Id of the Floor.</param>
        /// <param name="categoryId">equipment category.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/floors/{floorId}/categories/{categoryId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<EquipmentEntity>>> GetEquipmentsByFloorAndCategory(Guid siteId, Guid floorId, Guid categoryId)
        {
            return (await equipmentsService.GetByCategoryAndFloor(siteId, floorId, categoryId)).ToList();
        }

        /// <summary>
        /// List all equipment on site based on a given floor.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="floorId">Id of the Floor.</param>
        /// <param name="keyword">The query keyword.</param>
        /// <param name="includeEquipmentsNotLinkedToFloor">Include equipment not linked to the floor.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/floors/{floorId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<EquipmentEntity>>> GetEquipmentsByFloor([FromRoute] Guid siteId, [FromRoute] Guid floorId, [FromQuery] string keyword, [FromQuery] bool includeEquipmentsNotLinkedToFloor = false)
        {
            return (await equipmentsService.GetByFloorKeyword(siteId, floorId, keyword, includeEquipmentsNotLinkedToFloor)).ToList();
        }

        /// <summary>
        /// List all equipment on site (optionally filtered by keyword).
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="keyword">The query keyword.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/[controller]/search")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<EquipmentEntity>>> GetEquipments([FromRoute] Guid siteId, [FromQuery] string keyword)
        {
            return (await equipmentsService.GetByKeywordAsync(siteId, keyword)).ToList();
        }

        /// <summary>
        /// Find equipment by Id.
        /// </summary>
        /// <param name="equipmentId">Id of equipment to be retrieved.</param>
        /// <param name="includePoints">Pass true to include points for the equipment.</param>
        /// <param name="includePointTags">Include tags of equipment points into equipment tags list.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Equipment not found.</response>
        /// <response code="400">Invalid input.</response>
        [HttpGet("/[controller]/{equipmentId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EquipmentEntity>> Get(Guid equipmentId, [FromQuery] bool? includePoints, [FromQuery] bool? includePointTags)
        {
            var equipment = await equipmentsService.GetAsync(equipmentId, includePoints, includePointTags);
            if (equipment == null)
            {
                return NotFound();
            }

            return equipment;
        }
    }
}
