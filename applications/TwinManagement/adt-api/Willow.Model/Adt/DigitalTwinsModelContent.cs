using Newtonsoft.Json;
using Willow.Model.Serialization;

namespace Willow.Model.Adt;

public class DigitalTwinsModelContent
{
    [JsonProperty("@type")]
    [JsonConverter(typeof(StringCollectionConverter))]
    public string[] type { get; set; } = Array.Empty<string>();

    public string? name { get; set; }

    public string? target { get; set; }
}
