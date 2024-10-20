using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Controllers.Responses;

public class TicketActivityResponse
{
    /// <summary>
    /// Ticket Id
    /// </summary>
    public Guid TicketId { get; set; }

    /// <summary>
    /// Ticket Summary
    /// </summary>
    public string TicketSummary { get; set; }
    /// <summary>
    /// Activity Type
    /// </summary>
    public string ActivityType { get; set; }
    /// <summary>
    /// Activity Date
    /// </summary>
    public DateTime ActivityDate { get; set; }
    /// <summary>
    /// Source Id user id or application id
    /// </summary>
    public Guid SourceId { get; set; }
    /// <summary>
    /// Source Type user or application
    /// </summary>
    public SourceType SourceType { get; set; }
    /// <summary>
    /// List of activities for the ticket
    /// Key is columns name and value is the value of the column
    /// </summary>
    public List<KeyValuePair<string, string>> Activities { get; set; }

    public static TicketActivityResponse MapFromTicketActivity(TicketActivity activity) => new()
    {
        TicketId = activity.TicketId,
        TicketSummary = activity.TicketSummary,
        ActivityType = activity.ActivityType.ToString(),
        ActivityDate = activity.ActivityDate,
        SourceId = activity.SourceId,
        SourceType = activity.SourceType,
        Activities = activity.Activities
    };

    public static List<TicketActivityResponse> MapFromTicketActivities(List<TicketActivity> activities) => activities.Select(MapFromTicketActivity).ToList();


}
