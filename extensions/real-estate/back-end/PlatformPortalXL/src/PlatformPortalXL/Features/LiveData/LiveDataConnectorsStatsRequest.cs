using System;

namespace PlatformPortalXL.Features.LiveData
{
	public class LiveDataConnectorStatsRequest
	{
		public Guid[] ConnectorIds { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
	}
}
