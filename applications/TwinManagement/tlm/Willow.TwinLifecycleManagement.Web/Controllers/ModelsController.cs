using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.Exceptions;
using Willow.Model.Adt;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Models Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IModelsService _modelsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsController"/> class.
    /// </summary>
    /// <param name="modelsService"> Implementation of Models Service.</param>
    public ModelsController(IModelsService modelsService)
    {
        ArgumentNullException.ThrowIfNull(modelsService);
        _modelsService = modelsService;
    }

    /// <summary>
    /// Returns a list of Ids and Display Names (en) of all models present in the ADT instance.
    /// </summary>
    /// <param name="rootModel">Root model type.</param>
    /// <param name="sourceType">Source Type.</param>
    /// <returns>Root models and descendants or all models.</returns>
    /// <response code="200">Models retrieved.</response>
    [HttpGet("GetModels")]
    [Authorize(Policy = AppPermissions.CanReadModels)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<ModelsTwinInfo>))]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<ModelsTwinInfo>))]
    [ProducesResponseType(typeof(IEnumerable<Models.InterfaceTwinsInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ModelsTwinInfo>>> GetAllModelsAsync(string rootModel = null, SourceType sourceType = SourceType.Adx)
    {
        var response = string.IsNullOrEmpty(rootModel)
            ? await _modelsService.GetModelsInfo(sourceType)
            : (await _modelsService.GetModelFamilyAsync(rootModel)).Cast<ModelsTwinInfo>();

        return Ok(response);
    }
}
