using System.Text.Json;

namespace WorkflowCore.Models
{
    public class UserPreferences
    {
        public bool MobileNotificationEnabled { get; set; }
        public string Language { get; set; }
        public JsonElement Profile { get; set; }
    }
}
