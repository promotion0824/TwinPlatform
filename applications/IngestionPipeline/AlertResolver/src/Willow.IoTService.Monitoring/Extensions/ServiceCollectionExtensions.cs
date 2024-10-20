using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using Willow.IoTService.Monitoring.Extensions;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Options;
using Willow.IoTService.Monitoring.Persistence.AzureDataTables;
using Willow.IoTService.Monitoring.Ports;
using Willow.IoTService.Monitoring.Services;
using Willow.IoTService.Monitoring.Services.AppInsights;
using Willow.IoTService.Monitoring.Services.DataTable;

namespace Willow.IoTService.Monitoring.Alerting
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAlerting(this IServiceCollection services, IConfiguration configuration)
        {
            AddAppInsights(services);
            AddMonitorMetrics(services, configuration);
            AddAlertingAzureDataTablesPersistence(services, configuration);

            if (!services.HasRegistration<AlertsService>())
            {
                services.AddSingleton<IAlertsService, AlertsService>();
            }

            services.AddSingleton<IAlertsFactory, AlertsFactory>();
            services.AddSingleton<IAlertNotificationChannelFactory, AlertNotificationChannelFactory>();

            var alertTypes = AlertsFactory.ScanForAlertTypes();

            foreach (var alertType in alertTypes)
            {
                services.AddTransient(alertType);
            }
        }

        public static AlertNotificationChannelSpec AddMicrosoftTeamsAlerting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => configuration.GetSection("Alerting:MicrosoftTeamsNotificationChannelOptions").Get<MicrosoftTeamsNotificationChannelOptions>());
            services.AddSingleton<IAlertNotificationChannel, MicrosoftTeamsNotificationChannel>();

            var isEnabled = configuration.GetValue("Alerting:MicrosoftTeamsNotificationChannelOptions:Enabled", true);

            return AlertNotificationChannelSpec.For<MicrosoftTeamsNotificationChannel>(isEnabled);
        }

        public static AlertNotificationChannelSpec AddPagerDutyEmailAlerting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => configuration.GetSection("Alerting:PagerDutyEmailNotificationChannelOptions")
                                                    .Get<PagerDutyEmailOptions>());

            services.AddSingleton<IAlertNotificationChannel, PagerDutyEmailNotificationChannel>();

            var isEnabled = configuration.GetValue("Alerting:PagerDutyEmailNotificationChannelOptions:Enabled", false);

            return AlertNotificationChannelSpec.For<PagerDutyEmailNotificationChannel>(isEnabled);
        }

        public static AlertNotificationChannelSpec AddSendGridEmailAlerting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => configuration.GetSection("Alerting:SendGridEmailNotificationChannelOptions").Get<SendGridOptions>());
            services.AddSingleton<IAlertNotificationChannel, SendGridEmailNotificationChannel>();

            var isEnabled = configuration.GetValue("Alerting:SendGridEmailNotificationChannelOptions:Enabled", true);

            return AlertNotificationChannelSpec.For<SendGridEmailNotificationChannel>(isEnabled);
        }

        public static AlertNotificationChannelSpec AddSupportNotificationChannel(this IServiceCollection services, IConfiguration configuration)
        {
            // Using SendGrid config for now
            var isEnabled = configuration.GetValue("Alerting:SendGridEmailNotificationChannelOptions:Enabled", true);
            services.AddSingleton<IAlertNotificationChannel, SupportNotificationChannel>();

            return AlertNotificationChannelSpec.For<SupportNotificationChannel>(isEnabled);
        }

        public static AlertNotificationChannelSpec AddAlertResolverNotificationChannel(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(_ => configuration.GetSection("Alerting:AlertResolverNotificationChannelOptions").Get<AlertResolverNotificationChannelOptions>());
            services.AddSingleton<IAlertNotificationChannel, AlertResolverNotificationChannel>();

            var isEnabled = configuration.GetValue("Alerting:AlertResolverNotificationChannelOptions:Enabled", false);

            return AlertNotificationChannelSpec.For<AlertResolverNotificationChannel>(isEnabled);
        }

        private static bool HasRegistration<T>(this IServiceCollection services)
        {
            return services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T)) != null;
        }

        private static void AddMonitorMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions(configuration, "HttpClientFactory")
                    .AddManagement();

            services.ConfigureClient<IMonitorMetricsService, MonitorMetricsService>(AddAzureManagementApiClient);
        }

        private static void AddAlertingAzureDataTablesPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions(configuration, "Azure")
                    .AddDataTables();

            services.AddTransient<IDataTableService, DataTableService>();
            services.AddTransient<IAlertRunHistoryRepository, AlertRunHistoryDataTablesRepository>();
            services.AddTransient<IActiveAlertRepository, ActiveAlertDataTablesRepository>();
        }

        private static void AddAppInsights(this IServiceCollection services)
        {
            services.AddSingleton<AppInsightsQueryProvider, AppInsightsQueryProvider>();
        }

        private static Action<IServiceProvider, HttpClient> AddAzureManagementApiClient
        => (serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<AzureManagementApiOptions>();
            httpClient.BaseAddress = new Uri(options.BaseAddress ?? string.Empty);
        };
    }
}