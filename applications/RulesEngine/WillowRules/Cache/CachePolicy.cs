namespace Willow.Rules.Cache;

/// <summary>
/// Disk cache policy
/// </summary>
public enum CachePolicy
{
	/// <summary>
	/// Cache requests must wait for latest data
	/// </summary>
	EagerReload = 1,

	/// <summary>
	/// Cache requests can use current data and lazily update it in the background
	/// ready for the next request to arrive
	/// </summary>
	LazyReload = 2
}
