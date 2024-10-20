namespace Willow.Msm.Connector.Models
{
    /// <summary>
    /// Represents the body of the response received when requesting an authentication token
    /// including the access token, its expiry, and the token type.
    /// </summary>
    public class WillowTokenResponseBody
    {
        /// <summary>
        /// Gets or sets the access token used for authenticating subsequent requests.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds after which the access token expires.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the type of the token, typically indicating the authentication scheme to be used with the token, e.g., "Bearer".
        /// </summary>
        public string? TokenType { get; set; }
    }
}
