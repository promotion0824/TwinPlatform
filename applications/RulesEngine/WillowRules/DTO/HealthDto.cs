using System;

namespace Willow.Rules.DTO;

/// <summary>
/// Health status of application
/// </summary>
public class HealthDto
{
	private static DateTimeOffset startTime = DateTimeOffset.Now;

	/// <summary>
	/// Uptime
	/// </summary>
	public TimeSpan Uptime => DateTimeOffset.Now - startTime;

	/// <summary>
	/// Name of application
	/// </summary>
	public string App { get; init; } = "";

	/// <summary>
	/// Can we connect to ServiceBus
	/// </summary>
	public HealthStatus ServiceBus { get; init; }

	/// <summary>
	/// Can we connect to SQL?
	/// </summary>
	public HealthStatus Database { get; init; }

	/// <summary>
	/// Can we connect to ADT?
	/// </summary>
	public HealthStatus ADTInstances { get; init; }

	/// <summary>
	/// Can we connect to ADX?
	/// </summary>
	public HealthStatus ADXInstance { get; init; }

	/// <summary>
	/// Assembly version of the entry assembly
	/// </summary>
	public string Version { get; init; } = "";
}