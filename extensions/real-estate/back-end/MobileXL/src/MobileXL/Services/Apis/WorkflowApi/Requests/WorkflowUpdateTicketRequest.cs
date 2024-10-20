using System;
using System.Collections.Generic;
using MobileXL.Models;

namespace MobileXL.Services.Apis.WorkflowApi.Requests
{
	public class WorkflowUpdateTicketRequest
	{
		public Guid CustomerId { get; set; }
		public int? Status { get; set; }
		public string Cause { get; set; }
		public string Solution { get; set; }
		public TicketAssigneeType? AssigneeType { get; set; }
		public Guid? AssigneeId { get; set; }
		public Guid? CategoryId { get; set; }
		public string Notes { get; set; }
		public List<TicketTask> Tasks { get; set; }
		public DateTime? ExternalCreatedDate { get; set; }
		public DateTime? ExternalUpdatedDate { get; set; }
		public bool LastUpdatedByExternalSource { get; set; }
		public Guid SourceId { get; set; }
		public TicketSourceType? SourceType { get; } = TicketSourceType.Platform;
	}
}
