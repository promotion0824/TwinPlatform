namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Microsoft.Azure.Devices;
using Newtonsoft.Json.Linq;

/// <inheritdoc />
public class BaseModuleTransformer : IBaseModuleTransformer
{
    private const string PropertyNamePrefix = "properties.desired";

    /// <inheritdoc />
    public void Transform(ConfigurationContent content)
    {
        if (!content.ModulesContent.TryGetValue("$edgeAgent", out var edgeAgentDict) ||
            !edgeAgentDict.TryGetValue(PropertyNamePrefix, out var desiredProperties) ||
            desiredProperties is not JObject desiredPropertiesJObj)
        {
            return;
        }

        var modulesObj = desiredPropertiesJObj["systemModules"];
        if (modulesObj is not JObject modulesJObj)
        {
            return;
        }

        foreach (var module in modulesJObj.Properties().Select(x => x.Value))
        {
            var settingsObj = module["settings"];

            if (settingsObj?["createOptions"] is not JObject createOptionsJObj)
            {
                continue;
            }

            settingsObj["createOptions"] = createOptionsJObj.ToString()
                                                            .Replace("\r\n", string.Empty)
                                                            .Replace("\n", string.Empty);
        }
    }
}
