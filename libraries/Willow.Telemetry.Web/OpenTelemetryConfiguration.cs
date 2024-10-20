namespace Willow.Telemetry.Web
{
    using System.Diagnostics;
    using Azure.Monitor.OpenTelemetry.Exporter;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using Willow.AppContext;

    /// <summary>
    /// Extension which sets up OpenTelemetry for the application.
    /// </summary>
    public static class OpenTelemetryConfiguration
    {
        private static readonly string DefaultReplicaName = "01";

        /// <summary>
        /// Configures OpenTelemetry for the application.
        /// </summary>
        /// <param name="services">The app service collection.</param>
        /// <param name="configuration">The configuration of the application.</param>
        /// <returns>The complete services collection.</returns>
        public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var willowContext = configuration.GetSection("WillowContext").Get<WillowContextOptions>();

            // Load either from the AppInsights section or from the environment variables
            string? cloudRoleName = configuration.GetValue<string>("ApplicationInsights:CloudRoleName") ?? willowContext?.AppName ?? "Unknown";
            string? auditLogConnectionString = configuration.GetValue<string>("ApplicationInsights:AuditConnectionString");
            string? telemetryConnectionString = configuration.GetValue<string>("ApplicationInsights:ConnectionString");

            var replicaName = Environment.GetEnvironmentVariable("CONTAINER_APP_REPLICA_NAME");

            // OpenTelemetry configuration
            var addConsoleExporter = configuration.GetValue<bool?>("OpenTelemetry:AddConsoleExporter") ?? false;
            var addHttpClientInstrumentation = configuration.GetValue<bool?>("OpenTelemetry:AddHttpClientInstrumentation") ?? false;
            var addAspNetCoreInstrumentation = configuration.GetValue<bool?>("OpenTelemetry:AddAspNetCoreInstrumentation") ?? false;
            var addSqlClientInstrumentation = configuration.GetValue<bool?>("OpenTelemetry:AddSqlClientInstrumentation") ?? false;
            var samplingRatio = configuration.GetValue<float?>("OpenTelemetry:SamplingRatio") ?? 1.0f;

            if (string.IsNullOrEmpty(replicaName))
            {
                replicaName = DefaultReplicaName;
            }

            var resourceAttributes = new Dictionary<string, object>()
            {
                {
                    "service.name", cloudRoleName
                },
            };

            if (willowContext != null)
            {
                resourceAttributes.Add("service.instance.id", willowContext.AppRoleInstanceId);

                foreach (var val in willowContext.Values)
                {
                    resourceAttributes.Add(val.Key, val.Value);
                }
            }

            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    // Set the name of the Activity Source to listen by the trace provider
                    // This name must be same as the ActivitySource created by the calling application
                    // eg: var activitySource = new ActivitySource("sourceName")
                    builder.AddSource(cloudRoleName);

                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes));

                    if (willowContext != null)
                    {
                        builder.AddProcessor(new WillowActivityEnrichProcessor(willowContext));
                    }

                    if (addHttpClientInstrumentation)
                    {
                        var contextEnricher = (Activity activity, object request) =>
                        {
                            if (willowContext is null)
                            {
                                return;
                            }

                            foreach (var prop in willowContext.Values)
                            {
                                activity.SetTag(prop.Key, prop.Value);
                            }
                        };
                        builder.AddHttpClientInstrumentation(x =>
                        {
                            x.EnrichWithHttpRequestMessage = contextEnricher;
                            x.EnrichWithHttpResponseMessage = contextEnricher;
                            x.EnrichWithException = contextEnricher;
                            x.FilterHttpRequestMessage = (request) =>
                            {
                                return !request.RequestUri?.AbsolutePath.ToString().EndsWith("healthz") ?? false;
                            };
                        });
                    }

                    if (addAspNetCoreInstrumentation)
                    {
                        builder.AddAspNetCoreInstrumentation((options) => options.Filter = httpContext =>
                        {
                            // Do not collect logs for healthz, livez, readyz methods
                            return !httpContext.Request.Path.ToString().Contains("healthz", StringComparison.InvariantCultureIgnoreCase) &&
                                   !httpContext.Request.Path.ToString().Contains("readyz", StringComparison.InvariantCultureIgnoreCase) &&
                                   !httpContext.Request.Path.ToString().Contains("livez", StringComparison.InvariantCultureIgnoreCase);
                        });
                    }

                    if (addSqlClientInstrumentation)
                    {
                        builder.AddSqlClientInstrumentation();
                    }

                    if (!string.IsNullOrEmpty(telemetryConnectionString))
                    {
                        builder.AddAzureMonitorTraceExporter(o =>
                            {
                                o.ConnectionString = telemetryConnectionString;
                                o.Diagnostics.IsDistributedTracingEnabled = true;
                                o.SamplingRatio = samplingRatio;
                            });
                    }

                    if (addConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }
                })
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                            .AddAttributes(resourceAttributes))
#if NET8_0_OR_GREATER
                        .AddMeter(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown")
#endif
                        .AddAspNetCoreInstrumentation();

                    if (!string.IsNullOrEmpty(telemetryConnectionString))
                    {
                        builder.AddAzureMonitorMetricExporter(o =>
                        {
                            o.ConnectionString = telemetryConnectionString;
                        });
                    }
                });

            services.AddLogging((options) => options.AddConfiguration(configuration)
                                                    .AddOpenTelemetry(option =>
            {
                option.SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes));
                option.IncludeScopes = true;
                option.ParseStateValues = true;
                option.IncludeFormattedMessage = true;

                if (willowContext != null)
                {
                    option.AddProcessor(new WillowLogRecordEnrichProcessor(willowContext));
                }

                if (!string.IsNullOrEmpty(telemetryConnectionString))
                {
                    option.AddAzureMonitorLogExporter(o =>
                    {
                        o.ConnectionString = telemetryConnectionString;
                    });
                }
            }));

            services.TryAddSingleton(serviceProvider =>
            {
                var auditLoggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddOpenTelemetry(option =>
                        {
                            option.SetResourceBuilder(ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes));
                            option.IncludeScopes = true;
                            option.ParseStateValues = true;
                            option.IncludeFormattedMessage = true;

                            if (willowContext != null)
                            {
                                option.AddProcessor(new WillowLogRecordEnrichProcessor(willowContext));
                            }

                            if (!string.IsNullOrEmpty(auditLogConnectionString))
                            {
                                option.AddAzureMonitorLogExporter(o =>
                                {
                                    o.ConnectionString = auditLogConnectionString;
                                });
                            }
                        });
                });
                return new AuditLoggerFactory(auditLoggerFactory);
            });

            services.TryAddSingleton(typeof(IAuditLogger<>), typeof(AuditLogger<>));

            return services;
        }
    }
}
