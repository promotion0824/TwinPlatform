namespace Willow.LiveData.Pipeline;

/// <summary>
/// Sends telemetry to a destination.
/// </summary>
/// <typeparam name="TTelemetry">The type of telemetry message this sender supports.</typeparam>
public interface ISender<TTelemetry>
{
    /// <summary>
    /// Sends the given telemetry to the pipeline destination.
    /// </summary>
    /// <param name="telemetry">Telemetry.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Task.</returns>
    /// <exception cref="PipelineException">Thrown if the sender fails to send.</exception>
    Task SendAsync(TTelemetry telemetry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the given telemetry to the pipeline destination.
    /// </summary>
    /// <param name="telemetry">Telemetry.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Task.</returns>
    /// <exception cref="PipelineException">Thrown if the sender fails to send.</exception>
    Task SendAsync(IEnumerable<TTelemetry> telemetry, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sends message using the default telemetry schema.
/// </summary>
public interface ISender : ISender<Telemetry>
{
}
