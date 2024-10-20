using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PlatformPortalXL.Models
{
    public class AutodeskTokenResponse
    {
        [JsonProperty("access_token")]
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        [JsonPropertyName("expires_in")]
        public long ExpiresIn { get; set; }
    }
}
