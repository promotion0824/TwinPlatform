using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.IoTService.Monitoring.Options
{
    public static class OptionsBuilderExtensions
    {
        private const string DataTablesKey = "DataTables";
        private const string AzureManagementApiKey = "AzureManagementApi";

        public static IOptionBuilder AddOptions(this IServiceCollection services, IConfiguration configuration, string? baseKey = null)
        {
            return new OptionBuilder(services, configuration, baseKey);
        }

        public static IOptionBuilder AddDataTables(this IOptionBuilder builder, string key = DataTablesKey)
        {
            return builder.AddDataTables<DataTableOptions>(key);
        }

        private static IOptionBuilder AddDataTables<T>(this IOptionBuilder builder, string key = DataTablesKey) where T : DataTableOptions
        {
            builder.Services.AddTransient(_ => builder.Configuration.GetSection(CombinedKey(builder.BaseKey ?? string.Empty, key)).Get<T>());
            return builder;
        }

        public static IOptionBuilder AddManagement(this IOptionBuilder builder, string key = AzureManagementApiKey)
        {
            return builder.AddManagement<AzureManagementApiOptions>(key);
        }

        private static IOptionBuilder AddManagement<T>(this IOptionBuilder builder, string key = AzureManagementApiKey) where T : AzureManagementApiOptions
        {
            builder.Services.AddTransient(_ => builder.Configuration.GetSection(CombinedKey(builder.BaseKey ?? string.Empty, key)).Get<T>());
            return builder;
        }

        private static string CombinedKey(string baseKey, string key) => string.Join(":", new[] { baseKey, key }.Where(s => !string.IsNullOrEmpty(s)));
    }
}