using System;
using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos.DirectoryCore;

public class SiteDto
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("customerId")]
    public Guid CustomerId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }
}