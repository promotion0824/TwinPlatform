using System.Collections.Generic;

namespace WorkflowCore.Services.Apis.Requests;

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
    public string ExternalIdName { get; set; }
}
