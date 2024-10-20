using System;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Request;

public class CreateTicketAttachmentRequest
{
	public Guid? SourceId { get; set; }
	public SourceType? SourceType { get; set; }
}

