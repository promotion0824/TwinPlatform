using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Dto;

public class InsightOccurrenceDto
{
	public string Id { get; set; }
	public bool IsValid { get; init; }
	public bool IsFaulted { get; init; }
	public DateTime Started { get; set; }
	public DateTime Ended { get; set; }
	public string Text { get; set; }

	public static InsightOccurrenceDto MapFrom(InsightOccurrence insightOccurrence)
	{
		if (insightOccurrence == null)
			return null;
		return new InsightOccurrenceDto
		{
			 Id = insightOccurrence.OccurrenceId,
			 Ended = insightOccurrence.Ended,
			 IsFaulted = insightOccurrence.IsFaulted,
			 IsValid = insightOccurrence.IsValid,
			 Started = insightOccurrence.Started,
			 Text = insightOccurrence.Text = insightOccurrence.Text
		};
	}

	public static List<InsightOccurrenceDto> MapFrom(IEnumerable<InsightOccurrence> insights)
	{
		return insights?.Select(MapFrom).ToList();
	}
}
