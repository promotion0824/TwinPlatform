using System.Text.Json.Serialization;

namespace Willow.DataQuality.Model.Capability;

/// <summary>
/// A capability status of a twin
/// </summary>
public class CapabilityStatusDto
{
    /// <summary>
    /// Id of the twin
    /// </summary>
    /// <remarks>
    /// It can be used with TrendId, ConnectorId, and ExternalId
    /// </remarks>
    public string? TwinId { get; set; }

    /// <summary>
    /// TrendId
    /// </summary>
    /// <remarks>
    /// It can be used with TwinId, ConnectorId, and ExternalId
    /// </remarks>
    public Guid? TrendId { get; set; }

    /// <summary>
    /// ConnectorID + ExternalID is used on some time series values instead of trendId
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// ExternalId is used on some time series values instead of trendId
    /// </summary>
    /// <remarks>
    /// When it is used we also need to check the connector id
    /// </remarks>
    public string? ExternalId { get; set; }

    /// <summary>
    /// The list of capability statuses reported
    /// </summary>
    public List<StatusType> Status { get; set; } = new List<StatusType>();

    /// <summary>
    /// The reported datetime of the twin's capability status in utc
    /// </summary>
    public DateTime ReportedDateTime { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusType
{
    /// <summary>
    /// The status is healthy
    /// </summary>
    Ok,

    /// <summary>
    /// The value is above the maximum or below the minimum for this type
    /// </summary>
    IsValueOutOfRange,

    /// <summary>
    /// The period is too far from the expected interval
    /// </summary>
    IsPeriodOutOfRange,

    /// <summary>
    /// The sensor has been stuck on the same value for a long time
    /// </summary>
    IsStuck,

    /// <summary>
    /// The sensor appears to be offline, no data has been received for > 2 x expected period
    /// </summary>
    IsOffline,
}
