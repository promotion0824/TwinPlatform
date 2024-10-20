using System;
using DirectoryCore.Services;
using DirectoryCore.Services.AzureB2C;
using DirectoryCore.Test.MockServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Willow.Common;
using Willow.Notifications.Interfaces;
using Willow.Tests.Infrastructure;
using Willow.Tests.Infrastructure.MockServices;

namespace DirectoryCore.Test
{
    public static class ServerArrangementExtensions
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/ef/core/querying/related-data
        /// Entity Framework Core will automatically fix-up navigation properties to any other entities
        /// that were previously loaded into the context instance. So even if you don't explicitly include
        /// the data for a navigation property, the property may still be populated if some or all of the
        /// related entities were previously loaded.
        /// Therefore, a context should be recreated for Arrange and Act parts of the test
        /// </summary>
        public static T CreateDbContext<T>(this ServerArrangement arrangement)
            where T : DbContext
        {
            var options = arrangement.MainServices.GetRequiredService<DbContextOptions<T>>();
            return (T)Activator.CreateInstance(typeof(T), new object[] { options });
        }

        public static ServerArrangement SetCurrentDateTime(
            this ServerArrangement arrangement,
            DateTime currentDateTime
        )
        {
            var service = (MockDateTimeService)
                arrangement.MainServices.GetRequiredService<IDateTimeService>();
            service.UtcNow = currentDateTime;
            return arrangement;
        }

        public static MockNotificationService GetEmailService(this ServerArrangement arrangement)
        {
            var service = (MockNotificationService)
                arrangement.MainServices.GetRequiredService<INotificationService>();
            return service;
        }

        public static DependencyServiceHttpHandler GetAuth0Api(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.Auth0);
        }

        public static DependencyServiceHttpHandler GetImageHubApi(
            this ServerArrangement arrangement
        )
        {
            return arrangement.GetHttpHandler(ApiServiceNames.ImageHub);
        }

        public static FakeAzureB2CService GetAzureB2CService(this ServerArrangement arrangement)
        {
            var service = (FakeAzureB2CService)
                arrangement.MainServices.GetRequiredService<IAzureB2CService>();
            return service;
        }

        public static DependencyServiceHttpHandler GetSiteCoreApi(
            this ServerArrangement arrangement
        )
        {
            return arrangement.GetHttpHandler(ApiServiceNames.SiteCore);
        }
    }
}
