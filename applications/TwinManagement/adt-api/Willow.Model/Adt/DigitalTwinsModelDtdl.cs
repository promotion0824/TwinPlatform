using Newtonsoft.Json;
using Willow.Model.Serialization;

namespace Willow.Model.Adt;

public class DigitalTwinsModelDtdl
{
    [JsonProperty("@id")]
    public string id { get; set; } = string.Empty;

    public DigitalTwinsModelContent[] contents { get; set; } = Array.Empty<DigitalTwinsModelContent>();

    [JsonConverter(typeof(StringCollectionConverter))]
    public string[] extends { get; set; } = Array.Empty<string>();
}
