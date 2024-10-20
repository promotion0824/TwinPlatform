using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;

namespace Authorization.TwinPlatform.Web.Services;

public record TwinsApiOptions
{
    /// <summary>
    /// Array of Model Ids for Location Twin Request
    /// </summary>
    public List<string> LocationTwinModels { get; set; } = [];

    /// <summary>
    /// Location Twin Request - True: To do Exact Twin Model Match otherwise False.
    /// </summary>
    public bool LocationTwinModelExactMatch = false;
}

/// <summary>
///  Twin Manager Class
/// </summary>
/// <param name="twinsClient">Adt Api Twins Client</param>
public class TwinManager(ITwinsClient twinsClient, IOptions<TwinsApiOptions> twinsApiOptions, IMemoryCache memoryCache, ILogger<TwinManager> logger) : BaseManager, ITwinManager
{
    /// <summary>
    /// Get Twin Locations
    /// </summary>
    /// <returns>List of LocationTwinSlim</returns>
    public async Task<List<LocationTwinSlim>> GetTwinLocationsAsync()
    {
        return await memoryCache.GetOrCreateAsync(nameof(TwinManager.GetTwinLocationsAsync), async (cacheEntry) =>
        {
            try
            {
                var locationResponse = await twinsClient.GetTreesByModelAsync(twinsApiOptions.Value.LocationTwinModels, null, null, null, exactModelMatch: twinsApiOptions.Value.LocationTwinModelExactMatch);
                var response = MapToModel(locationResponse);
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return response;
            }
            catch (SocketException)
            {
                logger.LogWarning("Unable to reach adt-api service. Check your configuration and adt-api service is running.");
                return [];
            }

        }) ?? [];
    }

    private static List<LocationTwinSlim> MapToModel(ICollection<NestedTwin> nestedTwins, HashSet<string>? visitedIds = null)
    {
        List<LocationTwinSlim> result = [];
        visitedIds ??= [];
        if (nestedTwins == null || nestedTwins.Count == 0)
        {
            return result;
        }
        foreach (var nestTwin in nestedTwins)
        {
            if (nestTwin.Twin is null || visitedIds.Contains(nestTwin.Twin.Id)) // track visited Id and ignore loops
                continue;
            LocationTwinSlim locationTwin = ToLocationTwin(nestTwin.Twin);
            if (nestTwin.Children != null && nestTwin.Children.Count > 0)
            {
                locationTwin.Children = MapToModel(nestTwin.Children, visitedIds);
            }
            result.Add(locationTwin);
        }

        return result;
    }

    private static LocationTwinSlim ToLocationTwin(BasicDigitalTwin basicDigitalTwin)
    {
        return new LocationTwinSlim()
        {
            Id = basicDigitalTwin.Id,
            Name = basicDigitalTwin.Contents.TryGetValue("name", out object? name) ? (name?.ToString())! : string.Empty,
            ModelId = basicDigitalTwin.Metadata.ModelId,
        };
    }
}
