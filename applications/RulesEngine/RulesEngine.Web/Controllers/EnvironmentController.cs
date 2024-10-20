using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Willow.Rules.Options;
using System;
using Willow.Rules.Sources;
using Newtonsoft.Json;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for environment
/// </summary>
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
public partial class EnvironmentController : ControllerBase
{
	private readonly ADB2COptions config;
	private readonly WillowEnvironment willowEnvironment;

	//	private readonly ITokenAcquisition tokenAcquisition;
	private readonly ILogger<EnvironmentController> logger;

	/// <summary>
	/// Creates a new <see cref="EnvironmentController" />
	/// </summary>
	public EnvironmentController(IOptions<ADB2COptions> options,
		WillowEnvironment willowEnvironment,
		ILogger<EnvironmentController> logger)
	{
		this.config = options.Value;
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Gets application environment information including ADB2C redirect and Willow Environment name and id
	/// </summary>
	/// <returns>Application information (ADB2C redirect etc)</returns>
	[HttpGet]
	[Route("api/environment", Name = "GetEnvironmentInfo")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EnvironmentDto))]
	[ProducesDefaultResponseType]
	public async Task<IActionResult> GetApplicationInfo()
	{
		var result = new EnvironmentDto
		{
			Redirect = config.Redirect,
			EnvironmentId = this.willowEnvironment.Id,
			EnvironmentName = this.willowEnvironment.Name
		};
		return Ok(result);
	}

	/// <summary>
	/// Gets application environment information as a javascript script file
	/// </summary>
	/// <returns>Application information (ADB2C redirect etc)</returns>
	[HttpGet]
	[Route("api/environmentscript", Name = "GetEnvironmentScript")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
	[ProducesDefaultResponseType]
	// TODO: Enable this once development is stable
	//[ResponseCache(Location = ResponseCacheLocation.Any, NoStore = false, Duration = 60)]
	public async Task<IActionResult> GetApplicationInfoJavascript()
	{
		var result = new
		{
			redirect = config.Redirect,
			baseurl = config.BaseUrl,
			environment = willowEnvironment.Name
		};

		return Content("window._env_=" + JsonConvert.SerializeObject(result) + ";",
			"application/javascript");
	}
}