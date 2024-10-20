using System;
using System.Collections.Generic;
using System.Threading;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Processor.Services;
using Willow.Rules.Configuration;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Extensions for registering ADX Services with dependency injection
/// </summary>
public static class ADXServiceExtensions
{
	public static IAsyncEnumerable<RawData> RunRawQuery(this IADXService adxService, DateTime earliest, DateTime latest, IdFilter? idFilter = null, CancellationToken cancellationToken = default)
	{
		var (_, queryResult) = adxService.RunRawQueryPaged(earliest, latest, idFilter is not null ? new IdFilter[] { idFilter } : Array.Empty<IdFilter>(), cancellationToken: cancellationToken);
		return queryResult.ReadAllAsync(cancellationToken);
	}

	public static IAsyncEnumerable<RawData> RunRawQuery(this IADXService adxService, DateTime earliest, DateTime latest, IEnumerable<IdFilter> idFilters, CancellationToken cancellationToken = default)
	{
		var (_, queryResult) = adxService.RunRawQueryPaged(earliest, latest, idFilters, cancellationToken: cancellationToken);
		return queryResult.ReadAllAsync(cancellationToken);
	}

	/// <summary>
	/// Adds AdxService to the services collection
	/// </summary>
	/// <remarks>
	/// Can inject a special debug service that pulls from a file instead
	/// </remarks>
	public static IServiceCollection AddAdxService(this IServiceCollection services)
	{
		// ADX resolves to a file-based service if that's configured in options (used for debugging)
		// TODO: Move this to an extension method and use in both locations
		return services.AddSingleton<IADXService, IADXService>((s) =>
			{
				var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
				var adx = customerOptions.Value.ADX;
				var credential = s.GetRequiredService<DefaultAzureCredential>();
				var health = s.GetRequiredService<HealthCheckADX>();
				var loggerFactory = s.GetRequiredService<ILoggerFactory>();
#if DEBUG
				if (!string.IsNullOrEmpty(adx.FilePath))
				{
					return new FileBasedADXService(adx.FilePath!, loggerFactory.CreateLogger<FileBasedADXService>());
				}
#endif
				return new ADXService(customerOptions, credential, health, loggerFactory.CreateLogger<ADXService>());
			});
	}
}
