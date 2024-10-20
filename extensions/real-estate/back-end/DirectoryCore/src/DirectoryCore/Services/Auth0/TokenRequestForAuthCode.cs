using System.Text.Json.Serialization;

namespace DirectoryCore.Services.Auth0
{
    public class TokenRequestForAuthCode
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("redirect_uri")]
        public string RedirectUri { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        public TokenRequestForAuthCode() { }

        public TokenRequestForAuthCode(
            string clientId,
            string clientSecret,
            string code,
            string redirectUri
        )
        {
            GrantType = "authorization_code";
            ClientId = clientId;
            ClientSecret = clientSecret;
            Code = code;
            RedirectUri = redirectUri;
            Scope = $"{clientId} offline_access";
        }
    }
}
