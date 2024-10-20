using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Willow.ServiceBus;

/// <summary>
/// Base handler for ServiceBus messages
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseHandler<T> : IMessageHandler
{
	protected readonly ILogger logger;

	protected BaseHandler(ILogger logger)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Carry out any initialization needed by this handler
	/// </summary>
	/// <returns></returns>
	public abstract Task Initialize();

	public bool CanHandle(string messageType) => typeof(T).Name.Equals(messageType);

	/// <summary>
	/// Handle the incoming message of type [T]
	/// </summary>
	public abstract Task<bool> Handle(T value, CancellationToken cancellationToken);

	public async Task<bool> Handle(BinaryData message, CancellationToken cancellationToken)
	{
		try
		{
			var innerMessage = JsonConvert.DeserializeObject(message.ToString(), typeof(T),
			new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			});

			if (innerMessage is T expected)
			{
				return await Handle(expected, cancellationToken);
			}
			else
			{
				logger.LogWarning($"Could not deserialize a message of type {nameof(T)}");
			}
		}
		catch (JsonException)
		{
			logger.LogWarning($"Could not deserialize {message}");
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Handler failed");
		}

		return false;
	}
}
