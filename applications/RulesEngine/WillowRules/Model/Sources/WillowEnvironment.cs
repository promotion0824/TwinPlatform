using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Willow.Rules.Configuration;
using Azure.Identity;
using TimeZoneConverter;

namespace Willow.Rules.Sources;

/// <summary>
/// A Willow Environment = customer + dev/prod
/// </summary>
/// <remarks>
/// This provides all the necessary data source connections and repositories
///
/// This is registered as a scoped object and is resolved per request for web applications or
/// within a scope created on startup for terminal appplications. For applications that
/// run multiple environments it would be created on demand.
/// </remarks>
[DebuggerVisualizer("{id}")]
public class WillowEnvironment
{
	/// <summary>
	/// Creates a new <see cref="WillowEnvironment"/>
	/// </summary>
	public WillowEnvironment(
		CustomerOptions customerOptions)
	{
		Id = new WillowEnvironmentId(customerOptions.Id);
		Name = customerOptions.Name;

		//we could make this a setting in the future
		if (string.Equals(Name, "axa", StringComparison.OrdinalIgnoreCase))
		{
			this.LanguageCode = "fr";
		}
		else
		{
			this.LanguageCode = "en";
		}
	}

	/// <summary>
	/// Name of the Willow Environment
	/// </summary>
	/// <example>
	/// CustomerName, Dev
	/// </example>
	public string Name { get; init; }
	
	/// <summary>
	/// The language code for the customer, current defaults to "en" for English
	/// </summary>
	public string LanguageCode { get; init; }

	/// <summary>
	/// Id of the environment (short url-safe version of name)
	/// </summary>
	public WillowEnvironmentId Id { get; init; }
}
