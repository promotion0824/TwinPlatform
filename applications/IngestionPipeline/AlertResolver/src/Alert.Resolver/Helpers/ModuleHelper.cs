using Newtonsoft.Json;
using Willow.Alert.Resolver.Transformers;

namespace Willow.Alert.Resolver.Helpers;

/// <summary>
/// Module helper.
/// </summary>
internal interface IModuleHelper
{
    /// <summary>
    /// Get dependent modules.
    /// </summary>
    /// <param name="connectorType">Type of the connector.</param>
    /// <param name="connectorName">Name of the connector.</param>
    /// <returns></returns>
    IEnumerable<string> GetDependentModules(string connectorType, string connectorName);
    /// <summary>
    /// Construct restart payloads.
    /// </summary>
    /// <param name="modules">Names of the modules.</param>
    /// <returns></returns>
    IEnumerable<string> GetRestartPayloads(IEnumerable<string> modules);
}

/// <inheritdoc />
public sealed class ModuleHelper : IModuleHelper
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ModuleHelper> _logger;
    private readonly IModuleNameTransformer _moduleNameTransformer;

    /// <summary>
    /// Constructor of ModuleHelper.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="moduleNameTransformer">Module name transformer.</param>
    /// <param name="configuration">Configuration.</param>
    public ModuleHelper(ILogger<ModuleHelper> logger,
                        IModuleNameTransformer moduleNameTransformer,
                        IConfiguration configuration)
    {
        _logger = logger;
        _moduleNameTransformer = moduleNameTransformer;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetDependentModules(string connectorType, string connectorName)
    {
        var modules = new List<string>();
        var moduleName = _moduleNameTransformer.Transform(connectorType, connectorName);
        var dependentModules = _configuration.GetValue<string>($"DependsOn:{connectorType}")?
           .Split(",");
        if (dependentModules is not null &&
            dependentModules.Length > 0)
            modules.AddRange(dependentModules);
        modules.Add(moduleName);
        return modules;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetRestartPayloads(IEnumerable<string> modules)
    {
        return modules.Select(GetRestartPayload).ToList();
    }

    private string GetRestartPayload(string moduleName)
    {
        var payload = new { schemaVersion = "1.0", id = moduleName };
        _logger.LogInformation("RestartModule Payload: {Payload}", payload);
        return JsonConvert.SerializeObject(payload);
    }
}
