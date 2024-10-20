
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.Api.Authentication;

namespace Willow.AzureDigitalTwins.Api.APITests
{
	public class CustomWebApplicationFactory<TStartup> :
		WebApplicationFactory<TStartup> where TStartup : class
	{

		public CustomWebApplicationFactory()
		{

		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			IConfigurationRoot? integrationConfig =null;
			builder.ConfigureAppConfiguration(config =>
			{
				integrationConfig = new ConfigurationBuilder()
					.AddJsonFile("appsettings.Test.json")
					.Build();

				config.AddConfiguration(integrationConfig);
			});
			builder
			.UseEnvironment("Test")
			.UseTestServer();

			builder.ConfigureLogging(logging => {
				logging.ClearProviders();
				logging.AddDebug();
				logging.AddConsole();
			});
			builder.ConfigureServices(services =>
			{
				services.AddMemoryCache();
				services.AddLogging();
				services.AddClientCredentialToken(integrationConfig.GetSection("AzureAdB2C"));
			});

		}
	}
}
