using System;
using System.Collections.Generic;

namespace InsightCore.Dto;

public class InsightOccurrencesCountByDateResponse
{
    public List<InsightOccurrencesCountDto> Counts { get; set; }
    public int AverageDuration { get; set; }
}

public class InsightOccurrencesCountDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
