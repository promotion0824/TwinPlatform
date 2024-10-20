using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RulesEngine.Web;
using Willow.HealthChecks;

namespace Willow.Rules.Web;

/// <summary>
/// Health check that calls out to the processor to retrieve its health status
/// </summary>
public class HealthCheckFederatedProcessor : HealthCheckFederated
{
    /// <summary>
    /// Creates a new <see cref="HealthCheckFederatedProcessor"/>
    /// </summary>
    public HealthCheckFederatedProcessor(IOptions<ServicesConfiguration> options, IHttpClientFactory httpClientFactory, IWebHostEnvironment env) :
        base(new HealthCheckFederatedArgs(options.Value.RulesEngineProcessor.BaseUri, null, env.IsDevelopment()), httpClientFactory)
    {
    }
}
