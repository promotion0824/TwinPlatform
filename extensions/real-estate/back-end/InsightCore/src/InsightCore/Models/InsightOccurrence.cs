using System;
using System.ComponentModel.DataAnnotations;

namespace InsightCore.Models
{
	public class InsightOccurrence
	{
		[StringLength(36)]
		public string OccurrenceId { get; set; }
		public bool IsValid { get; init; }
		public bool IsFaulted { get; init; }
		public DateTime Started { get; set; }
		public DateTime Ended { get; set; }
		public string Text { get; set; }
	}
}
