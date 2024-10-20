using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Dto;

public class StatusLogDto:BaseStatusLogEntryDto
{
	public Guid? UserId { get; set; }
	public int Priority { get; set; }
	public int OccurrenceCount { get; set; }
	public List<ImpactScore> ImpactScores { get; set; }
	public bool PreviouslyIgnored { get; set; }
	public bool PreviouslyResolved { get; set; }
	
	public static List<StatusLogDto> MapFrom(List<StatusLog> statusLogs, Func<SourceType?, Guid?, string> getSourceName = null)
	{
		if(statusLogs==null || !statusLogs.Any())
			return null;
		
		var result = new List<StatusLogDto>();
		var orderedLogs = statusLogs.OrderByDescending(c => c.CreatedDateTime).ToList();
		foreach (var statusLog in  orderedLogs)
		{
			var statusLogDto = new StatusLogDto
			{
				UserId = statusLog.UserId,
				Status = statusLog.Status,
				CreatedDateTime = statusLog.CreatedDateTime,
				Reason = statusLog.Reason,
				Id = statusLog.Id,
				SourceType = statusLog.SourceType,
				SourceId = statusLog.SourceId,
                SourceName = getSourceName != null ? getSourceName(statusLog.SourceType, statusLog.SourceId) : (string)null,
                InsightId = statusLog.InsightId,
                ImpactScores = statusLog.ImpactScores,
				OccurrenceCount = statusLog.OccurrenceCount,
				Priority = statusLog.Priority
			};
			var statusLogIndex = orderedLogs.IndexOf(statusLog);
			if (orderedLogs.Count - (statusLogIndex + 1) > 0)
			{
				var subList = orderedLogs.Skip(statusLogIndex + 1)
					.Take(orderedLogs.Count - (statusLogIndex + 1)).ToList();
				statusLogDto.PreviouslyIgnored = subList.Any(c => c.Status == InsightStatus.Ignored);

				statusLogDto.PreviouslyResolved = subList.Any(c => c.Status == InsightStatus.Resolved);
			}

			result.Add(statusLogDto);
		}

		 
		return result;
	}
}
