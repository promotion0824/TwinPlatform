using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Willow.Common.Configuration;

namespace DigitalTwinCore
{
    public static class Program
    {
        public static void Main()
        {
            CreateWebHostBuilder(null).Build().Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
	            .ConfigureWebHostDefaults(webBuilder =>
	            {
                    webBuilder
                        .ConfigureAppConfiguration((context, config) =>
                        {
	                        // By default users secrets are added if the environment name is "Development", whereas
	                        // they should also be added for other development environments such as "Development_UAT".
	                        if (IsDevelopment(context.HostingEnvironment.EnvironmentName))
	                        {
		                        config.AddUserSecrets(Assembly.GetExecutingAssembly(), true, true);
	                        }
	                        config.ConfigureWillowAppConfiguration();
                        })
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                });

        /// <summary>
        /// Development and other environments such as Development_UAT etc are all considered development environments.
        /// </summary>
        private static bool IsDevelopment(string environmentName) =>
	        environmentName.Contains("Development", StringComparison.OrdinalIgnoreCase);
    }
}
