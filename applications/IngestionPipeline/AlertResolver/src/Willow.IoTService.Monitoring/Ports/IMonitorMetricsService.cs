using System;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos;
using Willow.IoTService.Monitoring.Enums;

namespace Willow.IoTService.Monitoring.Ports
{
    public interface IMonitorMetricsService
    {
        public Task<MetricResponseDto?> GetMetricData(string resourceUri,
                                                    DateTime dateTimeFrom,
                                                    DateTime dateTimeTo,
                                                    MetricInterval metricInterval,
                                                    MetricName metricName,
                                                    MetricAggregation metricAggregation,
                                                    string metricNamespace);
    }
}