using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Authorization.TwinPlatform.Permission.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Diagnostics.Metrics;
using System.Reflection;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace Authorization.TwinPlatform.Permission.Api.Extensions;

/// <summary>
/// Static extension class for registering app services
/// </summary>
public static class ServiceCollectionExtension
{
	/// <summary>
	/// Extension method for configuring AzureAd Authentication
	/// </summary>
	/// <param name="services">ServiceCollection</param>
	/// <param name="configuration">ConfigurationInstance</param>
	public static void AddAdAuthentication(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApi(
			configuration
			//configSectionName: "AzureAd" - AzureAd Configuration section will get picked up by default
			);
		services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
		{
			var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;

			options.Events.OnTokenValidated = async context =>
			{
				await existingOnTokenValidatedHandler(context);
				// Your code to add extra configuration that will be executed after the current event implementation.
				// .i.e Token Issuer or Audience Validators will be added here later as needed
			};

		});
	}

    /// <summary>
    /// Adds Observability telemetry initializers to the collection.
    /// </summary>
    /// <param name="services">IServiceCollection Instance.</param>
    /// <param name="configuration">IConfiguration Instance.</param>
    /// <returns>Instance of <see cref="Meter"/></returns>
    public static Meter AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWillowContext(configuration);
        var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName();

        services.AddSingleton(new MetricsAttributesHelper(configuration));

        var meter = new Meter(entryAssemblyName?.Name ?? "Unknown",
            entryAssemblyName?.Version?.ToString() ?? "Unknown");
        services.AddSingleton(meter);

        return meter;
    }

    /// <summary>
    /// Extension method for registering all manager services used by the controller
    /// </summary>
    /// <param name="services">ServiceCollection</param>
    public static void AddManagerServices(this IServiceCollection services)
	{
		services.AddScoped<IPermissionAggregationManager, PermissionAggregationManager>();
		services.AddScoped<IImportManager, ImportManager>();
        services.AddScoped<IGroupManager, GroupManager>();
        services.AddScoped<IUserManager, UserManager>();
	}

}

