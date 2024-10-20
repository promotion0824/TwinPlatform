using System.Text.Json.Serialization;

namespace DirectoryCore.Services.Auth0
{
    public class NewAccessTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        public NewAccessTokenRequest() { }

        public NewAccessTokenRequest(string clientId, string clientSecret, string refreshToken)
        {
            GrantType = "refresh_token";
            ClientId = clientId;
            ClientSecret = clientSecret;
            RefreshToken = refreshToken;
        }
    }
}
