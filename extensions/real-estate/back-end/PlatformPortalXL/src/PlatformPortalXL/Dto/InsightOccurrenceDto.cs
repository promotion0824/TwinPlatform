using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Dto
{
    public class InsightOccurrenceDto
	{
		public string Id { get; set; }
		public Guid InsightId { get; set; }
		public bool IsValid { get; init; }
		public bool IsFaulted { get; init; }
		public DateTime Started { get; set; }
		public DateTime Ended { get; set; }
		public string Text { get; set; }

		public static InsightOccurrenceDto MapFromModel(InsightOccurrence insightOccurrence,Guid insightId)
		{
			if (insightOccurrence == null)
				return null;
            return new InsightOccurrenceDto
			{
                Id =  insightOccurrence.Id,
				InsightId = insightId,
				Ended = insightOccurrence.Ended,
				Text = insightOccurrence.Text,
				IsValid = insightOccurrence.IsValid,
				IsFaulted = insightOccurrence.IsFaulted,
				Started = insightOccurrence.Started
			};
        }

        public static List<InsightOccurrenceDto> MapFromModels(IEnumerable<InsightOccurrence> insights,Guid insightId )
        {
            return insights?.Select(x=>MapFromModel(x,insightId)).ToList();
        }

    }
}
