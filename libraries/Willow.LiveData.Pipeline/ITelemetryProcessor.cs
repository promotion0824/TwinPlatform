namespace Willow.LiveData.Pipeline;

/// <summary>
/// Processes an incoming telemetry message using the default telemetry schema.
/// </summary>
public interface ITelemetryProcessor : ITelemetryProcessor<Telemetry>
{
}

/// <summary>
/// Processes an incoming telemetry message.
/// </summary>
/// <typeparam name="TTelemetry">The type of telemetry message this processor supports.</typeparam>
public interface ITelemetryProcessor<TTelemetry>
{
    /// <summary>
    /// Processes the given telemetry.
    /// </summary>
    /// <param name="telemetry">The telemetry to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(TTelemetry telemetry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes the given telemetry.
    /// </summary>
    /// <param name="batch">The batch of telemetry to process.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<TTelemetry> batch, CancellationToken cancellationToken = default);
}
