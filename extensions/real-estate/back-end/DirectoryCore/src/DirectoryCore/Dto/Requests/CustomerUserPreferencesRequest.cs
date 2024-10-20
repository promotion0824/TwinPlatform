using System.Text.Json;
using DirectoryCore.Enums;

namespace DirectoryCore.Dto.Requests
{
    public class CustomerUserPreferencesRequest
    {
        public bool? MobileNotificationEnabled { get; set; }
        public string Language { get; set; }
        public JsonElement Profile { get; set; }
    }
}
