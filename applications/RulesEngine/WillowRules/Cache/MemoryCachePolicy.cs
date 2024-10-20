namespace Willow.Rules.Cache;

/// <summary>
/// Does the disk cache also have a memory cache?
/// </summary>
public enum MemoryCachePolicy
{
	/// <summary>
	/// The disk cache does not have a memory cache
	/// </summary>
	NoMemoryCache = 0,

	/// <summary>
	/// The disck cache does have a memory cache
	/// </summary>
	WithMemoryCache = 1
}
