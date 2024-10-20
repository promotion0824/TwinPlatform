namespace Willow.LiveData.Pipeline;

/// <summary>
/// Standard metric tags that can be passed to MetricsAttributesHelper.GetValues.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Messages processed metric.
    /// </summary>
    public const string MessagesProcessed = nameof(MessagesProcessed);

    /// <summary>
    /// Processing status tag.
    /// </summary>
    public const string StatusDimensionName = "MessageProcessingStatus";

    /// <summary>
    /// Success status tag.
    /// </summary>
    public const string SuccessStatus = "Success";

    /// <summary>
    /// Skipped status tag.
    /// </summary>
    public const string SkippedStatus = "Skipped";

    /// <summary>
    /// Parse error status tag.
    /// </summary>
    public const string ParseErrorStatus = "ParseError";

    /// <summary>
    /// Failed status tag.
    /// </summary>
    public const string FailedStatus = "Failed";

    /// <summary>
    /// Checkpoint update duration metric.
    /// </summary>
    public const string CheckpointUpdateDuration = nameof(CheckpointUpdateDuration);

    /// <summary>
    /// Action tag.
    /// </summary>
    public const string Action = nameof(Action);

    /// <summary>
    /// Status tag.
    /// </summary>
    public const string Status = nameof(Status);

    /// <summary>
    /// Processed tag.
    /// </summary>
    public const string Processed = nameof(Processed);

    /// <summary>
    /// Succeeded tag.
    /// </summary>
    public const string Succeeded = nameof(Succeeded);

    /// <summary>
    /// Failed tag.
    /// </summary>
    public const string Failed = nameof(Failed);

    /// <summary>
    /// Client error tag.
    /// </summary>
    public const string ClientError = nameof(ClientError);
}
