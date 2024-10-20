using System.Text.Json;

namespace SiteCore.Domain
{
    public class SitePreferences
    {
        public JsonElement TimeMachine { get; set; }
        public JsonElement ModuleGroups { get; set; }
    }
}
