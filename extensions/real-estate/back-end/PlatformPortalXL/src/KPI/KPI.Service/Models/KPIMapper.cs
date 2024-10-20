using System;
using System.Collections.Generic;
using System.Linq;

namespace KPI.Service.Models;

public static class KPIMapper
{
    private const int _ndxSiteId = 1;
    private const int _metricName = 2;
    private const int _xValue = 3;
    private const int _yValue = 4;
    private const int _yUOM = 5;
    public static KPIBuildingComfortDashboardDto MapToComfortBuildingDashboard(IEnumerable<object> rawData)
    {
          var lstLine = rawData.ToList();
          return new KPIBuildingComfortDashboardDto
          {
              DailyComfortScore = Convert.ToDouble(lstLine[0]),
              DailyZoneAirTemp = Convert.ToDouble(lstLine[1]),
              Date = Convert.ToDateTime(lstLine[2])
          };
    }

    public static KPIBuildingPerformanceScoresDto MapToBuildingPerformanceScores(IEnumerable<object> rawData)
    {
        var lstLine = rawData.ToList();
        return new KPIBuildingPerformanceScoresDto
        {
            MetricName = Convert.ToString(lstLine[_metricName]),
            Date = Convert.ToDateTime(lstLine[_xValue]),
            Value = Convert.ToDouble(lstLine[_yValue])
        };
    }
}
