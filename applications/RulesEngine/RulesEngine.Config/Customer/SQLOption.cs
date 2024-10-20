// Used for IOptions
#nullable disable

using System;

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Configuration for a SQL connection
/// </summary>
public class SqlOption
{
	/// <summary>
	/// Connection string
	/// </summary>
	public string ConnectionString { get; set; }

	/// <summary>
	/// The amount of time items are kept in memory cache
	/// </summary>
	public TimeSpan CacheExpiration { get; set; } = TimeSpan.MaxValue;
}