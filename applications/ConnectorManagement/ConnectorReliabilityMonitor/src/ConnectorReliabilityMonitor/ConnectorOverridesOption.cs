namespace Willow.ConnectorReliabilityMonitor;

internal record ConnectorOverridesOption
{
    public IEnumerable<IntervalOverrides> Overrides { get; init; } = new List<IntervalOverrides>();
}

internal record IntervalOverrides
{
    public string Name { get; init; } = string.Empty;

    public int Interval { get; init; }
}
