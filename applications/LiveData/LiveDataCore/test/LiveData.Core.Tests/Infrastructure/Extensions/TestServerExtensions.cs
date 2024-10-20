namespace Willow.Tests.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Microsoft.AspNetCore.TestHost;
    using Newtonsoft.Json;
    using Willow.Tests.Infrastructure.Security;

    public static class TestServerExtensions
    {
        public static HttpClient CreateClient(this TestServer server, string auth0UserId)
        {
            var client = server.CreateClient();
            var token = new TestAuthToken
            {
                UserId = Guid.NewGuid(),
                Roles = new string[] { },
                Auth0UserId = auth0UserId,
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthToken.TestScheme, JsonConvert.SerializeObject(token));
            return client;
        }
    }
}
