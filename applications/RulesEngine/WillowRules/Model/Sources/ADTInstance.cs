using System;
using System.Text.Json;
using Azure.Core.Serialization;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace Willow.Rules.Sources;

/// <summary>
/// An ADT Instance 
/// </summary>
/// <remarks>
/// A customer environment may have one or more ADT instances. 
/// There is a many-one relationship between an ADT instance and a time series (ADX) instance.
/// This will normally be one-one but there will be cases where a single large ADX instance supports multiple ADT instances.
/// Rule creation is per ADT instance. The same rules may be applied to each ADT instance for a customer by repetition.
/// Rule instances are stored in a single database per customer environment (ID = ADT ID + Twin ID + Rule ID)
/// Rule execution is per ADX instance.
/// Insight creation is per customer environment, single database.
/// </remarks>
public class ADTInstance
{
	private readonly ILogger logger;

	/// <summary>
	/// Creates a new <see cref="ADTInstance"/>
	/// </summary>
	public ADTInstance(string uri, ILogger logger, DefaultAzureCredential credential)
	{
		Uri = uri ?? throw new ArgumentNullException(nameof(uri));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.lazyADTClient = new Lazy<DigitalTwinsClient>(() => GetADTConnection(uri, logger, credential));
	}

	public string Uri { get; }

	/// <summary>
	/// Gets the Azure DigitalTwinsClient
	/// </summary>
	public DigitalTwinsClient ADTClient => this.lazyADTClient.Value;

	private Lazy<DigitalTwinsClient> lazyADTClient;

	private static DigitalTwinsClient GetADTConnection(string url, ILogger logger, DefaultAzureCredential credential)
	{
		var options = new DigitalTwinsClientOptions()
		{
			Serializer = new JsonObjectSerializer(new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			})
		};
		var client = new DigitalTwinsClient(new Uri(url), credential, options);
		logger.LogInformation($"ADT client created");
		return client;
	}
}
