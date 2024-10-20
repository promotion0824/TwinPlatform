using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Twin Manager Contract
/// </summary>
public interface ITwinManager
{
    /// <summary>
    /// Get Twin Locations
    /// </summary>
    /// <returns>List of LocationTwinSlim</returns>
    public Task<List<LocationTwinSlim>> GetTwinLocationsAsync();
}
