using System;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO
{
	/// <summary>
	/// Dto for an <see cref="InsightOccurrence" />
	/// </summary>
	public class InsightOccurrenceDto
	{
		/// <summary>
		/// Creates a new <see cref="InsightOccurrenceDto" /> from a <see cref="InsightOccurrence" />
		/// </summary>
		/// <param name="occurrence"></param>
		public InsightOccurrenceDto(InsightOccurrence occurrence)
		{
			this.Id = occurrence.Id.ToString();
			this.IsFaulted = occurrence.IsFaulted;
			this.IsValid = occurrence.IsValid;
			this.Started = occurrence.Started;
			this.Text = occurrence.Text;
			this.Ended = occurrence.Ended;
		}

		/// <summary>
		/// Id of the Insight Occurrence
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// Whether the insight occurrence is faulted
		/// </summary>
		public bool IsFaulted { get; }

		/// <summary>
		/// Whether the range is valid (has enough data in it)
		/// </summary>
		public bool IsValid { get; }

		/// <summary>
		/// Start of interval
		/// </summary>
		public DateTimeOffset Started { get; }

		/// <summary>
		/// Text description of the insight over the interval
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// End of interval
		/// </summary>
		public DateTimeOffset Ended { get; }
	}
}

