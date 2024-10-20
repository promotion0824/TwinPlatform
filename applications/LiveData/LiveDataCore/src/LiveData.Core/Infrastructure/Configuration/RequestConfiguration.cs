namespace Willow.LiveData.Core.Infrastructure.Configuration;

internal record RequestConfiguration
{
    private const int DefaultMaxPageSize = 1000;

    public int MaxPageSize { get; init; } = DefaultMaxPageSize;
}
