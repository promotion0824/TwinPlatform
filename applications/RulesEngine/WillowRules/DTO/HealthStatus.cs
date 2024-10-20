namespace Willow.Rules.DTO;

/// <summary>
/// Health status value
/// </summary>
public enum HealthStatus
{
	/// <summary>
	/// Healthy: reachable and authorized.
	/// </summary>
	OK = 0,

	/// <summary>
	/// Doesn't seem to be configured
	/// </summary>
	NotConfigured = 1,

	/// <summary>
	/// Cannot reach it
	/// </summary>
	NotReachable = 2,

	/// <summary>
	/// Missing data (e.g. cache, required database entries, ...)
	/// </summary>
	NoData = 3,

	/// <summary>
	/// Unknwon (not implemented yet)
	/// </summary>
	Unknown = 999
}