namespace Willow.Api.Authorization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Willow.Api.Authentication;
    using Willow.Api.Authorization.Policies;
    using Willow.Api.Authorization.Requirements;

    /// <summary>
    /// Extension methods for setting up authorization services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static readonly Dictionary<string, AuthorizationPolicy> DefaultPolicies = new()
        {
            [PolicyNames.PlatformAdminUser] = new PlatformAdminUserPolicy(),
            [PolicyNames.PlatformAdminUserOrPlatformApplication] = new PlatformAdminUserOrPlatformApplicationPolicy(),
        };

        /// <summary>
        /// Adds the platform role claims to the specified <see cref="HttpContext" />.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <param name="platformRoles">A list of roles to assigned to the user.</param>
        public static void AddPlatformRoleClaims(this HttpContext context, IEnumerable<string> platformRoles)
        {
            var claims = platformRoles.Select(role => new Claim(ClaimTypes.Role, role));
            context.User.AddIdentity(new ClaimsIdentity(claims));
        }

        /// <summary>
        /// Adds the authorization services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The input list of services.</param>
        public static void AddAuthorizationWithDefaultPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                foreach (var (key, value) in DefaultPolicies)
                {
                    options.AddPolicy(key, value);
                }

                options.DefaultPolicy =
                    new AuthorizationPolicyBuilder(AuthenticationSchemes.AzureAdB2C)
                        .RequireAuthenticatedUser()
                        .Build();
            });

            services.AddSingleton<IAuthorizationHandler, PlatformRoleRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, PlatformAdminUserHandler>();
            services.AddSingleton<IAuthorizationHandler, PlatformApplicationHandler>();
        }
    }
}
