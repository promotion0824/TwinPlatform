using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace PlatformPortalXL.Infrastructure
{
    /// <summary>
    /// Extracts the value with the specified key and converts it to type TConfig
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static TConfig GetConfigValue<TConfig>(this IConfiguration configuration, string key) where TConfig : class, new()
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var configurationSection = configuration.GetSection(key);
            if(configurationSection == null)
            {
                throw new ArgumentNullException(nameof(key), $"{nameof(configurationSection)} section could not be loaded.");
            }

            var configurationObject = new TConfig();
            configurationSection.Bind(configurationObject);
            return configurationObject;
        }

        /// <summary>
        /// Adds Environment Specific (based on AuthorizationAPI:Import:InstanceType) configuration json file to configuration builder
        /// </summary>
        /// <param name="manager">Instance of Configuration Builder</param>
        /// <param name="environmentName">The name of the environment</param>
        /// <remarks>
        /// Methods assumes the configuration file names in format "usermanagement.import.{0}.json", 0 => AuthorizationAPI:Import:InstanceType
        /// </remarks>
        public static IConfigurationBuilder AddUserManagementConfiguration(this IConfigurationBuilder manager, string environmentName)
        {
            var envConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                             .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                                                             .AddEnvironmentVariables()
                                                             .Build();

            var instanceType = envConfiguration.GetValue<string>("AuthorizationAPI:InstanceType");
            if (string.IsNullOrWhiteSpace(instanceType))
            {
                return manager;
            }

            var importFileName = $"usermanagement.import.{instanceType.ToLowerInvariant()}.json";
            if (File.Exists(importFileName))
            {
                manager.AddJsonFile(importFileName, optional: false);
            }

            return manager;
        }
    }
}
