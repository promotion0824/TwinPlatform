using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Willow.Infrastructure.Services;
using Willow.Tests.Infrastructure.MockServices;

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
            var service = (MockDateTimeService)arrangement.MainServices.GetRequiredService<IDateTimeService>();
            service.UtcNow = currentDateTime;
            return arrangement;
        }

        public static ServerArrangement SetTwinVersionsDto(this ServerArrangement arrangement, TwinHistoryDto twinHistoryDto)
        {
            var service = (MockCachelessAssetService)arrangement.MainServices.GetRequiredService<IAssetService>();
            service.TwinHistoryDto = twinHistoryDto;
            return arrangement;
        }

        public static ServerArrangement SetTwinFieldsDto(this ServerArrangement arrangement, TwinFieldsDto twinFieldsDto)
        {
            var service = (MockCachelessAssetService)arrangement.MainServices.GetRequiredService<IAssetService>();
            service.TwinFieldsDto = twinFieldsDto;
            return arrangement;
        }

		public static ServerArrangement SetAssetNamesForMultiSitesAsync(this ServerArrangement arrangement, List<TwinSimpleDto> dto)
		{
			var service = (MockCachelessAssetService)arrangement.MainServices.GetRequiredService<IAssetService>();
			service.AssetNamesForMultiSitesResponseListDto = dto;
			return arrangement;
		}

	}
}
