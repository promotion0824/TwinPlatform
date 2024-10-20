namespace Willow.LiveData.Core.Infrastructure.Configuration;

internal record TelemetryConfiguration
{
    private const decimal DefaultThresholdPercentage = 0.8M;

    public decimal ThresholdPercentage { get; init; } = DefaultThresholdPercentage;
}
