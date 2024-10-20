using DirectoryCore.Infrastructure.Security;
using DirectoryCore.Services.AzureB2C;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensionsForSecurity
    {
        public static void AddJwtAuthentication(
            this IServiceCollection services,
            string auth0AuthenticationDomain,
            string auth0Audience,
            AzureADB2COptions azureB2COptions,
            IWebHostEnvironment env
        )
        {
            //Using multiple schemes can cause issues when validating the issuesSigningKey therefore we need to create a context-sensitive JwtBearerHandler that will ignore tokens issued for different issuer than their authority states.! => cfr: https://oliviervaillancourt.com/posts/Fixing-IDX10501-MultipleAuthScheme
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<
                    IPostConfigureOptions<JwtBearerOptions>,
                    JwtBearerPostConfigureOptions
                >()
            );

            string domain = $"https://{auth0AuthenticationDomain}/";

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddScheme<JwtBearerOptions, JwtAuthenticationHandlers>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Authority = domain;
                        options.Audience = auth0Audience;
                        options.RequireHttpsMetadata = !env.IsDevelopment();
                    }
                )
                .AddScheme<JwtBearerOptions, JwtAuthenticationHandlers>(
                    "AzureB2C",
                    options =>
                    {
                        options.Authority = AzureB2CEndpointBuilder.BuildAuthority(azureB2COptions);
                        options.Audience = azureB2COptions.ClientId;
                        options.RequireHttpsMetadata = !env.IsDevelopment();
                    }
                )
                .AddScheme<JwtBearerOptions, JwtAuthenticationHandlers>(
                    "AzureAD",
                    options =>
                    {
                        options.Audience = azureB2COptions.ClientId;
                        options.Authority =
                            $"https://login.microsoftonline.com/{azureB2COptions.TenantId}";
                        options.TokenValidationParameters =
                            new IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidAudience = azureB2COptions.ClientId,
                                ValidIssuer =
                                    $"https://login.microsoftonline.com/{azureB2COptions.TenantId}/v2.0"
                            };
                    }
                );

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    "AzureB2C",
                    "AzureAD"
                );
                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
        }
    }
}
