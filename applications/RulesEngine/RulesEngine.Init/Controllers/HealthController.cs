using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using Willow.Rules.Sources;
using Willow.Rules.DTO;
using Microsoft.Extensions.Logging;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Health / status end point
/// </summary>
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
	private readonly ILogger<HealthController> logger;

	/// <summary>
	/// Creates a new instance of the HealthController
	/// </summary>
	public HealthController(
		ILogger<HealthController> logger)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Gets the health status
	/// </summary>
	/// <returns>200 OK when everything is healthy</returns>
	[HttpGet]
	[Route("", Name = "GetHealth")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HealthDto))]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetHealth()
	{
		var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

		var health = new HealthDto
		{
			App = "RulesEngine.Init",
			Version = version.ToString()
		};

		logger.LogInformation("Health check");
		return Ok(health);
	}
}
