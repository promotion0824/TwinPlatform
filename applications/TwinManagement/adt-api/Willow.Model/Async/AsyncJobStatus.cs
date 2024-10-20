
using System.Text.Json.Serialization;

namespace Willow.Model.Async;

/// <summary>
/// Status of Async jobs; DO NOT change the order of Status and new values to be added at the end
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AsyncJobStatus
{
    Queued,
    Processing,
    Done,
    Error,
    Canceled,
    Aborted,
    CancelPending,
    DeletePending
}
