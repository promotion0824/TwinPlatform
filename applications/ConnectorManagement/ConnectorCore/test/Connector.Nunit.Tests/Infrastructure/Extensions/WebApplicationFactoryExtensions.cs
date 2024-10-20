namespace Connector.Nunit.Tests.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Willow.Tests.Infrastructure.Security;

internal static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateClientRandomUser<TEntryPoint>(this WebApplicationFactory<TEntryPoint> server)
        where TEntryPoint : class
    {
        return CreateClient(server, userId: Guid.NewGuid());
    }

    public static HttpClient CreateClient<TEntryPoint>(this WebApplicationFactory<TEntryPoint> server, string[] roles = null, Guid? userId = null, string auth0UserId = null)
        where TEntryPoint : class
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
