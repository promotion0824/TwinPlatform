using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinCore.DTO;

public class GetBuildingTwinsByExternalIdsRequest
{
    public GetBuildingTwinsByExternalIdsRequest()
    {
        ExternalIdValues = new List<string>();
    }
    /// <summary>
    /// external id values should not exceed 50 items
    /// limitation imposed by ADT https://learn.microsoft.com/en-us/azure/digital-twins/reference-service-limits
    /// </summary>
    public List<string> ExternalIdValues { get; set; }
    /// <summary>
    /// External Id property name
    /// </summary>
    [StringLength(100)]
    public string ExternalIdName { get; set; }
}
