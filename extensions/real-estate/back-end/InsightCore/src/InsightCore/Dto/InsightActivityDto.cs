using InsightCore.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace InsightCore.Dto;

public class InsightActivityDto
{
    public StatusLogDto StatusLog { get; set; }
    public InsightOccurrenceDto InsightOccurrence { get; set; }

    public static List<InsightActivityDto> MapFrom(List<InsightActivity> InsightActivities, Func<SourceType?, Guid?, string> getSourceName = null)
    {
        if(InsightActivities is null)
        {
            return null;
        }

        var statusLogList = InsightActivities.Select(x => x.StatusLog).ToList();
        var statusLogDtoList = StatusLogDto.MapFrom(statusLogList, getSourceName);
        var insightActivityDto = InsightActivities.Select(x => new InsightActivityDto
        {
            StatusLog = statusLogDtoList.FirstOrDefault(c => c.Id == x.StatusLog.Id),
            InsightOccurrence = InsightOccurrenceDto.MapFrom(x.InsightOccurrence)
        }).ToList();

        return insightActivityDto; 

    }
}

