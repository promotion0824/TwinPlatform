using System;
using Newtonsoft.Json;

namespace Willow.IoTService.Monitoring.Dtos.DirectoryCore;

public class CustomerDto
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    public CustomerDto(Guid guid, string name)
    {
        Id = guid;
        Name = name;
    }
}