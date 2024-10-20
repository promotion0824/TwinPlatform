using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Willow.HealthChecks.Checks;

internal class HealthCheckFederatedPublicApi
    : HealthCheckFederated
{
    public HealthCheckFederatedPublicApi(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    : base(new HealthCheckFederatedArgs("http://publicapi", null, env.IsDevelopment()), httpClientFactory)
    {
    }
}
