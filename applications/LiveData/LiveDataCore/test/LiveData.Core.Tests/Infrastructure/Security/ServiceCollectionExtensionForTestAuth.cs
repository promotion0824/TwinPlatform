namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.AspNetCore.Authentication;
    using Willow.Tests.Infrastructure.Security;

    public static class ServiceCollectionExtensionForTestAuth
    {
        public static void AddTestAuthentication(this IServiceCollection services)
        {
            services
               .AddAuthentication(options =>
               {
                   options.DefaultAuthenticateScheme = TestAuthToken.TestScheme;
                   options.DefaultChallengeScheme = TestAuthToken.TestScheme;
               })
               .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthToken.TestScheme, "Test Auth", o => { });
        }
    }
}
