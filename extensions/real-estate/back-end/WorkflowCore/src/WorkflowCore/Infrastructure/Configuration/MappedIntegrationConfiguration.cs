using System.Collections.Generic;
using System;
using System.Linq;

namespace WorkflowCore.Infrastructure.Configuration;

public class MappedIntegrationConfiguration
{
    public MappedIntegrationConfiguration()
    {
        SiteIds = Enumerable.Empty<Guid>().ToList();
    }
    /// <summary>
    /// Enable or disable the integration
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The name of the customer
    /// </summary>
    public string CustomerName { get; set; }
    /// <summary>
    /// customer id
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Site ids to be used for the integration
    /// </summary>
    public List<Guid> SiteIds { get; set; }
    /// <summary>
    /// The url to be used to send tickets events (create|update|delete)
    /// </summary>
    public string WebhookUrl { get; set; }

    /// <summary>
    /// Webhook Auth Header.
    /// </summary>
    public string WebhookAuthHeader { get; init; } = string.Empty;

    /// <summary>
    /// Webhook Auth key.
    /// </summary>
    public string WebhookAuthKey { get; init; } = string.Empty;

    /// <summary>
    /// if the tickets will be read only in Willow
    /// no update or create in Willow
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Override ticket source name with SourceName
    /// the SourceName represents the third-party CMMS
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// Mapped connector id to sync ticket meta data
    /// </summary>
    public string TicketMetaDataConnectorId { get; set; }

    /// <summary>
    /// Enable or disable ticket metadata sync background service 
    /// </summary>
    public bool IsTicketMetaDataSyncEnabled { get; set; }

    /// <summary>
    /// Base url for MTI API
    /// </summary>
    public string MtiBaseUrl { get; set; }

}
