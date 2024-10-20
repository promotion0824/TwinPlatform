using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Enums;

namespace Willow.IoTService.Monitoring.Models
{
    public interface IAlert
    {
        public string AlertType { get; }

        public int AlertFrequencyInMins { get; }

        public Task<bool> IsActive();

        public Task<AlertNotification?> Evaluate();

        public ConnectorConnectionType[] ConnectorConnectionTypes { get; }

        public ConnectorConfigInfo ConnectorConfigInfo { get; set; }

        public bool IsADXEnabled { get; }
        /// <summary>
        /// A list of alert types which will cause this alert instance to be skipped, if active.
        /// </summary>
        public string[] SkipAlerts { get; }

        string GetAlertSubject()
        {
            return $"{AlertType} | {ConnectorConfigInfo.CustomerName} | {ConnectorConfigInfo.SiteName} | {ConnectorConfigInfo.ConnectorName} | " +
                   $"{ConnectorConfigInfo.ConnectorType} | {ConnectorConfigInfo.ConnectionType}";
        }
    }
}