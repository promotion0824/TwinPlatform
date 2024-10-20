using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Willow.Common;
using Willow.Tests.Infrastructure.MockServices;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using WorkflowCore.Test.Infrastructure.MockServices;

namespace Willow.Tests.Infrastructure
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
        public static T CreateDbContext<T>(this ServerArrangement arrangement) where T : DbContext
        {
            var options = arrangement.MainServices.GetRequiredService<DbContextOptions<T>>();
            return (T)Activator.CreateInstance(typeof(T), new object[] { options });
        }

        public static ServerArrangement SetCurrentDateTime(this ServerArrangement arrangement, DateTime currentDateTime)
        {
            var mockService = (MockDateTimeService)arrangement.MainServices.GetRequiredService<IDateTimeService>();
            mockService.UtcNow = currentDateTime;
            return arrangement;
        }

        public static IDateTimeService GetDateTimeService(this ServerArrangement arrangement)
        {
            return (MockDateTimeService)arrangement.MainServices.GetRequiredService<IDateTimeService>();
        }

        public static DependencyServiceHttpHandler GetImageHubHttpHandler(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.ImageHub);
        }

        public static IImagePathHelper GetImagePathHelper(this ServerArrangement arrangement)
        {
            return arrangement.MainServices.GetRequiredService<IImagePathHelper>();
        }

        public static DependencyServiceHttpHandler GetDirectoryApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DirectoryCore);
        }

        public static DependencyServiceHttpHandler GetInsightCore(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.InsightCore);
        }
        public static DependencyServiceHttpHandler GetSiteApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.SiteCore);
        }

        public static DependencyServiceHttpHandler GetDynamicsIntegrationForTicketCreationApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DynamicsIntegrationForTicketCreation);
        }
        public static DependencyServiceHttpHandler GetDynamicsIntegrationForTicketUpdateApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DynamicsIntegrationForTicketUpdate);
        }
        public static DependencyServiceHttpHandler GetDigitalTwinApi(this ServerArrangement arrangement)
        {
	        return arrangement.GetHttpHandler(ApiServiceNames.DigitalTwinCore);
        }
		public static ServerArrangement SetSessionData(this ServerArrangement arrangement, SourceType sourceType,Guid sourceId )
		{
			var mockService = (MockSessionService)arrangement.MainServices.GetRequiredService<ISessionService>();
			mockService.SetSessionData(sourceType, sourceId);
			return arrangement;
		}
	}
}

