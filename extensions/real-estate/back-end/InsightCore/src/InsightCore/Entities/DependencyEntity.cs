using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace InsightCore.Entities;

[Table("Dependencies")]
public class DependencyEntity
{

	public Guid Id { get; set; }

	[Required]
	public Guid FromInsightId { get; set; }

	[ForeignKey(nameof(FromInsightId))]
	public InsightEntity FromInsight { get; set; }

	[StringLength(500)]
	[Required(AllowEmptyStrings = false)]
	public string Relationship { get; set; }

	[Required]
	public Guid ToInsightId { get; set; }

    [ForeignKey(nameof(ToInsightId))]
    public InsightEntity ToInsight { get; set; }

    public static List<DependencyEntity> MapFrom(Insight insight)
	{
		if (insight?.Dependencies is null)
		{
			return null;
		}
		return insight.Dependencies.Select(x=> new DependencyEntity
		{
			FromInsightId = insight.Id,
			Relationship = x.Relationship,
			ToInsightId = x.InsightId
		}).ToList();

	}

	public static List<Dependency> MapTo(IEnumerable<DependencyEntity> entities)
	{
		return entities?.Select(MapTo).ToList();
	}

	public static Dependency MapTo(DependencyEntity entity)
	{
		if (entity is null)
		{
			return null;
		}
		return new Dependency
		{
			Relationship = entity.Relationship,
			InsightId = entity.ToInsightId
		};
	}

}

