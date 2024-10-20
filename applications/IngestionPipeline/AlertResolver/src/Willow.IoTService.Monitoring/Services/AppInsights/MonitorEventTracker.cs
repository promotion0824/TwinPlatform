using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Willow.IoTService.Monitoring.Contracts;

namespace Willow.IoTService.Monitoring.Services.AppInsights;

public class MonitorEventTracker : IMonitorEventTracker
{
    private readonly ILogger<MonitorEventTracker> _logger;
    private readonly TelemetryClient _telemetryClient;

    public MonitorEventTracker(ILogger<MonitorEventTracker> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public void Execute(MonitorEvent monitorEvent)
    {
        var evt = GetEventTelemetry(monitorEvent);

        try
        {
            _telemetryClient.TrackEvent(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError("{Message}", ex.Message);
            throw;
        }
        finally
        {
            _telemetryClient.Flush();
        }
    }

    private static EventTelemetry GetEventTelemetry(MonitorEvent monitorEvent)
    {
        var eventTelemetry = new EventTelemetry(monitorEvent.MonitorSource.ToString())
        {
            ProactiveSamplingDecision = SamplingDecision.None
        };
        var nonEmptyProperties = GetProperties(monitorEvent);
        var customProperties = monitorEvent.CustomProperties ?? new Dictionary<string, string>();
        var customMetrics = monitorEvent.Metrics ?? new Dictionary<string, double>();

        foreach (var prop in nonEmptyProperties.Where(prop => !string.IsNullOrWhiteSpace(prop.Value)))
        {
            if (customProperties.ContainsKey(prop.Key))
            {
                continue;
            }
            eventTelemetry.Properties.Add(prop);
        }
        foreach (var customProperty in customProperties)
        {
            eventTelemetry.Properties.Add(customProperty.Key, customProperty.Value);
        }
        foreach (var customMetric in customMetrics)
        {
            eventTelemetry.Metrics.Add(customMetric.Key, customMetric.Value);
        }
        return eventTelemetry;
    }

    private static Dictionary<string, string> GetProperties(MonitorEvent monitorEvent)
    {
        //Skip customProperties and metrics as they are added separately
        return monitorEvent.GetType()
                           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                           .Where(prop => prop.Name != nameof(MonitorEvent.CustomProperties) &&
                                          prop.Name != nameof(MonitorEvent.Metrics))
                           .ToDictionary(prop => prop.Name,
                                         prop => prop.GetValue(monitorEvent,
                                                               null)
                                                    ?.ToString() ??
                                                 "");
    }
}
