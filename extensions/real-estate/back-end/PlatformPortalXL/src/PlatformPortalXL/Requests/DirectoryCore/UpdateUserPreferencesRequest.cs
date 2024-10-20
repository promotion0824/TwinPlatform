using PlatformPortalXL.Models;
using System.Text.Json;

namespace PlatformPortalXL.Requests.DirectoryCore
{
    public class UpdateUserPreferencesRequest
    {
        public JsonElement Profile { get; set; }
        public string Language { get; set; }
    }
}
