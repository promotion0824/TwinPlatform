using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Options;
using Microsoft.Graph;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface for providing Microsoft Graph API Client
/// </summary>
public interface IGraphApplicationClientService
{

	/// <summary>
	/// Gets a singleton instance of the GraphServiceClient
	/// </summary>
	/// <returns>GraphServiceClient</returns>
	public GraphServiceClient GetGraphServiceClient();

    /// <summary>
    /// Get Graph Client Configuration
    /// </summary>
    public GraphApplicationOptions GraphConfiguration { get; }

    /// <summary>
    /// Gets the health check instance for Graph Application Client
    /// </summary>
    public HealthCheckAD HealthCheckInstance { get; }
}
