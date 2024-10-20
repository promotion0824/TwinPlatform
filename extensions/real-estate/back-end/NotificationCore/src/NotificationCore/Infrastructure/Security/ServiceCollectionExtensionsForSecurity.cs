using NotificationCore.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NotificationCore.Infrastructure.Security;
using NotificationCore.Infrastructure.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensionsForSecurity
{
    public static void AddJwtAuthentication(this IServiceCollection services, string authenticationDomain, string audience, AzureB2CConfiguration azureB2CConfig, IWebHostEnvironment env)

    {
        string domain = $"https://{authenticationDomain}/";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = audience;
            options.RequireHttpsMetadata = !env.IsDevelopment();
        })
        .AddJwtBearer("AzureB2C", options =>
        {
            options.Authority = AzureB2CEndpointBuilder.BuildAuthority(azureB2CConfig);
            options.Audience = azureB2CConfig.ClientId;
            options.RequireHttpsMetadata = !env.IsDevelopment();
        })
        .AddJwtBearer("AzureAD", options =>
        {
            options.Audience = azureB2CConfig.ClientId;
            options.Authority = $"https://login.microsoftonline.com/{azureB2CConfig.TenantId}";
            options.TokenValidationParameters = new IdentityModel.Tokens.TokenValidationParameters
            {
                ValidAudience = azureB2CConfig.ClientId,
                ValidIssuer = $"https://login.microsoftonline.com/{azureB2CConfig.TenantId}/v2.0"
            };
        });

        services.AddAuthorization(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme,
                "AzureB2C",
                "AzureAD");
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

        });
    }
}
