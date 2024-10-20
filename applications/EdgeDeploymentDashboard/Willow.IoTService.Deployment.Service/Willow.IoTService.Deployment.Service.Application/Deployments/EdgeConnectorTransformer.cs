namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using System.Text.RegularExpressions;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Willow.IoTService.Deployment.Common.Messages;

/// <inheritdoc />
public partial class EdgeConnectorTransformer(
    ILogger<EdgeConnectorTransformer> logger,
    IEdgeConnectorEnvService edgeConnectorEnvService)
    : IEdgeConnectorTransformer
{
    private const string ModulePropertyNamePrefix = "properties.desired.modules";

    // regex contains "casbacnetrpc", "bacnet" or "modbus" or "opcua" or "opcda", ignore case
    private static readonly Regex ModuleTypeRegex = EdgeConnectorRegex();

    /// <inheritdoc />
    public bool CanTransform(string moduleType)
    {
        return ModuleTypeRegex.IsMatch(moduleType);
    }

    /// <inheritdoc />
    public async Task TransformAsync(EdgeConnectorTransformConfig config, CancellationToken cancellationToken = default)
    {
        var (content, connectorId, connectorName, platform, moduleType, environment, configs) = config;
        logger.LogInformation("Transforming edge connector manifest");
        var (moduleName, configuration) = configs.FirstOrDefault(x => ModuleTypeRegex.IsMatch(x.Key));
        moduleName ??= GetModulePropertyName(moduleType);
        var containerModuleName = GetContainerModuleName(moduleType, connectorName);

        var modulePropertyName = $"{ModulePropertyNamePrefix}.{moduleName}";
        if (!content.ModulesContent.TryGetValue(
                                                "$edgeAgent",
                                                out var edgeAgentDict) ||
            !edgeAgentDict.TryGetValue(
                                       modulePropertyName,
                                       out var moduleProperty) ||
            moduleProperty is not JObject modulePropertyJObj)
        {
            return;
        }

        // Replace the module name key in edgeAgentDict
        // Having the module name include the connector name allows for multiple deployments of same connector type
        edgeAgentDict.Remove(modulePropertyName);
        edgeAgentDict.Add(containerModuleName, moduleProperty);

        var settingsObj = modulePropertyJObj["settings"];

        if (settingsObj?["createOptions"] is JObject createOptionsJObj)
        {
            // No support for environment variables in CASBACnetRPC manifest for now
            if (moduleType != "CASBACnetRPC")
            {
                var envs = await edgeConnectorEnvService.GetEdgeConnectorEnvs(
                                                                        connectorId,
                                                                        connectorName,
                                                                        environment,
                                                                        cancellationToken);
                var jArray = new JArray();
                foreach (var env in envs)
                {
                    jArray.Add(env);
                }

                createOptionsJObj["Env"] = jArray;
            }

            // stringify the json object to meet deployment manifest format
            settingsObj["createOptions"] = createOptionsJObj.ToString()
                                                            .Replace("\r\n", string.Empty)
                                                            .Replace("\n", string.Empty);
        }

        if (configuration?.Image != null && settingsObj != null)
        {
            settingsObj["image"] = configuration.Image;
        }

        if (settingsObj?["image"] != null)
        {
            settingsObj["image"] = settingsObj["image"]!.ToString()
                                                        .Replace("${Platform}", platform);
        }

        if (configuration?.RunState != null)
        {
            modulePropertyJObj["status"] = configuration.RunState.ToString();
        }
    }

    private static string GetModulePropertyName(string moduleType)
    {
        // check contains casbacnet, bacnet or modbus or opcua, ignore case
        // if match CASBAcnet, return CASBACnetRPC
        // if match CBACNET, return CBacnetConnectorModule
        // if match BACNET, return BacnetConnectorModule
        // if match OPCUA, return OpcuaConnectorModule
        // if match MODBUS, return ModbusConnectorModule
        var match = ModuleTypeRegex.Match(moduleType);
        return match.Value.ToUpperInvariant() switch
        {
            "CASBACNETRPC" => "CASBACnetRPC",
            "CHIPKINBACNET" => "CBacnetConnectorModule",
            "BACNET" => "BacnetConnectorModule",
            "MODBUS" => "ModbusConnectorModule",
            "OPCUA" => "OpcuaConnectorModule",
            _ => throw new ArgumentException($"Invalid module type: {match.Value}"),
        };
    }

    private static string GetContainerModuleName(string moduleType, string connectorName)
    {
        //https://stackoverflow.com/questions/42642561/docker-restrictions-regarding-naming-container
        // Container name must start with a letter or number, and can contain only letters, numbers, dashes, underscores and dots.
        var validContainerName = Regex.Replace(connectorName, "[^a-zA-Z0-9_.-]", string.Empty);
        return moduleType == "CASBACnetRPC" ? $"{ModulePropertyNamePrefix}.{moduleType}" : $"{ModulePropertyNamePrefix}.{validContainerName}-{GetModulePropertyName(moduleType)}";
    }

    [GeneratedRegex("(casbacnetrpc|chipkinbacnet|bacnet|modbus|opcua)", RegexOptions.IgnoreCase, "en-AU")]
    private static partial Regex EdgeConnectorRegex();
}

/// <summary>
///     Configuration for the edge connector transformation.
/// </summary>
/// <param name="Content">IoT Hub deployment configuration.</param>
/// <param name="ConnectorId">ConnectorId for deployment.</param>
/// <param name="ConnectorName">Connector Name for the deployment.</param>
/// <param name="Platform">Platform for deployment. Eg: amd64, arm64v8.</param>
/// <param name="ModuleType">Module type for deployment.</param>
/// <param name="Environment">Environment variables for deployment.</param>
/// <param name="Configs">Dictionary of container configs for deployment.</param>
public record EdgeConnectorTransformConfig(
    ConfigurationContent Content,
    string ConnectorId,
    string ConnectorName,
    string Platform,
    string ModuleType,
    string? Environment,
    IReadOnlyDictionary<string, IContainerConfiguration> Configs);
