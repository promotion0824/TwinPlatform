using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace InsightCore.Entities;

[Table("StatusLog")]
public class StatusLogEntity
{
	public Guid Id { get; set; }
	public Guid InsightId { get; set; }
	public Guid? UserId { get; set; }
	public SourceType? SourceType { get; set; }
	public Guid? SourceId { get; set; }
	public InsightStatus Status { get; set; }
	public DateTime CreatedDateTime { get; set; }
	[MaxLength(2048)]
	public string Reason { get; set; }
	public int Priority { get; set; }
	public int OccurrenceCount { get; set; }
	public string ImpactScores { get; set; }

	[ForeignKey(nameof(InsightId))]
	public InsightEntity Insight { get; set; }

	public static StatusLogEntity MapFrom(StatusLog statusLog)
	{
		if (statusLog == null)
		{
			return null;
		}

		return new StatusLogEntity
		{
			 Status = statusLog.Status,
			 CreatedDateTime = statusLog.CreatedDateTime,
			 Reason = statusLog.Reason,
			 Id = statusLog.Id,
			 UserId = statusLog.UserId,
			 SourceType = statusLog.SourceType,
			 SourceId = statusLog.SourceId,
			 InsightId = statusLog.InsightId,
			 OccurrenceCount = statusLog.OccurrenceCount,
			 Priority = statusLog.Priority,
			 ImpactScores = statusLog.ImpactScores!=null &&statusLog.ImpactScores.Any()? JsonConvert.SerializeObject(statusLog.ImpactScores):null
		};
	}
    public static List<StatusLogEntity> MapFrom(IEnumerable<StatusLog> statusLogs)
    {
        return statusLogs?.Select(MapFrom).ToList();
    }

    public static StatusLogEntity MapFrom(Insight insight)
	{
		if (insight == null)
		{
			return null;
		}

		return new StatusLogEntity
		{
			Status = insight.Status,
			CreatedDateTime = DateTime.UtcNow,
			Id = Guid.NewGuid(),
			UserId = insight.CreatedUserId,
			SourceType = insight.SourceType,
			SourceId = insight.SourceId,
			InsightId = insight.Id,
			OccurrenceCount = insight.OccurrenceCount,
			Priority = insight.Priority,
			ImpactScores = insight.ImpactScores!=null && insight.ImpactScores.Any()? JsonConvert.SerializeObject(insight.ImpactScores):null
		};
	}
	public static StatusLog MapTo(StatusLogEntity statusLog)
	{
		if (statusLog == null)
		{
			return null;
		}

		return new StatusLog
		{
			Status = statusLog.Status,
			CreatedDateTime = statusLog.CreatedDateTime,
			Reason = statusLog.Reason,
			Id = statusLog.Id,
			UserId = statusLog.UserId,
			SourceType = statusLog.SourceType,
			SourceId = statusLog.SourceId,
			InsightId = statusLog.InsightId,
			OccurrenceCount = statusLog.OccurrenceCount,
			Priority = statusLog.Priority,
			ImpactScores =string.IsNullOrEmpty(statusLog.ImpactScores)?null: JsonConvert.DeserializeObject<List<ImpactScore>>(statusLog.ImpactScores)
		};
	}

	public static List<StatusLog> MapToList(IEnumerable<StatusLogEntity> statusLog)
	{
		return statusLog?.Select(MapTo).ToList();	
	}	
}
