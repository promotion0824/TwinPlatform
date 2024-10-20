namespace Willow.PublicApi.Config;

using Willow.Api.Authentication;

internal class B2CConfig : AzureAdB2COptions
{
    public required string Audience { get; init; }

    public required IEnumerable<string> B2CScopes { get; init; }
}
