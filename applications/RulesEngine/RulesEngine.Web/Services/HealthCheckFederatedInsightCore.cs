using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RulesEngine.Web;
using Willow.HealthChecks;

namespace Willow.Rules.Web;

/// <summary>
/// Health check that calls out to insight core to retrieve its health status
/// </summary>
public class HealthCheckFederatedInsightCore : HealthCheckFederated
{
    /// <summary>
    /// Creates a new <see cref="HealthCheckFederatedInsightCore"/>
    /// </summary>
    public HealthCheckFederatedInsightCore(IOptions<ServicesConfiguration> options, IHttpClientFactory httpClientFactory, IWebHostEnvironment env) :
        base(new HealthCheckFederatedArgs("http://insightcore", null, env.IsDevelopment()), httpClientFactory)
    {
    }
}



