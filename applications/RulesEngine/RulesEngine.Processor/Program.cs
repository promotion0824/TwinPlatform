using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Services;
using Willow.ServiceBus;

namespace RulesEngine.Processor;

/// <summary>
/// The rules engine processor
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

				// app insights is registered during open telemetry setup using services.AddWillowContext(Configuration) in startup
				string connection = hostingContext.Configuration["Customer:SQL:CONNECTIONSTRING"];

				//details about the sql package can be found @ https://github.com/serilog-mssql/serilog-sinks-mssqlserver
				//the default implementation use SqlBulkCopy that writes a batch asynchronously
				builder.AddSerilogSqlSink(hostingContext.Configuration, connection, "Logs");
			})

			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});

}
