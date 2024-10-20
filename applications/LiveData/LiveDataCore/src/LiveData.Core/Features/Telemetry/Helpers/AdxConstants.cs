namespace Willow.LiveData.Core.Features.Telemetry.Helpers;

internal static class AdxConstants
{
    public const string ActiveTwinsFunction = "ActiveTwins";
    public const string TelemetryTable = "Telemetry";
    public const string TwinsId = "Id";
    public const string SiteId = "SiteId";
}

internal static class TelemetryTable
{
    public const string ExternalId = "ExternalId";
    public const string TrendId = "TrendId";
    public const string SourceTimestamp = "SourceTimestamp";
    public const string EnqueuedTimestamp = "EnqueuedTimestamp";
    public const string TelemetryField = "ScalarValue";
}

internal static class DataQualityTelemetryTable
{
    public const string ConnectorId = "ConnectorId";
    public const string DtId = "DtId";
    public const string ExternalId = "ExternalId";
    public const string SourceTimestamp = "SourceTimestamp";
    public const string EnqueuedTimestamp = "EnqueuedTimestamp";
    public const string ValidationResults = "ValidationResults";
    public const string LastValidationUpdatedAt = "LastValidationUpdatedAt";
}
