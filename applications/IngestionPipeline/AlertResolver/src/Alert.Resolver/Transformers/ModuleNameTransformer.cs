using System.Text.RegularExpressions;

namespace Willow.Alert.Resolver.Transformers;

/// <summary>
/// Module name transfomer.
/// </summary>
public interface IModuleNameTransformer
{
    /// <summary>
    /// Transform to a known module name.
    /// </summary>
    /// <param name="connectorType">Type of the connector.</param>
    /// <param name="connectorName">Name of the connector.</param>
    /// <returns></returns>
    string Transform(string connectorType, string connectorName);
}

/// <inheritdoc />
public sealed class ModuleNameTransformer : IModuleNameTransformer
{
    private static readonly Regex _moduleTypeRegex = new("(chipkinbacnet|bacnet|modbus|opcua)", RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public string Transform(string connectorType, string connectorName)
    {
        return GetContainerModuleName(connectorType, connectorName);
    }

    private static string GetModulePropertyName(string moduleType)
    {
        // check contains bacnet or modbus or opcua, ignore case
        // if match CBACNET, return CBacnetConnectorModule
        // if match BACNET, return BacnetConnectorModule
        // if match OPCUA, return OpcuaConnectorModule
        // if match MODBUS, return ModbusConnectorModule
        var match = _moduleTypeRegex.Match(moduleType);
        return match.Value.ToUpperInvariant() switch
        {
            "CHIPKINBACNET" => "CBacnetConnectorModule",
            "BACNET" => "BacnetConnectorModule",
            "MODBUS" => "ModbusConnectorModule",
            "OPCUA" => "OpcuaConnectorModule",
            _ => throw new ArgumentException($"Invalid module type: {match.Value}")
        };
    }

    private static string GetContainerModuleName(string moduleType, string connectorName)
    {
        //https://stackoverflow.com/questions/42642561/docker-restrictions-regarding-naming-container
        // Container name must start with a letter or number, and can contain only letters, numbers, dashes, underscores and dots.
        var validContainerName = Regex.Replace(connectorName, "[^a-zA-Z0-9_.-]", string.Empty);
        return $"{validContainerName}-{GetModulePropertyName(moduleType)}";
    }
}
