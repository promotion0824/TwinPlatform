using System.Collections.Generic;
using Newtonsoft.Json;
namespace WorkflowCore.Dto;

public class BuildingsTwinDto
{
    public BuildingsTwinDto()
    {
        ExternalIds = new Dictionary<string, object>();
    }
    public string TwinId { get; set; }
    [JsonExtensionData]
    public Dictionary<string, object> ExternalIds { get; set; }
}

