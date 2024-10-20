using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteCore.ServiceTests.Server
{
    // Test scheme for Service tests auth and its handler.
    public class TestSchemeProvider : AuthenticationSchemeProvider
    {
        
        public TestSchemeProvider(IOptions<AuthenticationOptions> options)
            : base(options)
        {

        }

        public TestSchemeProvider(
            IOptions<AuthenticationOptions> options,
            IDictionary<string, AuthenticationScheme> schemes)
            : base(options, schemes)
        {

        }

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            var scheme = new AuthenticationScheme(
                IdentityConstants.ApplicationScheme,
                IdentityConstants.ApplicationScheme,
                typeof(TestAuthHandler)
                );

            return Task.FromResult(scheme);
        }
    }
}
