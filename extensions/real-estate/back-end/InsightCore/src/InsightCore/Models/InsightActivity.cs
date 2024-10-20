using InsightCore.Dto;
using InsightCore.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Models;

public class InsightActivity
{
    public StatusLog StatusLog { get; set; }
    public InsightOccurrence InsightOccurrence { get; set; }

    public static List<InsightActivity> MapFrom(InsightActivityData data)
    {
        if(data is null)
        {
            return null;
        }
        var insightActivity = data.StatusLog.Select(x => new InsightActivity {StatusLog = StatusLogEntity.MapTo(x) }).ToList();

        return insightActivity;
    }
}

public record InsightActivityData (IEnumerable<InsightOccurrenceEntity> Occurrences, IEnumerable<StatusLogEntity> StatusLog);

