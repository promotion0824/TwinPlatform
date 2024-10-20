// POCO class, serialized to DB
#nullable disable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Willow.Rules.Repository;

namespace Willow.Rules.Model
{
	/// <summary>
	/// Impact of the fault (if any)
	/// </summary>
	public class ImpactScore : IId
	{
		/// <summary>
		/// Constructor for impact score
		/// </summary>
		public ImpactScore(Insight insight, string name, string fieldId, string externalId, double value, string unit)
		{
			if (insight is null)
			{
				throw new ArgumentNullException(nameof(insight));
			}

			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
			}

			Id = $"{insight.Id}_{fieldId}";
			InsightId = insight.Id;
			Name = name;
			FieldId = fieldId;
            ExternalId = externalId;
			Score = value;
			//last update must align to insight's lastupdate date so "old" impact scores can be deleted
			LastUpdated = insight.LastUpdated;
			if (!string.IsNullOrEmpty(unit))
			{
				BaseScore = Expressions.Unit.ConvertToBaseValue(Expressions.Unit.Get(unit), value);
			}
			else
			{
				BaseScore = value;
			}

			if (double.IsNaN(BaseScore)) BaseScore = 0.0;  // EF has issues with NaN values
			if (double.IsNaN(Score)) Score = 0.0;  // EF has issues with NaN values

			Unit = unit;
		}

		/// <summary>
		/// EF Constructor
		/// </summary>
		[JsonConstructor]
		private ImpactScore()
		{
		}

		/// <summary>
		/// The Id of the impact score (Insight.Id + fieldid)
		/// </summary>
		public string Id { get; init; }

		/// <summary>
		/// The insight id which it belongs to
		/// </summary>
		public string InsightId { get; init; }

		/// <summary>
		/// The unit of measure for the score
		/// </summary>
		public string Unit { get; init; }

		/// <summary>
		/// The name of the impact score
		/// </summary>
		public string Name { get; init; }

		/// <summary>
		/// The field id of the impact score
		/// </summary>
		public string FieldId { get; init; }

        /// <summary>
        /// The ADX external id of the impact score
        /// </summary>
        public string ExternalId { get; init; }

        /// <summary>
        /// The value of the impact score
        /// </summary>
        public double Score { get; init; }

		/// <summary>
		/// The score converted to it base value (eg killowatt to watt)
		/// </summary>
		public double BaseScore { get; init; }

		/// <summary>
		/// The date which the impact score was last updated
		/// </summary>
		public DateTimeOffset LastUpdated { get; init; }

		/// <summary>
		/// The parent Insight. Used by EF for one-to-many to Insight
		/// </summary>
		[JsonIgnore]
		public Insight Insight { get; init; }
	}
}

