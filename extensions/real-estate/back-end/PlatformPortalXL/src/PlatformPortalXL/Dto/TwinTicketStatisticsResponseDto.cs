using System;

namespace PlatformPortalXL.Dto;

public class TwinTicketStatisticsResponseDto
{
    public Guid? UniqueId { get; set; }
    public Guid? GeometryViewerId { get; set; }
    public string TwinId { get; set; }
    public int TicketCount { get; set; }
    public int HighestPriority { get; set; }
}

