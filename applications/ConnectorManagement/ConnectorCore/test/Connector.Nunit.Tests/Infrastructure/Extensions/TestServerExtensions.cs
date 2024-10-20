namespace Connector.Nunit.Tests.Infrastructure.Extensions
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.TestHost;
    using Newtonsoft.Json;
    using Willow.Tests.Infrastructure.Security;

    public static class TestServerExtensions
    {
        public static HttpClient CreateClientRandomUser(this TestServer server)
        {
            return CreateClient(server, userId: Guid.NewGuid());
        }

        public static HttpClient CreateClient(this TestServer server, string[] roles = null, Guid? userId = null, string auth0UserId = null)
        {
            var client = server.CreateClient();
            var token = new TestAuthToken
            {
                UserId = userId ?? Guid.NewGuid(),
                Roles = roles ?? [],
                Auth0UserId = auth0UserId,
            };

            var tokenJson = JsonConvert.SerializeObject(token);
            var encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenJson));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthToken.TestScheme, encodedString);
            return client;
        }
    }
}
