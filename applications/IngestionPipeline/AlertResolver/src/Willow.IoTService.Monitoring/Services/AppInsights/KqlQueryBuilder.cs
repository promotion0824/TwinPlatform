using System.Collections.Generic;
using System.Text;

namespace Willow.IoTService.Monitoring.Services.AppInsights
{
    //WARN: this can suffer from KQL injection .. will need to find a way to support param queries .. for now the app needs to ensure queries are safe ...

    public static class KqlQueryBuilder
    {
        internal static string GetQueryWithParams(string kql, IDictionary<string, object>? parameters = null)
        {
            var query = new StringBuilder(kql);

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    query = query.Replace(item.Key, item.Value?.ToString());
                }
            }

            return query.ToString();
        }
    }
}