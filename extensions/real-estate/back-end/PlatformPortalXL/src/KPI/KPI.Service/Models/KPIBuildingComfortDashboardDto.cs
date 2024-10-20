

using System;

namespace KPI.Service.Models
{
    public  class KPIBuildingComfortDashboardDto
    {
        public double DailyComfortScore { get; set; }
        public double DailyZoneAirTemp { get; set; }
        public DateTime Date { get; set; }
    }
}
