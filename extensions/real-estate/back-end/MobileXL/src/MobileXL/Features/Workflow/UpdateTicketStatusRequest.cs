using MobileXL.Models;

namespace MobileXL.Features.Workflow
{
	public class UpdateTicketStatusRequest
	{
		public class Open2ReassignForm
		{
			public string RejectComment { get; set; }
		}

		public class RepairForm
		{
			public string Cause { get; set; }
			public string Solution { get; set; }
			public string Notes { get; set; }
		}

		public int StatusCode { get; set; }

		public Open2ReassignForm Open2Reassign { get; set; }
		public RepairForm InProgress2LimitedAvailability { get; set; }
		public RepairForm InProgress2Resolved { get; set; }
		public RepairForm LimitedAvailability2Resolved { get; set; }
	}

}
