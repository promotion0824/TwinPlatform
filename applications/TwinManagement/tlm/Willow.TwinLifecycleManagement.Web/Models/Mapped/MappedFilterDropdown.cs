using Willow.AzureDigitalTwins.SDK.Client;

namespace Willow.TwinLifecycleManagement.Web.Models.Mapped
{
    public class CombinedMappedEntriesGroupCount
    {
        public IEnumerable<MappedEntriesGroupCount> BuildingIdGroupedEntries { get; set; }
        public IEnumerable<MappedEntriesGroupCount> ConnectorIdGroupedEntries { get; set; }
    }
}
