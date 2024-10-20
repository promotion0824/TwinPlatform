using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensionsForSecurity
    {
        public static void AddJwtAuthentication(this IServiceCollection services, string authenticationDomain, string audience, IWebHostEnvironment env)
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
            });
        }
    }
}
