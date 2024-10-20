using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Willow.Rules.Configuration;

namespace RulesEngine.Init;

/// <summary>
/// Telemetry initializer sets role for Application Insights
/// </summary>
public class TelemetryInitializerForInit : ITelemetryInitializer
{
	/// <summary>
	/// Creates a new <see cref="TelemetryInitializerForInit" />
	/// </summary>
	public TelemetryInitializerForInit()
	{
	}

	/// <summary>
	/// Initializes the telemetry processor, setting the cloud role name
	/// </summary>
	public void Initialize(ITelemetry telemetry)
	{
		telemetry.Context.Cloud.RoleName = RulesOptions.ProcessorCloudRoleName;
		TelemetryDebugWriter.IsTracingDisabled = true;
	}
}

/// <summary>
/// Telemetry initializer to set version from Assembly version
/// </summary>
public class VersionTelemetryInitializerForInit : ITelemetryInitializer
{
	/// <summary>
	/// Creates a new <see cref="VersionTelemetryInitializerForInit" />
	/// </summary>
	public void Initialize(ITelemetry telemetry)
	{
		var assemblyName = Assembly.GetEntryAssembly()?.GetName();
		var version = assemblyName?.Version?.ToString();
		if (!string.IsNullOrWhiteSpace(version))
		{
			telemetry.Context.Component.Version = version;
		}
	}
}
