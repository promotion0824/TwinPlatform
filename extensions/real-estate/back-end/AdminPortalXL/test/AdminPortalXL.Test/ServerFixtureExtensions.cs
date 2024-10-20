using System.Net.Http;
using System;
using AdminPortalXL.Security;

namespace Willow.Tests.Infrastructure
{
    public static class ServerFixtureExtensions
    {
        public static HttpClient CreateClientWithSupervisorRole(this ServerFixture serverFixture, Guid? userId = null)
        {
            return serverFixture.CreateClient(new string[] { UserRoles.Supervisor }, userId);
        }
    }
}