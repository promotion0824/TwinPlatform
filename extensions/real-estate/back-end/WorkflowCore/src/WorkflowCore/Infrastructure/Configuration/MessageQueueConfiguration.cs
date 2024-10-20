using System;

namespace WorkflowCore.Infrastructure.Configuration;

public class MessageQueueConfiguration
{
	/// <summary>
	/// The notification queue
	/// </summary>
	public string CommServiceQueue { get; set; }
	/// <summary>
	/// The insight status update queue
	/// </summary>
	public string TicketInsightStatusQueue { get; set; }
}
