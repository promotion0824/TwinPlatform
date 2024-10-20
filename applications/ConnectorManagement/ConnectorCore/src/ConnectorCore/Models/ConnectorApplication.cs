namespace ConnectorCore.Models;

internal record ConnectorApplication
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public bool IsEnabled { get; set; }

    public int Interval { get; set; } = 300;

    public required string ConnectorType { get; init; }
}
