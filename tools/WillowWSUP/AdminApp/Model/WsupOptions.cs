namespace Willow.AdminApp;

using System.Reflection;
using Willow.AppContext;

/// <summary>
/// The options for the Willow Service Utility Provider.
/// </summary>
public class WsupOptions
{
    public WsupOptions()
    {
        string name = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";

        MeterOptions = new WillowMeterOptions
        {
            Name = name,
            Version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown",
        };
    }

    /// <summary>
    /// Gets the meter options.
    /// </summary>
    public WillowMeterOptions MeterOptions { get; }
}
