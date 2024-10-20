using Microsoft.Extensions.Logging;
using System.Diagnostics.Tracing;

namespace Willow.Rules.Repository;

/// <summary>
/// Listens for sql event source
/// </summary>
public class SqlClientListener : EventListener
{
	private readonly ILogger logger;

	public SqlClientListener(ILogger logger)
	{
		this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
	}

	protected override void OnEventSourceCreated(EventSource eventSource)
	{
		if (eventSource.Name.Equals("Microsoft.Data.SqlClient.EventSource"))
		{
			EnableEvents(eventSource, EventLevel.Informational, EventKeywords.All);
		}
	}

	protected override void OnEventWritten(EventWrittenEventArgs eventData)
	{
		if (eventData.EventSource.Name.Contains("SqlClient") && eventData.Payload is not null)
		{
			if (eventData.Payload.Count > 0)
			{
				var message = eventData.Payload[0]?.ToString() ?? "";

				if (message.Contains("ERR") == true)
				{
					logger.LogError("Sql error from {eventName}: {message}", eventData.EventName, message);
				}
				else if (eventData.EventName == "SNITrace")
				{
					bool isChatty = message.Contains("Data received from stream asynchronously") ||
						message.Contains("Data read from stream synchronously") ||
						message.Contains("Data sent to stream asynchronously") ||
						message.Contains("Data sent to stream synchronously");

					if (!isChatty)
					{
						logger.LogInformation("Sql event from {eventName}: {message}", eventData.EventName, message);
					}

				}
			}
		}
	}
}
