using System;
using System.Collections.Generic;

namespace WorkflowCore.Models;
public class TicketActivity
{
	public Guid TicketId { get; set; }
    public string TicketSummary { get; set; }
	public TicketActivityType ActivityType { get; set; }
	public DateTime ActivityDate { get; set; }
	public Guid SourceId { get; set; }
	public SourceType SourceType { get; set; }
	public List<KeyValuePair<string, string>> Activities { get; set; }
}

public enum TicketActivityType
{
	NewTicket = 1,
	TicketModified = 2,
	TicketComment = 3,
	TicketAttachment = 4,
}
