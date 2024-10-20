namespace Willow.Msm.Connector.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Get Token for the Willow Client.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public async Task<WillowToken> GetToken()
        {
            if (this.willowToken.ExpiryDate > DateTime.UtcNow.AddMinutes(5))
            {
                this.log!.LogInformation("Token is still valid.  No need to aquire new token.");
                return this.willowToken;
            }

            this.log!.LogInformation("Getting new token");
            using var client = httpClientFactory.CreateClient();
            var tokenEndpoint = $"https://{this.carbonActivityRequestMessage!.OrganizationShortName}.app.willowinc.com/publicapi/v2/oauth2/token";

            var requestBody = JsonConvert.SerializeObject(new
            {
                clientId = this.carbonActivityRequestMessage.ClientId,
                clientSecret = this.carbonActivityRequestMessage.ClientSecret,
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json"),
            };

            using var responseMessage = await client.SendAsync(req);

            var responseBody = responseMessage.Content.ReadAsStringAsync().Result;

            var tokenResponse = JsonConvert.DeserializeObject<WillowTokenResponseBody>(responseBody);

            this.willowToken.Token = tokenResponse!.AccessToken;
            this.willowToken.ExpiryDate = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return this.willowToken;
        }
    }
}
