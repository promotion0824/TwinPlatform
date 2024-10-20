namespace Willow.CommandAndControl.Application.Services.Abstractions;

internal interface ITwinInfoService
{
    Task<double?> GetPresentValueAsync(string connectorId, string externalId);

    Task<IDictionary<string, double?>> GetPresentValueAsync(IEnumerable<string> externalIds);

    Task<IDictionary<string, IEnumerable<LocationTwin>>> GetTwinLocationInfoAsync(IEnumerable<string> twinIds);

    Task<IDictionary<string, TwinInfoModel>> GetTwinInfoAsync(IReadOnlyCollection<string> externalIds);
}
