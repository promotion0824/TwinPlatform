using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json;

namespace DigitalTwinCore.DTO;

public class BuildingsTwinDto
{
    public string TwinId { get; set; }
    [JsonExtensionData]
    public Dictionary<string, object> ExternalIds { get; set; }
}
