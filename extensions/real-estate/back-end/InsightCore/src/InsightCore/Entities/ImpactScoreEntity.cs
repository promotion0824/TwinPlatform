using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace InsightCore.Entities
{
    [Table("ImpactScores")]
    public class ImpactScoreEntity
    {
        public Guid Id { get; set; }
        public Guid InsightId { get; set; }

        [ForeignKey(nameof(InsightId))]
        public InsightEntity Insight { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string Name { get; set; }
		/// <summary>
		/// The field id of the impact score
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		[MaxLength(256)]
		public string FieldId { get; set; }

		public double Value { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Unit { get; set; }

        public string ExternalId { get; set; }
        public string RuleId { get; set; }

        public static ImpactScore MapTo(ImpactScoreEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new ImpactScore
            {
                Name = entity.Name,
                Value = entity.Value, 
                Unit = entity.Unit,
				FieldId = entity.FieldId,
                ExternalId = entity.ExternalId,
                RuleId = entity.RuleId
            };
        }

        public static List<ImpactScore> MapTo(IEnumerable<ImpactScoreEntity> entities)
        {
            return entities?.Select(MapTo).ToList();
        }

        public static List<ImpactScoreEntity> MapFrom(Insight insight)
        {
            if (insight.ImpactScores == null)
            {
                return null;
            }

            return insight.ImpactScores.Select(impactScore => new ImpactScoreEntity
            {
                Id = Guid.NewGuid(),
                InsightId = insight.Id,
                Name = impactScore.Name,
				FieldId = impactScore.FieldId,
                Value = impactScore.Value,
                Unit = impactScore.Unit,
                ExternalId = impactScore.ExternalId,
                RuleId = insight.RuleId
            }).ToList();
        }
    }
}
