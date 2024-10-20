using Willow.IoTService.Monitoring.Contracts;

namespace Willow.IoTService.Monitoring.Services.AppInsights
{
    public interface IMonitorEventTracker
    {
        void Execute(MonitorEvent monitorEvent);
    }
}