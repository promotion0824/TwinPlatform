using System.Collections.Generic;

namespace WorkflowCore.Dto;

/// <summary>
/// The DTO for the ticket status transitions.
/// </summary>
public class TicketStatusTransitionsDto
{
    /// <summary>
    /// The list of ticket status transitions.
    /// </summary>
    public List<TicketStatusTransition> TicketStatusTransitionList { get; set; } = [];
}
/// <summary>
/// Represents a ticket status transition.
/// </summary>
/// <param name="FromStatus"></param>
/// <param name="ToStatus"></param>
public record TicketStatusTransition(int FromStatus, int ToStatus);

