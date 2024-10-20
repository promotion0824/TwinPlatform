namespace Willow.Tests.Infrastructure.Security
{
    using System;

    public class TestAuthToken
    {
        public const string TestScheme = "TestScheme";

        public string Scope { get; set; }

        public string[] Roles { get; set; }

        public string Auth0UserId { get; set; }

        public Guid UserId { get; set; }
    }
}
