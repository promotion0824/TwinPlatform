using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Dto
{
    public class InsightDiagnosticDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string SequenceNumber { get; set; }
        public string TwinId { get; set; }
        public string TwinName { get; set; }
        public InsightType Type { get; set; }
        public int Priority { get; set; }
        public InsightStatus LastStatus { get; set; }
        public string PrimaryModelId { get; set; }
        public int OccurrenceCount { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public Guid ParentId { get; set; }
        public bool Check { get; set; }
        public OccurrenceLiveData OccurrenceLiveData { get; set; }
        public static InsightDiagnosticDto MapFrom(Insight insight, Insight parent, List<InsightOccurrence> parentOccurrences, DateTime start, DateTime end,double interval)
        {
            if (insight == null)
                return null;
            return new InsightDiagnosticDto
            {
                Id = insight.Id,
                SiteId = insight.SiteId,
                SequenceNumber = insight.SequenceNumber,
                TwinId = insight.TwinId,
                TwinName = insight.TwinName,
                Type = insight.Type,
                Priority = insight.Priority,
                LastStatus = insight.Status,
                PrimaryModelId = insight.PrimaryModelId,
                OccurrenceCount = insight.OccurrenceCount,
                RuleId = insight.RuleId,
                RuleName = insight.RuleName,
                ParentId = parent.Id,
                Check = IsInsightCheckPass(insight.InsightOccurrences?.ToList(), parentOccurrences),
                OccurrenceLiveData = new OccurrenceLiveData
                {
                    PointEntityId = insight.Id,
                    PointId = insight.Id,
                    PointName = insight.Name,
                    TimeSeriesData = GenerateDiagnosticTimeSeries(insight.InsightOccurrences?.ToList(), start, end, interval)
                }

            };
        }


        public static IEnumerable<InsightDiagnosticDto> DiagnosticInsightDtoMapper(IEnumerable<Insight> insights, Insight parent, List<InsightOccurrence> parentOccurrences, DateTime start, DateTime end, double interval)
        {
            return insights?.Select(c => MapFrom(c, parent, parentOccurrences, start, end, interval));

        }

        private static bool IsInsightCheckPass(List<InsightOccurrence> insightOccurrences, IReadOnlyCollection<InsightOccurrence> parentOccurrences)
        {
            if (!parentOccurrences.Any(c => c.IsFaulted))
                return true;

            var parentFaultOccurrences = parentOccurrences.Where(c => c.IsFaulted).ToList();
            foreach (var occurrence in parentFaultOccurrences)
            {
                if (insightOccurrences.Any(c => c.IsFaulted && c.Started < occurrence.Ended && c.Ended > occurrence.Started))
                    return false;
            }
            return true;
        }

        private static List<TimeSeriesBinaryData> GenerateDiagnosticTimeSeries(
            List<InsightOccurrence> insightOccurrences, DateTime start, DateTime end, double interval)
        {

            var result = new List<TimeSeriesBinaryData>();
            // Filter out irrelevant occurrences
            var filteredOccurrences = GetFilteredOccurrencesByDateRange(insightOccurrences, start, end);
            foreach (var occurrence in filteredOccurrences)
            {
                var occurrenceStart = occurrence.Started;

                // Calculate the number of breakdowns within the occurrence
                var numOfBreakdown =(int)Math.Ceiling((occurrence.Ended - occurrenceStart).TotalMinutes / interval);

                for (var i = 0; i < numOfBreakdown; i++)
                {
                    var endDate = occurrenceStart.AddMinutes((i + 1) * interval);
                    result.Add(new TimeSeriesBinaryData
                    {
                        Start = occurrenceStart.AddMinutes(i * interval),
                        End =endDate> occurrence.Ended?occurrence.Ended:endDate,
                        IsFaulty = occurrence.IsFaulted
                    });
                }
            }

            return result;
        }

        private static List<InsightOccurrence> GetFilteredOccurrencesByDateRange(List<InsightOccurrence> insightOccurrences, DateTime start, DateTime end)
        {
            // Filter occurrences that overlap with the specified date range
            var filteredOccurrences = insightOccurrences?
                .Where(c => c.Started < end && c.Ended > start).OrderBy(c=>c.Started)
                .ToList();

            if (filteredOccurrences == null || filteredOccurrences.Count == 0)
            {
                // If input list is null or empty, return a single occurrence covering the entire range
                return
                [
                    new InsightOccurrence
                    {
                        Started = start,
                        Ended = end,
                        IsFaulted = false
                    }
                ];
            }

            // Determine the earliest start date and latest end date among the filtered occurrences
            var firstStartDate = filteredOccurrences.First().Started;
            var lastEndDate = filteredOccurrences.Last().Ended;
            // Add a new occurrence to cover the gap after the latest occurrence if needed
            if (lastEndDate < end)
            {
                filteredOccurrences.Add(new InsightOccurrence
                {
                    Started = lastEndDate,
                    Ended = end,
                    IsFaulted = false
                });
            }
            else
            {
                filteredOccurrences.Last().Ended = end;
            }
            // Add a new occurrence to cover the gap before the earliest occurrence if needed
            if (firstStartDate > start)
            {
                filteredOccurrences.Add(new InsightOccurrence
                {
                    Started = start,
                    Ended = firstStartDate,
                    IsFaulted = false
                });
            }
            else
            {
                filteredOccurrences.First().Started = start;
            }

            

            // Sort the filtered occurrences by start date
            filteredOccurrences.Sort((a, b) => a.Started.CompareTo(b.Started));

            return filteredOccurrences;
        }

    }
    public class OccurrenceLiveData
    {
        public Guid PointId { get; set; }
        public Guid PointEntityId { get; set; }
        public string PointName { get; set; }
        public string PointType => "Binary";
        public string Unit => "Bool";
        public List<TimeSeriesBinaryData> TimeSeriesData { get; set; }
    }
    public class TimeSeriesBinaryData
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsFaulty { get; set; }
    }
}
