using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Willow.Common.Middlewares;

public static class ServiceKeyModes
{
    public const string Enforce = "enforce";
}

public class ServiceKeyConfiguration
{
    public string ServiceKey1 { get; init; } = string.Empty;
    public string ServiceKey2 { get; init; } = string.Empty;
    public string ServiceKeyMode { get; init; } = string.Empty;
    public PathString[] ExcludeEndpoints { get; init; }
    public bool IsEnforceMode => string.Equals(ServiceKeyMode , ServiceKeyModes.Enforce, StringComparison.OrdinalIgnoreCase);
};

public class ServiceKeyMiddleware
{
    private const string ServiceKeyHeaderName = "service-key";
    private const string InvalidKeyErrorMessage = "Service key access denied";
    private const string MissingKeyErrorMessage = $"{ServiceKeyHeaderName} header not being passed";
    private const string ServiceKeyAuthConfigKey = "ServiceKeyAuth";
    private static readonly PathString[] DefaultExcludeEndpoints = { "/health", "/healthcheck" };
    
    private readonly RequestDelegate _next;
    private readonly ServiceKeyConfiguration _config;
    private readonly ILogger<ServiceKeyMiddleware> _logger;

    public ServiceKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ServiceKeyMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        
        _next = next;
        _logger = logger;
        _config = configuration.GetSection(ServiceKeyAuthConfigKey).Get<ServiceKeyConfiguration>()
                  ?? throw new KeyNotFoundException(ServiceKeyAuthConfigKey);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        var request = context.Request;
        var response = context.Response;
        
        var serviceKey = request.Headers[ServiceKeyHeaderName].ToString();
        var excludeList = _config.ExcludeEndpoints ?? DefaultExcludeEndpoints;

        if (context.GetEndpoint() != null &&
            excludeList.Any(request.Path.StartsWithSegments) == false &&
            serviceKey != _config.ServiceKey1 &&
            serviceKey != _config.ServiceKey2)
        {
            var errorMessage = string.IsNullOrEmpty(serviceKey) ? MissingKeyErrorMessage : InvalidKeyErrorMessage;
            _logger.LogError(errorMessage);

            if (_config.IsEnforceMode)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await response.WriteAsync(errorMessage);
                return;
            }

            _logger.LogInformation("Skipping authorization due to '{mode}' mode", _config.ServiceKeyMode);
        }

        _logger.LogTrace("{url} passed successfully", request.Path);
        await _next(context);
    }
}

public static class ApplicationBuilderExtensions
{
    public static void UseServiceKeyAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ServiceKeyMiddleware>();
    }
}