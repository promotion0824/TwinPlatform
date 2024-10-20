using System.Text.Json;

namespace SiteCore.Requests
{
    public class SitePreferencesRequest
    {
        public JsonElement TimeMachine { get; set; }
        public JsonElement ModuleGroups { get; set; }
    }
}
