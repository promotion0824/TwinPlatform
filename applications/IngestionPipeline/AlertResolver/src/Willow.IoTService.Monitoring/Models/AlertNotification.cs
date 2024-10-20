using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Models;

public class AlertNotification
{
    public AlertNotification() { Id = Guid.NewGuid(); }

    public Guid Id { get; }

    public string AlertName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string AlertKey { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public AlertSeverity Severity { get; set; }

    public string Message { get; set; } = string.Empty;

    public string MessageSupportAudience { get; set; } = string.Empty;      // Message that is only sent to support audience

    public bool AutoResolve { get; set; }

    internal IAlert? OriginalAlert { get; set; }

    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    public Dictionary<string, object> DataSupportAudience { get; set; } = new Dictionary<string, object>();     // Data that is only sent to support audience

    public IEnumerable<AlertAttachment>? Attachments { get; set; }

    public AlertNotification WithData(string dataKey, object value, bool supportAudience = false)
    {
        if (value != null)
        {
            if (supportAudience)
            {
                DataSupportAudience.Add(dataKey, value);
            }
            else
            {
                Data.Add(dataKey, value);
            }
        }

        return this;
    }

    public AlertNotification WithConnectorId(Guid? connectorId, bool supportAudience = false)
    {
        if (connectorId.HasValue)
        {
            AppendData("ConnectorId", connectorId, supportAudience);
        }

        return this;
    }

    public AlertNotification WithSiteId(Guid siteId) { return AppendData("SiteId", siteId); }

    public AlertNotification WithSiteName(string? siteName, bool supportAudience = false)
    {
        if (!string.IsNullOrEmpty(siteName))
        {
            AppendData("SiteName", siteName, supportAudience);
        }

        return this;
    }

    public AlertNotification WithConnectorName(string? connectorName, bool supportAudience = false)
    {
        if (!string.IsNullOrEmpty(connectorName))
        {
            AppendData("ConnectorName", connectorName, supportAudience);
        }

        return this;
    }

    public AlertNotification WithCustomerId(Guid? customerId)
    {
        if (customerId.HasValue)
        {
            AppendData("CustomerId", customerId);
        }

        return this;
    }

    public AlertNotification WithCustomerName(string? customerName, bool supportAudience = false)
    {
        if (!string.IsNullOrEmpty(customerName))
        {
            AppendData("CustomerName", customerName, supportAudience);
        }

        return this;
    }

    public AlertNotification WithActionTime(string? actionableTime)
    {
        if (!string.IsNullOrEmpty(actionableTime))
        {
            AppendData("Action timeframe", actionableTime);
        }

        return this;
    }

    public AlertNotification WithConnectorConnectionType(
        ConnectorConnectionType connectorConnectionType,
        bool supportAudience = false)
    {
        AppendData("ConnectorConnectionType", connectorConnectionType.ToString(), supportAudience);

        return this;
    }

    public AlertNotification WithConnectorType(string connectorType, bool supportAudience = false)
    {
        AppendData("ConnectorType", connectorType, supportAudience);

        return this;
    }

    public AlertNotification WithConnectorConfigInfo(ConnectorConfigInfo connectorConfigInfo)
    {
        WithCustomerName(connectorConfigInfo.CustomerName);
        WithSiteName(connectorConfigInfo.SiteName);
        WithConnectorName(connectorConfigInfo.ConnectorName);
        WithCustomerId(connectorConfigInfo.CustomerId);
        WithSiteId(connectorConfigInfo.SiteId);
        WithConnectorId(connectorConfigInfo.ConnectorId);
        WithConnectorConnectionType(connectorConfigInfo.ConnectionType);
        WithConnectorType(connectorConfigInfo.ConnectorType ?? string.Empty);

        return this;
    }

    public AlertNotification WithAlertAttachment(IEnumerable<AlertAttachment>? alertAttachments)
    {
        if (alertAttachments?.Any() == true)
        {
            Attachments = alertAttachments;
        }

        return this;
    }

    /// <summary>
    /// Add third party consumer information to the alert notification
    /// </summary>
    /// <param name="thirdPartyConsumers">List of third party consumers</param>
    /// <param name="connectorConfigInfo">Information on this connector</param>
    /// <param name="supportAudienceMessage">Extra message to add for a support audience if a third party consumer is identified</param>
    /// <param name="supportAudienceData">Add extra support audience data if a third party consumer is identified</param>
    /// <returns>this</returns>
    public AlertNotification WithThirdPartyConsumers(
        ThirdPartyConsumerOptions thirdPartyConsumers,
        ConnectorConfigInfo connectorConfigInfo,
        string supportAudienceMessage,
        bool supportAudienceData)
    {
        // Check parameters for null
        if (thirdPartyConsumers == null || connectorConfigInfo == null)
        {
            return this;
        }

        // Check consumers for this site
        var siteConsumers = thirdPartyConsumers.GetThirdPartyConsumers(connectorConfigInfo.SiteId);
        if (siteConsumers == null || siteConsumers.Count == 0)
        {
            return this;
        }

        // Build a list of consumer names
        var externalList = siteConsumers.Select(consumer => consumer.External);
        var externalString = string.Join(", ", externalList);

        // Add third party data and message
        if (supportAudienceData)
        {
            this.AppendData("ThirdPartyConsumers", externalString, supportAudience: true);
        }

        this.AppendData("ThirdPartyConsumers", externalString, supportAudience: false);
        this.MessageSupportAudience = string.Format(supportAudienceMessage, connectorConfigInfo.ConnectorName, connectorConfigInfo.SiteName, externalString);

        return this;
    }

    private AlertNotification AppendData(string key, object value, bool supportAudience = false)
    {
        var targetData = (supportAudience) ? DataSupportAudience : Data;
        if (!targetData.ContainsKey(key) && value != null)
        {
            targetData.Add(key, value);
        }

        return this;
    }
}

public record AlertAttachment(string Content, string FileName);
