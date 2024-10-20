using System.Text.Json;
using DirectoryCore.Enums;

namespace DirectoryCore.Domain
{
    public class CustomerUserPreferences
    {
        public bool MobileNotificationEnabled { get; set; }
        public string Language { get; set; }
        public JsonElement Profile { get; set; }
    }
}
