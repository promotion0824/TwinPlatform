namespace Willow.Api.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Willow.Api.Common;
using Willow.Api.Common.Extensions;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

/// <summary>
/// Extension methods for setting up authentication services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add cookie authentication to the service collection.
    /// </summary>
    /// <param name="services">The collection of services for the application.</param>
    /// <param name="cookieName">The name of the cookie to create.</param>
    public static void AddCookieAuthentication(
        this IServiceCollection services,
        string cookieName = "WillowPlatformAuth")
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // used for GDPR to avoid sending non essential cookies which can cause unexpected behavior
            options.CheckConsentNeeded = _ => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        const string authenticationScheme = "willow";

        _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = authenticationScheme;
                options.DefaultChallengeScheme = authenticationScheme;
            })
            .AddPolicyScheme(authenticationScheme, authenticationScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                    {
                        if (authHeader.Count > 0)
                        {
                            var tokenHeader = authHeader[0];

                            if (tokenHeader != null)
                            {
                                if (tokenHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                                {
                                    return AuthenticationSchemes.AzureAdB2C;
                                }
                            }
                        }
                    }

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = cookieName;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToLogout = context =>
                {
                    context.Response.Cookies.Delete(cookieName);

                    return Task.CompletedTask;
                };

                // Extract email from encoded authorization claim and it to user principal
                options.Events.OnValidatePrincipal = context =>
                {
                    var token = context.Principal?.FindFirstValue(ClaimTypes.Authentication);

                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var email = jwt.Claims.First(claim => claim.Type == WillowClaimTypes.Email).Value;
                    var identity = new ClaimsIdentity(new[]
                        {
                            new Claim(WillowClaimTypes.Emails, email),
                        },
                        authenticationScheme);
                    context.Principal?.AddIdentity(identity);

                    return Task.CompletedTask;
                };
            });
    }

    /// <summary>
    /// Add Azure AD authentication to the service collection.
    /// </summary>
    /// <param name="services">The list of services for the application.</param>
    /// <param name="configuration">The configuration for the application.</param>
    /// <param name="requireHttpsMetadata">Whether or not the BearToken requires https meta data. Defaults to true.</param>
    public static void AddAzureAdAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        bool requireHttpsMetadata = true)
    {
        var azureB2CSection = configuration.GetSection("AzureAdB2C");
        var azureAdSection = configuration.GetSection("AzureAD", AzureADOptions.PopulateDefaults);

        services.Configure<AzureAdB2COptions>(azureB2CSection);
        services.Configure<AzureADOptions>(azureAdSection);
        services.AddSingleton<IClientCredentialTokenService, ClientCredentialTokenService>();

        var azureB2COptions = azureB2CSection.Get<AzureAdB2COptions>();
        var azureAdOptions = azureAdSection.Get<AzureADOptions>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationSchemes.AzureAdB2C;
                options.DefaultChallengeScheme = AuthenticationSchemes.AzureAdB2C;
            })
            .AddJwtBearer(AuthenticationSchemes.AzureAdB2C, options =>
            {
                if (azureB2COptions != null)
                {
                    options.Authority = azureB2COptions.Authority;
                    options.Audience = azureB2COptions.ClientId;
                }

                options.RequireHttpsMetadata = requireHttpsMetadata;
            })
            .AddJwtBearer(AuthenticationSchemes.AzureAd, options =>
            {
                if (azureAdOptions != null)
                {
                    options.MetadataAddress = azureAdOptions.MetadataAddress;
                    options.Authority = azureAdOptions.Authority;
                    options.Audience = azureAdOptions.Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = azureAdOptions.ClientCredentialIssuer,
                        ValidateAudience = false,
                    };
                }
            });
    }

    /// <summary>
    /// Add the client credential token service to the service collection.
    /// </summary>
    /// <param name="services">The list of services for the application.</param>
    /// <param name="configuration">The configuration interface for the application.</param>
    public static void AddClientCredentialToken(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var azureAdSection = configuration.GetSection("AzureAD", AzureADOptions.PopulateDefaults);
        services.Configure<AzureADOptions>(azureAdSection);
        services.AddSingleton<IClientCredentialTokenService, ClientCredentialTokenService>();
    }
}
