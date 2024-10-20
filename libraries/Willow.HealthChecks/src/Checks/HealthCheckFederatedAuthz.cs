using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Willow.HealthChecks;

namespace Willow.HealthChecks.Checks;

internal class HealthCheckFederatedAuthz
    : HealthCheckFederated
{
    public HealthCheckFederatedAuthz(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    : base(new HealthCheckFederatedArgs("http://authrz-api", null, env.IsDevelopment()), httpClientFactory)
    {
    }
}
