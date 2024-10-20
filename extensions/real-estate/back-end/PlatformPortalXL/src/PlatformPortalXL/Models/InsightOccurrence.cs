using System;

namespace PlatformPortalXL.Models
{
	public class InsightOccurrence
	{
		public string Id { get; set; }
		public bool IsValid { get; init; }
		public bool IsFaulted { get; init; }
		public DateTime Started { get; set; }
		public DateTime Ended { get; set; }
		public string Text { get; set; }
	}
}
