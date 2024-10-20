using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Willow.AppContext;
using Willow.Exceptions;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Options;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Config Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IOptions<ApplicationInsightsDto> _applicationInsightsOptions;
    private readonly IOptions<AzureAppOptions> _azureAppOptions;
    private readonly IEnvService _envService;
    private readonly WillowContextOptions _willowContextOptions;
    private readonly IOptions<MtiOptions> _mtiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigController"/> class.
    /// </summary>
    /// <param name="applicationInsightsOptions">App Insights Configuration Option.</param>
    /// <param name="azureAppOptions">Front B2C Configuration Option.</param>
    /// <param name="willowContext">Willow App Context Instance.</param>
    /// <param name="mtiOptions">MTI Options.</param>
    /// <param name="envService">Environment Service.</param>
    public ConfigController(
        IOptions<ApplicationInsightsDto> applicationInsightsOptions,
        IOptions<AzureAppOptions> azureAppOptions,
        IOptions<WillowContextOptions> willowContext,
        IOptions<MtiOptions> mtiOptions,
        IEnvService envService)
    {
        ArgumentNullException.ThrowIfNull(applicationInsightsOptions);
        ArgumentNullException.ThrowIfNull(azureAppOptions);

        _applicationInsightsOptions = applicationInsightsOptions;
        _azureAppOptions = azureAppOptions;
        _willowContextOptions = willowContext.Value;
        _mtiOptions = mtiOptions;
        _envService = envService;
    }

    /// <summary>
    /// Retrieves Config controller data for AppInsights, AzureAdB2COptions, AzureAppOptions.
    /// </summary>
    /// <returns>
    /// Sample response:
    /// {
    ///  "appInsights": {
    ///    "instrumentationKey": "9569cd5a-dbdd-49c5-808f-8f98d51c5539"
    ///  },
    ///  "azureAdB2COptions": {
    ///    "clientId": "1d94dc8e-e007-4322-b6cc-08a380c0cc47",
    ///    "audience": "531b4367-2153-4122-b4c3-e69291d4cdaf"
    ///  },
    ///  "azureAppOptions": {
    ///    "baseUrl": "/",
    ///    "baseApi": "/",
    ///    "redirect": "http://localhost:3000",
    ///    "b2CScopes": [
    ///      "https://willowdevb2c.onmicrosoft.com/531b4367-2153-4122-b4c3-e69291d4cdaf/.default"
    ///    ],
    ///    "knownAuthorities": [
    ///      "willowidentity.b2clogin.com",
    ///      "willowdevb2c.b2clogin.com",
    ///      "willowinc.com"
    ///    ],
    ///    "authority": "https://willowdevb2c.b2clogin.com/willowdevb2c.onmicrosoft.com/B2C_1A_HRD_SIGNUPORSIGNIN"
    ///  }
    ///   "WillowContext": {
    ///  "EnvironmentConfiguration": {
    ///    "ShortName": "dev"
    ///  },
    ///  "RegionConfiguration": {
    ///    "ShortName": "eu2"
    ///  },
    ///  "StampConfiguration": {
    ///    "Name": "01"
    ///  },
    ///  "CustomerInstanceConfiguration": {
    ///    "CustomerSalesId": "xxxx",
    ///    "CustomerInstanceName": "Dev",
    ///    "Name": "development",
    ///    "DnsSubDomain": "xxxx"
    ///  }
    /// }.
    /// </returns>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(object))]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ObjectResult GetConfig()
    {
        return Ok(new
        {
            AppInsights = _applicationInsightsOptions.Value,
            AzureAppOptions = _azureAppOptions.Value,
            TlmAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            WillowContext = _willowContextOptions,
            MtiOptions = _mtiOptions.Value,
        });
    }

    /// <summary>
    /// Get the version of TLM build.
    /// </summary>
    /// <returns>TLM assembly version.</returns>
    [HttpGet("version", Name = "GetTlmAndDependenciesVersions")]
    [AllowAnonymous]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Models.AppVersion))]
    [ProducesResponseType(typeof(Models.AppVersion), StatusCodes.Status200OK)]
    public async Task<IActionResult> Version()
    {
        var adtVersionResponse = await _envService.GetAdtApiVersion();
        var response = new Models.AppVersion()
        {
            TlmAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            AdtApiVersion = adtVersionResponse.AdtApiVersion,
        };

        return Ok(response);
    }
}
