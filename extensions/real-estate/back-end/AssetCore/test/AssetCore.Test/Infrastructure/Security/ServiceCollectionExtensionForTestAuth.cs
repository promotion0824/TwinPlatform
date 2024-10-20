﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Willow.Tests.Infrastructure.Security;

namespace Microsoft.Extensions.DependencyInjection
{
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
                    
            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    TestAuthToken.TestScheme);
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
        }
    }
}
