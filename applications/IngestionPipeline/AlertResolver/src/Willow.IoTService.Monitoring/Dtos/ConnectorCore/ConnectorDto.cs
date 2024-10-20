using System;
using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos.ConnectorCore;

public class ConnectorDto
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("connectorTypeId")]
    public Guid ConnectorTypeId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("connectionType")]
    public string? ConnectionType { get; set; }

    [JsonProperty("clientId")]
    public Guid CustomerId { get; set; }

    [JsonProperty("siteId")]
    public Guid SiteId { get; set; }

    [JsonProperty("isEnabled")]
    public bool IsEnabled { get; set; }
        
    [JsonProperty("lastUpdatedAt")]
    public DateTime LastUpdatedAt { get; set; }

    [JsonProperty("isLoggingEnabled")]
    public bool IsLoggingEnabled { get; set; }

    [JsonProperty("isArchived")]
    public bool IsArchived { get; set; }

    [JsonProperty("configuration")]
    public string? Configuration { get; set; }
}