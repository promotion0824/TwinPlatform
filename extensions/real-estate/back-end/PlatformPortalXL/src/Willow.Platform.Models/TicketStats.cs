namespace Willow.Platform.Models;

public class TicketStats
{
    public int OverdueCount { get; set; }
    public int UrgentCount  { get; set; }
    public int HighCount    { get; set; }
    public int MediumCount  { get; set; }
    public int LowCount     { get; set; }
    public int OpenCount    { get; set; }
}

public class TicketStatsByStatus
{
    public int OpenCount                { get; set; }
    public int ResolvedCount            { get; set; }
    public int ClosedCount              { get; set; }
}
