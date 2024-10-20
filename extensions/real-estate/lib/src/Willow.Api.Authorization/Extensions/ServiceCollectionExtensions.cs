using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.Api.Authorization
{
    public static class ServiceCollectionExtensions
    {
        private const string WillowAuthenticationScheme = "willow";

        public static AuthenticationBuilder AddWillowAuthentication(this IServiceCollection services, B2CConfig config)
        {
             return services.AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = WillowAuthenticationScheme;
                 options.DefaultChallengeScheme = WillowAuthenticationScheme;
             })
             .AddPolicyScheme(WillowAuthenticationScheme, WillowAuthenticationScheme, options =>
             {
                 options.ForwardDefaultSelector = context =>
                 {
                     if (context.Request.Headers.ContainsKey("Authorization"))
                     {
                         var authHeader = context.Request.Headers["Authorization"];
                         if (authHeader[0].StartsWith("Bearer", StringComparison.InvariantCulture))
                         {
                             return JwtBearerDefaults.AuthenticationScheme;
                         }
                     }
                     return CookieAuthenticationDefaults.AuthenticationScheme;
                };
             })
             .AddCookie(options =>
              {
                  options.Cookie.Name = "WillowPlatformAuth";
                  options.Cookie.Path = "/";
                  options.Cookie.HttpOnly = true;
                  options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                  options.Events.OnRedirectToLogin = context =>
                  {
                      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                      return Task.CompletedTask;
                  };

                  options.Events.OnRedirectToAccessDenied = context =>
                  {
                      context.Response.StatusCode = StatusCodes.Status403Forbidden;
                      return Task.CompletedTask;
                  };

                  options.Events.OnRedirectToLogout = context =>
                  {
                      context.Response.Cookies.Delete("WillowPlatformAuth");

                      return Task.CompletedTask;
                  };
              })
             .AddJwtBearer("AzureB2C", options =>
             {
                 options.Authority = config.Authority;
                 options.Audience = config.ClientId;
                 options.RequireHttpsMetadata = false;
             })
             .AddJwtBearer("AzureAD", options =>
             {
                 options.Authority = $"https://login.microsoftonline.com/{config.TenantId}";
                 options.Audience  = config.ClientId;
                 options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                 {
                     ValidAudience = config.ClientId,
                     ValidIssuer   = $"https://login.microsoftonline.com/{config.TenantId}/v2.0"
                 };
             });
        }

        public static IServiceCollection AddWillowAuthorization(this IServiceCollection services, Action<AuthorizationOptions> configure)
        {
            services
                .AddAuthorization(options =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                        "AzureB2C",
                        "AzureAD");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                    configure(options);
                });

            return services;
        }
    }
}
