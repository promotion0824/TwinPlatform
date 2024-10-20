using System;

namespace KPI.Service.Models;

public class KPIPerformanceScoresRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public Guid SiteId { get; set; }
    public Guid PortfolioId { get; set; }
}

