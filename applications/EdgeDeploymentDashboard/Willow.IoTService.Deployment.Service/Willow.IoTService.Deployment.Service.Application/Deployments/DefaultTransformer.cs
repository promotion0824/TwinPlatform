namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;
using Newtonsoft.Json.Linq;
using Willow.IoTService.Deployment.Common.Messages;

/// <inheritdoc />
public class DefaultTransformer : IDefaultTransformer
{
    private const string ModulePropertyNamePrefix = "properties.desired.modules";

    /// <inheritdoc />
    public void Transform(ConfigurationContent content, IReadOnlyDictionary<string, IContainerConfiguration>? configs = null)
    {
        if (configs == null || !configs.Any())
        {
            return;
        }

        foreach (var (moduleName, configuration) in configs)
        {
            var modulePropertyName = $"{ModulePropertyNamePrefix}.{moduleName}";
            if (!content.ModulesContent.TryGetValue("$edgeAgent", out var edgeAgentDict) ||
                !edgeAgentDict.TryGetValue(modulePropertyName, out var moduleProperty) ||
                moduleProperty is not JObject modulePropertyJObj)
            {
                return;
            }

            var settingsObj = modulePropertyJObj["settings"];

            if (configuration.EnvironmentVariables != null && settingsObj?["createOptions"] is JObject createOptionsJObj)
            {
                var jArray = new JArray();
                foreach (var envString in configuration.EnvironmentVariables)
                {
                    jArray.Add(envString);
                }

                createOptionsJObj["Env"] = jArray;

                // stringify the json object to meet deployment manifest format
                settingsObj["createOptions"] = createOptionsJObj.ToString()
                                                                .Replace("\r\n", string.Empty)
                                                                .Replace("\n", string.Empty);
            }

            if (configuration.Image != null && settingsObj != null)
            {
                settingsObj["image"] = configuration.Image;
            }

            if (configuration.RunState != null)
            {
                modulePropertyJObj["status"] = configuration.RunState.ToString();
            }
        }
    }
}
