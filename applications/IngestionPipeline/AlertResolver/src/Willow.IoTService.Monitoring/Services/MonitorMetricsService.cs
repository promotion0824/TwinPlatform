using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Monitoring.Dtos;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Extensions;
using Willow.IoTService.Monitoring.Ports;

namespace Willow.IoTService.Monitoring.Services
{
    public class MonitorMetricsService : IMonitorMetricsService
    {
        private readonly HttpClient _httpClient;
        private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
        private readonly string _baseUrl = @"/{resourceUri}/providers/Microsoft.Insights/metrics?timespan={dateTimeFrom}/{dateTimeTo}&interval={metricInterval}&metricnames={metricName}&aggregation={metricAggregation}&metricNamespace={metricNamespace}&autoadjusttimegrain=true&validatedimensions=false&api-version=2019-07-01";
        private readonly ILogger _logger;

        public MonitorMetricsService(HttpClient httpClient, ILogger<MonitorMetricsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MetricResponseDto?> GetMetricData(string resourceUri,
                                                           DateTime dateTimeFrom,
                                                           DateTime dateTimeTo,
                                                           MetricInterval metricInterval,
                                                           MetricName metricName,
                                                           MetricAggregation metricAggregation,
                                                           string metricNamespace)
        {
            if (!string.IsNullOrWhiteSpace(resourceUri) && resourceUri.StartsWith('/'))
            {
                resourceUri = resourceUri.Remove(0, 1);
            }

            var url = _baseUrl.Replace("{resourceUri}", resourceUri)
                              .Replace("{dateTimeFrom}", dateTimeFrom.ToString("o", _culture))
                              .Replace("{dateTimeTo}", dateTimeTo.ToString("o", _culture))
                              .Replace("{metricInterval}", metricInterval.ToString())
                              .Replace("{metricName}", metricName.ToString())
                              .Replace("{metricAggregation}", metricAggregation.ToString())
                              .Replace("{metricNamespace}", HttpUtility.UrlEncode(metricNamespace));

            var metricResponse = await _httpClient.WithAzureAdAuth(_logger)
                                                  .Result
                                                  .GetFromJsonAsync<MetricResponseDto?>(url);

            return metricResponse;
        }
    }
}