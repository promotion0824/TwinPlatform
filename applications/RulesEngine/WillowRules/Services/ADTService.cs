using System.Linq;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Rules.Configuration;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Service for working with ADT
/// </summary>
public interface IADTService
{
	/// <summary>
	/// Gets the ADT Instances
	/// </summary>
	ADTInstance[] AdtInstances { get; }
}

/// Service for working with ADT
/// </summary>
public class ADTService : IADTService
{
	public ADTInstance[] AdtInstances { get; }

	public ADTService(
		IOptions<CustomerOptions> customerOptions,
		DefaultAzureCredential credential,
		ILoggerFactory loggerFactory
		)
	{
		var adtOptions = customerOptions.Value.ADT;

		this.AdtInstances = adtOptions
			.Select(a => new ADTInstance(a.Uri, loggerFactory.CreateLogger<ADTInstance>(), credential))
			.ToArray();
	}
}

