using Newtonsoft.Json;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Users;
using Willow.Workflow;
using Willow.Workflow.Models;

namespace PlatformPortalXL.Dto;

public class InsightActivityDto
{
	/// <summary>
	/// Insight Ticket Id
	/// </summary>
	public Guid? TicketId { get; set; }

    /// <summary>
    /// Ticket Summary
    /// </summary>
    public string TicketSummary { get; set; }

    /// <summary>
    /// Insight Activity Type
    /// </summary>
    public string ActivityType { get; set; }

	/// <summary>
	/// Insight Activity Date
	/// </summary>
	public DateTime ActivityDate { get; set; }

	/// <summary>
	/// App Id that generated the activity
	/// </summary>
	public Guid? SourceId { get; set; }

	/// <summary>
	/// User Id that generated the activity
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// User full name that generated the activity
	/// </summary>
	public string FullName { get; set; }
	/// <summary>
	/// App Name that generated the activity
	/// </summary>
	public string AppName { get; set; }

	/// <summary>
	/// Insight Source Type
	/// </summary>
	public InsightSourceType SourceType { get; set; }

	/// <summary>
	/// Insight activity details List
	/// </summary>
	public List<KeyValuePair<string, string>> Activities { get; set; }

	public static InsightActivityDto MapFromInsightTicketActivity(InsightTicketActivity insightTicketActivity)
	{
		if (insightTicketActivity is null)
		{
			return null;
		}

		return new InsightActivityDto()
		{
			TicketId = insightTicketActivity.TicketId,
            TicketSummary = insightTicketActivity.TicketSummary,
			ActivityType = insightTicketActivity.ActivityType.ToString(),
			ActivityDate = insightTicketActivity.ActivityDate,
			SourceId = insightTicketActivity.SourceType == TicketSourceType.App ? insightTicketActivity.SourceId : null,
			UserId = insightTicketActivity.SourceType != TicketSourceType.App ? insightTicketActivity.SourceId : null,
			SourceType = insightTicketActivity.SourceType == TicketSourceType.App ? InsightSourceType.App : InsightSourceType.Willow,
			Activities = insightTicketActivity.Activities,
		};
	}
	public static List<InsightActivityDto> MapFromInsightTicketActivities(List<InsightTicketActivity> insightTicketActivities)
	{
		if (insightTicketActivities is { Count: > 0 })
		{
			return insightTicketActivities
				.Select(MapFromInsightTicketActivity)
				.ToList();
		}
		return new();
	}

	public static InsightActivityDto MapFromInsightActivities(InsightActivity insightActivity)
	{
		if (insightActivity is null || insightActivity.StatusLog is null)
		{
			return null;
		}
        var insightStatusLogEntry = insightActivity.StatusLog;
        var OccurrenceStarted = insightActivity.InsightOccurrence?.Started.ToString("o");
        var OccurrenceEnded = insightActivity.InsightOccurrence?.Ended.ToString("o");

        var impactScores = insightStatusLogEntry.ImpactScores is { Count: > 0 } ? JsonConvert.SerializeObject(insightStatusLogEntry.ImpactScores) : string.Empty;

		return new InsightActivityDto
		{
			ActivityType = InsightActivityType.InsightActivity.ToString(),
			ActivityDate = insightStatusLogEntry.CreatedDateTime,
			SourceId = insightStatusLogEntry.SourceId,
            AppName = insightStatusLogEntry.SourceName,
			UserId = insightStatusLogEntry.UserId,
			SourceType = insightStatusLogEntry.SourceType.HasValue ? insightStatusLogEntry.SourceType.Value : InsightSourceType.Willow,
			Activities = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.Status), insightStatusLogEntry.Status.ToString()),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.Priority), insightStatusLogEntry.Priority.ToString()),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.OccurrenceCount), insightStatusLogEntry.OccurrenceCount.ToString()),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.PreviouslyIgnored), insightStatusLogEntry.PreviouslyIgnored.ToString()),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.PreviouslyResolved), insightStatusLogEntry.PreviouslyResolved.ToString()),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.ImpactScores), impactScores),
				new KeyValuePair<string, string>(nameof(insightStatusLogEntry.Reason), insightStatusLogEntry.Reason),
                new KeyValuePair<string, string>(nameof(OccurrenceStarted), OccurrenceStarted),
                new KeyValuePair<string, string>(nameof(OccurrenceEnded),OccurrenceEnded),
            }
		};
	}

	public static List<InsightActivityDto> MapFromInsightActivities(List<InsightActivity> insightActivity)
	{
		if(insightActivity is {Count: > 0 })
		{
			return insightActivity.Select(MapFromInsightActivities).ToList();
		}
		return new();
	}
	public enum InsightActivityType
	{
		NewTicket = 1,
		TicketModified = 2,
		TicketComment = 3,
		InsightActivity = 4,
	}
}


