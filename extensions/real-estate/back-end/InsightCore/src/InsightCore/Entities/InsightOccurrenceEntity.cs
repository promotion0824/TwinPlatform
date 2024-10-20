using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace InsightCore.Entities
{
	[Table("InsightOccurrences")]
	public class InsightOccurrenceEntity
	{
		public Guid Id { get; set; }
		public Guid InsightId { get; set; }

		[ForeignKey(nameof(InsightId))]
		public InsightEntity Insight { get; set; }

		[MaxLength(36)]
		public string OccurrenceId { get; set; }
		public bool IsValid { get; init; }
		public bool IsFaulted { get; init; }
		public DateTime Started { get; set; }
		public DateTime Ended { get; set; }
		public string Text { get; set; }

		public static InsightOccurrence MapTo(InsightOccurrenceEntity entity)
		{
			if (entity == null)
			{
				return null;
			}

			return new InsightOccurrence
			{
				OccurrenceId = entity.OccurrenceId,
				IsFaulted = entity.IsFaulted,
				IsValid = entity.IsValid,
				Started = entity.Started,
				Ended = entity.Ended,
				Text = entity.Text
			};
		}

		public static List<InsightOccurrence> MapTo(IEnumerable<InsightOccurrenceEntity> entities)
		{
			return entities?.Select(MapTo).ToList();
		}

		public static List<InsightOccurrenceEntity> MapFrom(Insight insight)
		{
			if (insight.InsightOccurrences == null)
			{
				return null;
			}

			return insight.InsightOccurrences.Select(insightOccurrence => new InsightOccurrenceEntity
			{
				InsightId = insight.Id,
				OccurrenceId = insightOccurrence.OccurrenceId,
				IsFaulted = insightOccurrence.IsFaulted,
				IsValid = insightOccurrence.IsValid,
				Started = insightOccurrence.Started,
				Ended = insightOccurrence.Ended,
				Text = insightOccurrence.Text
			}).ToList();
		}

        public static InsightOccurrenceEntity MapFrom(Guid insightId, InsightOccurrence insightOccurrence)
        {
            if (insightOccurrence == null)
            {
                return null;
            }

            return new InsightOccurrenceEntity
            {
                InsightId = insightId,
                OccurrenceId = insightOccurrence.OccurrenceId,
                IsFaulted = insightOccurrence.IsFaulted,
                IsValid = insightOccurrence.IsValid,
                Started = insightOccurrence.Started,
                Ended = insightOccurrence.Ended,
                Text = insightOccurrence.Text
            };
        }

        public static List<InsightOccurrenceEntity> MapFrom(Guid insightId, List<InsightOccurrence> insightOccurrences)
        {
            return insightOccurrences?.Select(x => MapFrom(insightId, x)).ToList();
        }
    }
}

