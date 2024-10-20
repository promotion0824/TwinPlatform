// Used for IOptions
#nullable disable

using System;

namespace Willow.Rules.Configuration.Customer;

/// <summary>
/// Configuration for Rule Execution
/// </summary>
public class ExecutionOption
{
	/// <summary>
	/// Configure to run every N times independent from how long it takes to process a window of time series data
	/// </summary>
	public TimeSpan RunFrequency { get; set; } = TimeSpan.FromMinutes(16);

	/// <summary>
	/// Configure 'settling' interval as not all connectors will deliver data in perfect synchronization. 
	/// </summary>
	public TimeSpan SettlingInterval { get; set; } = TimeSpan.FromMinutes(30);

	/// <summary>
	/// A ratio between 0-1 to lower parallelism for expansion
	/// </summary>
	public double ExpansionParallelismRatio { get; set; } = 1;
}
