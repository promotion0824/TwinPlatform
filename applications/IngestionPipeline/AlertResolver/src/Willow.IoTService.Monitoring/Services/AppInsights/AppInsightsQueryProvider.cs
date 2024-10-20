using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ApplicationInsights.Query;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Services.AppInsights
{
    public class AppInsightsQueryProvider
    {
        private static ApplicationInsightsDataClient AppInsightsClient(AppInsightsOptions options)
        {
            var creds = new ApiKeyClientCredentials(options.ApiKey);

            var client = new ApplicationInsightsDataClient(new Uri(options.ApiBaseUrl ?? string.Empty), creds);

            return client;
        }

        public async Task<MetricQueryResult> ExecuteQuery(AppInsightsOptions options, string kql, IDictionary<string, object>? parameters = null)
        {
            var query = KqlQueryBuilder.GetQueryWithParams(kql, parameters);

            var queryResults = await AppInsightsClient(options).Query.ExecuteAsync(options.ApplicationId, query);

            var results = new List<IDictionary<string, object>>();

            foreach (var table in queryResults.Tables)
            {
                foreach (var row in table.Rows)
                {
                    results.Add(table.Columns.Zip(row, (column, cell) => new { column.Name, cell }).ToDictionary(entry => entry.Name, entry => entry.cell));
                }
            }

            return new MetricQueryResult
            {
                Results = results
            };
        }
    }
}