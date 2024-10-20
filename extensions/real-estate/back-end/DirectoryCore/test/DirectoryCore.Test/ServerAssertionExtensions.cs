using DirectoryCore.Entities;
using DirectoryCore.Services.Auth0;
using DirectoryCore.Test.MockServices;
using Microsoft.Extensions.DependencyInjection;
using Willow.Notifications.Interfaces;
using Willow.Tests.Infrastructure;

namespace DirectoryCore.Test
{
    public static class ServerAssertionExtensions
    {
        public static DirectoryDbContext GetDirectoryDbContext(this ServerAssertion assertion)
        {
            return assertion.MainServices.GetRequiredService<DirectoryDbContext>();
        }

        public static FakeAuth0ManagementService GetAuth0ManagementService(
            this ServerAssertion assertion
        )
        {
            return (FakeAuth0ManagementService)
                assertion.MainServices.GetRequiredService<IAuth0ManagementService>();
        }

        public static MockNotificationService GetEmailService(this ServerAssertion assertion)
        {
            var service = (MockNotificationService)
                assertion.MainServices.GetRequiredService<INotificationService>();
            return service;
        }
    }
}
