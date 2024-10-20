using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Authorization.TwinPlatform.Common.Services;
public class ImportService : IImportService
{
	private readonly ILogger<ImportService> _logger;
	private readonly AuthorizationAPIOption _authorizationApiConfig;
	private readonly IAuthorizationApiTokenService _tokenService;
	private readonly HttpClient _httpClient;
	private Lazy<Task> _doLazyImport;

	public ImportService(ILogger<ImportService> logger,
	IAuthorizationApiTokenService tokenService,
	IHttpClientFactory httpClientFactory,
	IOptions<AuthorizationAPIOption> authorizationAPIOption)
	{
		_logger = logger;
		_authorizationApiConfig = authorizationAPIOption.Value;
		_tokenService = tokenService;
		_httpClient = httpClientFactory.CreateClient(AuthorizationAPIOption.APIName);
		_doLazyImport = new Lazy<Task>(ImportFromConfig);
	}

	/// <summary>
	/// Method to register configuration data in to Authorization Database
	/// </summary>
	/// <param name="importModel">Instance of Import Model</param>
	/// <returns>Completed Task</returns>
	public async Task ImportDataFromConfiguration(ImportModel? importModel = null)
	{
        if (!_authorizationApiConfig.ImportEnabled)
        {
            _logger.LogInformation("Authorization Auto Import is disabled. No data will be imported from the configuration.");
            return;
        }
        if (importModel is null)
		{
			_logger.LogInformation("Found no authorization data to import to authorization api.");
			return;
		}

		_logger.LogInformation("Sending request to Authorization API to register Permissions and Roles");
		await _tokenService.AuthorizeClient(_httpClient);
		var httpResponseMessage = await _httpClient.PostAsJsonAsync($"api/import/{_authorizationApiConfig.ExtensionName}", importModel);

		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			_logger.LogCritical("Authorization API: Error while registering Permissions and Roles");
            string failureMessage = await httpResponseMessage.Content.ReadAsStringAsync();
            _logger.LogError("Error importing data:{message}",failureMessage);
			return;
		}

		_logger.LogInformation("Authorization API successfully registered the configured Permissions and Roles");
	}

	private Task ImportFromConfig()
	{
		return ImportDataFromConfiguration(_authorizationApiConfig.Import);
	}

	/// <summary>
	/// Method to register configued Roles and Permission for the extension
	/// </summary>
	/// <returns>Completed Task</returns>
	public async Task ImportDataFromConfigLazy()
	{
        try
        {
            await _doLazyImport.Value.ConfigureAwait(false);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to import roles and permission from configuration. Reinitializing import task.");
            _doLazyImport = new Lazy<Task>(ImportFromConfig);
        }
	}

}
