using System;
using Willow.Platform.Models;

namespace Willow.Workflow.Models;
public class TicketStatisticsByPriority : TicketStats
{
    public Guid Id { get; set; }

    public static TicketStats MapTo(TicketStatisticsByPriority ticketStatisticsByPriority)
    {
        if (ticketStatisticsByPriority == null)
            return null;
        else
        {
            return new TicketStats
            {
                OverdueCount = ticketStatisticsByPriority.OverdueCount,
                UrgentCount = ticketStatisticsByPriority.UrgentCount,
                HighCount = ticketStatisticsByPriority.HighCount,
                MediumCount = ticketStatisticsByPriority.MediumCount,
                LowCount = ticketStatisticsByPriority.LowCount,
                OpenCount = ticketStatisticsByPriority.OpenCount,
            };
        }
    }
}

public class TicketStatisticsByStatus : TicketStatsByStatus
{
    public Guid Id { get; set; }

    public static TicketStatsByStatus MapTo(TicketStatisticsByStatus ticketStatisticsByStatus)
    {
        if (ticketStatisticsByStatus == null)
        {
            return null;
        }
        else
        {
            return new TicketStatsByStatus
            {
                OpenCount = ticketStatisticsByStatus.OpenCount,
                ResolvedCount = ticketStatisticsByStatus.ResolvedCount,
                ClosedCount = ticketStatisticsByStatus.ClosedCount,
            };
        }

    }

}

/// <summary>
/// TicketStatsByStatus for a Twin
/// </summary>
public class TwinTicketStatisticsByStatus : TicketStatsByStatus
{
    /// <summary>
    /// Twin identifier
    /// </summary>
    public string TwinId { get; set; }
}
