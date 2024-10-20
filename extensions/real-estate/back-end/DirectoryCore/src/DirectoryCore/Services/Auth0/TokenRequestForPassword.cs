using System.Text.Json.Serialization;

namespace DirectoryCore.Services.Auth0
{
    public class TokenRequestForPassword
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; }

        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        public TokenRequestForPassword() { }

        public TokenRequestForPassword(
            string clientId,
            string clientSecret,
            string userName,
            string password
        )
        {
            GrantType = "password";
            ClientId = clientId;
            ClientSecret = clientSecret;
            UserName = userName;
            Password = password;
            Scope = "openid";
        }
    }
}
