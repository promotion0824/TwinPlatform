using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Willow.Model.Mapping;

public class MtiAsyncJobRequest
{
    /// <summary>
    /// The type of MTI Async job.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public MtiAsyncJobType JobType { get; set; }

    /// <summary>
    /// The Mapped building identifier.
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// The Mapped connector identifier.
    /// </summary>
    public string? ConnectorId { get; set; }
}
