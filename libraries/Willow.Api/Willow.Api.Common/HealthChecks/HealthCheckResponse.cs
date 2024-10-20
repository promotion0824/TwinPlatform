namespace Willow.Api.Common.HealthChecks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public record HealthCheckResponse
{
    public string Status { get; init; } = default!;

    public IEnumerable<HealthCheck> HealthChecks { get; init; } = default!;

    public TimeSpan Duration { get; init; }
}

public record HealthCheck(string Name, string Status, string? Description);
