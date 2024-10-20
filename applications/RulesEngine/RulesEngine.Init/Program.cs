using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Willow.Rules.Configuration;
using Willow.ServiceBus;
using Willow.Rules.Services;
using System.Linq;
using Willow.Rules.Model;
using Azure.Identity;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Willow.Rules.Sources;

namespace RulesEngine.Init;

/// <summary>
/// The rules engine init container updates SQL databases
/// </summary>
public class Program
{
	/// <summary>
	/// Main method
	/// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public static async Task<int> Main(string[] args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(DomainUnhandledExceptionHandler);

		var hostBuilder = CreateHostBuilder(args);
		var host = hostBuilder.Build();

		// too slow ... await CheckPermissions(host);
		host.Run();
		return Environment.ExitCode;
	}

	static void DomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
	{
		Exception e = (Exception)args.ExceptionObject;
		// stdout for container
		Console.WriteLine("UE Handler caught : " + e.Message);
		Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
		Debug.WriteLine("Runtime terminating");
	}

	private static RateLimiter rateLimiter = new RateLimiter(10, 1);

	private static async Task CheckPermissions(IHost host)
	{
		using (var scope = host.Services.CreateScope())
		{
			var services = scope.ServiceProvider;
			var logger = services.GetRequiredService<ILogger<Program>>();

			try
			{
				logger.LogInformation("Checking configuration");

				var singleConfig = services.GetRequiredService<IOptions<CustomerOptions>>();
				if (singleConfig.Value is null) logger.LogError("Did not find configuration or single customer");
				logger.LogInformation("{json}", JsonConvert.SerializeObject(singleConfig.Value));

				logger.LogInformation("Checking ServiceBus connection");

				var messageSender = services.GetRequiredService<IMessageSenderBackEnd>();
				await messageSender.SendHeartBeat();

				logger.LogInformation("ServiceBus connection OK");

				// TODO: Check ADT Connection

				logger.LogInformation("Checking ADX Connection");
				var adxclient = services.GetRequiredService<IADXService>();

				if (adxclient is ADXService adx)
				{
					try
					{
						var lastTime = await adx.RunQuery();
						logger.LogInformation("ADX Connection OK {time}", lastTime.ToString());

						if (lastTime < DateTime.UtcNow.AddHours(-1))
						{
							logger.LogWarning("But data seems to be stale {hours} hours old", (DateTime.UtcNow - lastTime).TotalHours);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Could not connect to ADX");
					}
				}

				// Do we need to do anything to verify the service bus?
			}
			catch (CredentialUnavailableException ex)
			{
				rateLimiter.Limit("CredentialUnavailable", () =>
				{
					logger.LogError(ex, "Credential unavailable");
				}, logger);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred checking permissions");
			}
		}
	}

	/// <summary>
	/// Creates the background service and the web api
	/// </summary>
	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((hostContext, config) =>
			{
				var env = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT")
					?? "Production";

				config.AddJsonFile("appsettings.json", optional: false)
					.AddJsonFile($"appsettings.{env}.json", optional: true)
					.AddJsonFile($"appsettings.user.json", optional: true)
					//.AddJsonFile("customer-environments.json", optional: false)
					.AddEnvironmentVariables("RULES_");
			})

			.ConfigureLogging((hostingContext, builder) =>
			{
				// builder.ClearProviders();
				builder.AddSimpleConsole(opts =>
				{
					opts.IncludeScopes = false;
					opts.SingleLine = true;
					opts.TimestampFormat = "HH:mm:ss ";
				});
				builder.AddApplicationInsights();
			})

			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});

}