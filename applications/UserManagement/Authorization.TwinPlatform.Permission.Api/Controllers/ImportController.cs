using Authorization.Common.Models;
using Authorization.TwinPlatform.Permission.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

/// <summary>
/// Controller for managing Authorization data import
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ImportController : ControllerBase
{
	private readonly ILogger<ImportController> _logger;
	private readonly IImportManager _importManager;

	public ImportController(ILogger<ImportController> logger, IImportManager importManager)
	{
		_logger = logger;
		_importManager = importManager;
	}

	/// <summary>
	/// Action method to import Permission and Roles
	/// </summary>
	/// <param name="extensionName">Name of the extension</param>
	/// <param name="importModel">Import Model Instance as payload</param>
	/// <returns>Completed task</returns>
	[HttpPost("{extensionName}")]
	public async Task<IActionResult> ImportData([FromRoute] string extensionName, [FromBody] ImportModel importModel)
	{
		_logger.LogInformation("Received: Request for importing data from App:{App}", extensionName);

		try
		{
            await _importManager.ImportConfigurationData(extensionName, importModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed request, unable to complete the data import for Application:{App}",extensionName);
			return UnprocessableEntity();
		}

		_logger.LogInformation("Completed for importing data in to extension: {extension}", extensionName);

		return Ok();
	}
}
