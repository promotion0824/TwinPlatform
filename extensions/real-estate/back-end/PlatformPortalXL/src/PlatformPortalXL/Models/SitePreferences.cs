using System.Text.Json;

namespace PlatformPortalXL.Models
{
    public class SitePreferences
    {
        public JsonElement TimeMachine { get; set; }
        public JsonElement ModuleGroups { get; set; }
    }
}
