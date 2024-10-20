namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Manage sschema columns.
    /// </summary>
    [Route("schemas/{schemaId}/[controller]")]
    [ApiController]
    public class SchemaColumnsController : Controller
    {
        private readonly ISchemaColumnsService schemaColumnsService;
        private readonly IJsonSchemaDataGenerator dataGenerator;

        internal SchemaColumnsController(ISchemaColumnsService schemaColumnsService, IJsonSchemaDataGenerator dataGenerator)
        {
            this.schemaColumnsService = schemaColumnsService;
            this.dataGenerator = dataGenerator;
        }

        /// <summary>
        /// List template data object by schema id.
        /// </summary>
        /// <param name="schemaId">ID of the schema.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/schemas/{schemaId}/template")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTemplate(Guid schemaId)
        {
            var columns = await schemaColumnsService.GetBySchema(schemaId);
            var templateJson = dataGenerator.GenerateEmptyObject(columns);

            return Content(templateJson, "application/json", Encoding.UTF8);
        }

        /// <summary>
        /// List all schemas by schemaId.
        /// </summary>
        /// <param name="schemaId">ID of schema to schema columns belong to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="400">Schema id provided is not a valid GUID.</response>
        [HttpGet]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<SchemaColumnEntity>>> Get(Guid schemaId)
        {
            return (await schemaColumnsService.GetBySchema(schemaId)).ToList();
        }

        /// <summary>
        /// Create a new schema column.
        /// </summary>
        /// <param name="schemaId">ID of schema to schema columns belong to.</param>
        /// <param name="schemaColumn">Schema column object to be created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="201">Successfully created.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="400">Invalid input.</response>
        [HttpPost]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<SchemaColumnEntity>> Post(Guid schemaId, [FromForm] SchemaColumnEntity schemaColumn)
        {
            if (schemaColumn.SchemaId != schemaId)
            {
                return BadRequest("SchemaId is different from route value.");
            }

            try
            {
                schemaColumn = await schemaColumnsService.CreateAsync(schemaColumn);
                return Created(string.Empty, schemaColumn);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(new ValidationProblemDetails
                { Title = "Data provided violates foreign keys", Detail = e.Message });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ValidationProblemDetails
                { Title = "Data provided can't be saved", Detail = e.Message });
            }
        }

        /// <summary>
        /// Find schema by Id.
        /// </summary>
        /// <param name="schemaId">ID of schema to schemaColumns belong to.</param>
        /// <param name="schemaColumnId">ID of schema column to return.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Schema column not found.</response>
        /// <response code="400">Id provided is not a valid GUID.</response>
        [HttpGet("{schemaColumnId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SchemaColumnEntity>> Get(Guid schemaId, Guid schemaColumnId)
        {
            var column = await schemaColumnsService.GetItemAsync(schemaColumnId);
            if (column != null && column.SchemaId != schemaId)
            {
                return BadRequest("SchemaId is different from route value.");
            }

            if (column == null)
            {
                return NotFound();
            }

            return column;
        }
    }
}
