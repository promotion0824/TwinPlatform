using Newtonsoft.Json;

namespace DirectoryCore.Services.AzureB2C
{
    public class AzureB2CTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
