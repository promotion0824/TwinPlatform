using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using KPI.Service.Models;
using Willow.Common;
using Willow.KPI.Repository;

namespace Willow.KPI.Service
{
    public interface ICoreKPIService 
    {
        Task<IEnumerable<Metric>> GetByMetric(Guid portfolioId, string viewName, object filter = null, bool sortByX = true, bool isDateMetric = false);
        Task<List<IEnumerable<object>>> GetRawData(KPIViewNames viewName, string scopeId, KPIBaseRequest request);
        Task<List<IEnumerable<object>>> GetRawDataForPerformanceScoresByDate(KPIPerformanceScoresRequest request);
    }

    public class KPIAPI : ICoreKPIService
    {
        private readonly IQueryRepository _repo;
        private readonly string _schemaName;

        private const int _ndxSiteId  = 1;
        private const int _metricName = 2;
        private const int _xValue     = 3;
        private const int _yValue     = 4;
        private const int _yUOM       = 5;

        public KPIAPI(IQueryRepository repo, string schemaName)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));

            if(string.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentException(nameof(schemaName));

            _schemaName = schemaName;
        }

        public async Task<List<IEnumerable<object>>> GetRawData(Guid portfolioId, string viewName, object filter = null, bool sortByX = true)
        {
            var parms = new List<object>
            {
                portfolioId,
            };
            parms.AddRange(filter.ToDictionary()?.Values.ToList() ?? new List<object>());

            var query = $"SELECT * FROM TABLE({_schemaName}.get_{viewName}_data_udtf({string.Join(",", parms.Select(x => "?"))}))";
            var results = (await _repo.Query(query, parms.ToArray())).ToList();
          
            if (sortByX && results.Count > 0)
            {
                if (results[0].ToList()[_xValue] is DateTime)
                    results.Sort(new XDateTimeComparer());
                else
                    results.Sort(new XTextComparer());
            }

            return results;
        }

        public async Task<List<IEnumerable<object>>> GetRawData(KPIViewNames viewName, string scopeId, KPIBaseRequest request)
        {

            var query = GetQueryByKpiViewName(viewName,scopeId ,request);
            var results = (await _repo.Query(query)).ToList();

            return results;
        }

        public async Task<IEnumerable<Metric>> GetByMetric(Guid portfolioId, string viewName, object filter = null, bool sortByX = true, bool isDateMetric=false)
        {
            var results  = await GetRawData(portfolioId, viewName, filter, sortByX);
            var metrics  = new Dictionary<string, Metric>();

            foreach(var line in results)
            {
                var   lstLine = line.ToList();
                Guid? siteId  = !isDateMetric && string.IsNullOrWhiteSpace(lstLine[_ndxSiteId]?.ToString()) ? null : Guid.Parse(lstLine[_ndxSiteId].ToString());
                var   xValue   = siteId.HasValue && !isDateMetric ? siteId : lstLine[_xValue];

                var name    = lstLine[_metricName].ToString().Trim();
                var yUOM    = lstLine.Count > _yUOM ? lstLine[_yUOM]?.ToString() : null;
                var dataPt  = new DataPoint {  XValue = xValue, YValue = lstLine[_yValue]};

                if(metrics.ContainsKey(name))
                    metrics[name].Values.Add(dataPt);
                else
                { 
                    metrics.Add(name, new Metric { Name = name, Values = new List<DataPoint> { dataPt } } );  
                    metrics[name].YUOM = yUOM;
                }
            }

            return metrics.Values;
        }

        public async Task<List<IEnumerable<object>>> GetRawDataForPerformanceScoresByDate(KPIPerformanceScoresRequest request)
        {
            var queryParameters = new List<object>
            {
                request.PortfolioId,
                request.SiteId,
                request.StartDate,
                request.EndDate,
                true,
                true,
                "date"
            };
            var query = $"select * from TABLE(app_dashboards.get_building_data_udtf({string.Join(",", queryParameters.Select(x => "?"))}))";
            var results = (await _repo.Query(query, queryParameters.ToArray())).ToList();
            return results;
        }


        private class XTextComparer : IComparer<IEnumerable<object>>
        {
            public int Compare([AllowNull] IEnumerable<object> x, [AllowNull] IEnumerable<object> y)
            {
                if(x == null)
                    return 1;

                if(y == null)
                    return -1;

                var xList = x.ToList();
                var yList = y.ToList();

                return xList[KPIAPI._xValue]?.ToString()?.CompareTo(yList[KPIAPI._xValue]?.ToString()) ?? -1;
            }
        }
        private class XDateTimeComparer : IComparer<IEnumerable<object>>
        {
            public int Compare([AllowNull] IEnumerable<object> x, [AllowNull] IEnumerable<object> y)
            {
                if(x == null)
                    return 1;

                if(y == null)
                    return -1;

                var xList  = x.ToList();
                var yList  = y.ToList();
                var xValue = (DateTime)xList[KPIAPI._xValue];
                var yValue = (DateTime)yList[KPIAPI._xValue];

                return xValue.CompareTo(yValue);
            }
        }

        private static string GetQueryByKpiViewName(KPIViewNames viewName,string scopeId,KPIBaseRequest request)
        {
            return viewName switch
            {
                KPIViewNames.BuildingComfortDashboard =>
                    $"select AVG(COMFORT_SCORE) as DailyComfortScore, avg(Avg_Zone_Air_Temp) as DailyZoneAirTemp, c.date from COMFORT_DAILY_METRICS c left join BUILDING_SCOPES b on c.building_id = b.building_id where (c.DATE BETWEEN '{request.StartDate}' AND '{request.EndDate}') and b.scope_id={scopeId} group by c.date order by c.date",
                _ => throw new ArgumentOutOfRangeException(nameof(viewName), viewName, null)
            };
        }
    }
}
