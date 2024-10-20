namespace Willow.LiveData.Core.Infrastructure.Configuration;

internal record Auth0Configuration
{
    public string ClientId { get; init; } = string.Empty;

    public string Domain { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}
