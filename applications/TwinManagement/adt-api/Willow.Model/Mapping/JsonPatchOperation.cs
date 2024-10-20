using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Willow.Model.Mapping;

/// <summary>
/// represent a JSON Patch operation
/// </summary>
public class JsonPatchOperation
{
    [JsonProperty("op")]
    [JsonConverter(typeof(StringEnumConverter))]
    public OperationType Op { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("value")]
    public object? Value { get; set; }
}

public enum OperationType
{
    Add,
    Remove,
    Replace,
    Move,
    Copy,
    Test
}
