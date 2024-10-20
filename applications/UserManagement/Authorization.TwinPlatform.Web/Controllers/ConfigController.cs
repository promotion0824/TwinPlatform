using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using Willow.AppContext;

namespace Authorization.TwinPlatform.Web.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class ConfigController : ControllerBase
{
	private readonly ILogger<ConfigController> _logger;
	private readonly SPAConfig _spaConfig;
	private readonly AppInsightsSettings _appInsightSettings;
    private readonly WillowContextOptions _willowContext;

	public ConfigController(ILogger<ConfigController> logger, IOptions<SPAConfig> spaConfigOption,
		IOptions<AppInsightsSettings> appInsightSettingsOption,
        IOptions<WillowContextOptions> willowContextOption)
	{
		_logger = logger;
		_spaConfig = spaConfigOption.Value;

		_appInsightSettings = appInsightSettingsOption.Value;
        _willowContext = willowContextOption.Value;
	}

	/// <summary>
	/// Method to front end configuration
	/// </summary>
	/// <returns>SPA Config</returns>
	[HttpGet]
	public IActionResult GetConfig()
	{
        string? assemblyVersion = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
		return Ok(new { _spaConfig, _appInsightSettings, _willowContext, assemblyVersion});
	}

}

