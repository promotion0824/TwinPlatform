using Microsoft.Extensions.DependencyInjection;
using AdminPortalXL.Services;
using System;
using Willow.Infrastructure.Services;
using Willow.Tests.Infrastructure.MockServices;
using Willow.Infrastructure.MultiRegion;
using AdminPortalXL.Models.Directory;
using AdminPortalXL.Dto;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Willow.Tests.Infrastructure
{
    public static class ServerArrangementExtensions
    {
        public static ServerArrangement SetCurrentDateTime(this ServerArrangement arrangement, DateTime currentDateTime)
        {
            var service = (MockDateTimeService)arrangement.MainServices.GetRequiredService<IDateTimeService>();
            service.UtcNow = currentDateTime;
            return arrangement;
        }

        public static DependencyServiceHttpHandler GetDirectoryApi(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler(ApiServiceNames.DirectoryCore);
        }

        public static DependencyServiceHttpHandler GetRegionalDirectoryApi(this ServerArrangement arrangement, string regionId)
        {
            return arrangement.GetHttpHandler(MultiRegionHelper.ServiceName(ApiServiceNames.RegionalDirectoryCore, regionId));
        }

        public static void SetCustomerRegion(this ServerArrangement arrangement, ServerFixtureConfiguration serverFixture, string regionId, Guid customerId)
        {
            var customer = new Customer
            {
                Id = customerId,
                Name = "name",
                Country = "country",
                LogoId = null,
                Status = 0
            };

            foreach (var id in serverFixture.RegionIds)
            {
                arrangement.GetRegionalDirectoryApi(id)
                    .SetupRequest(HttpMethod.Get, $"customers?active=true")
                    .ReturnsJson(id == regionId ? new Customer[] { customer } : new Customer[0]);
            }
        }

        public static void SetCustomerRegions(this ServerArrangement arrangement, ServerFixtureConfiguration serverFixture, IEnumerable<Guid> customerIdsInRegion0, IEnumerable<Guid> customerIdsInRegion1)
        {
            var regionId0 = serverFixture.RegionIds[0];
            var customersInRegion0 = customerIdsInRegion0.Select(id => new Customer
            {
                Id = id,
                Name = "name0",
                Country = "country0",
                LogoId = null,
                Status = 0
            });
            arrangement.GetRegionalDirectoryApi(regionId0)
                .SetupRequest(HttpMethod.Get, $"customers")
                .ReturnsJson(customersInRegion0);

            var regionId1 = serverFixture.RegionIds[1];
            var customersInRegion1 = customerIdsInRegion1.Select(id => new Customer
            {
                Id = id,
                Name = "name1",
                Country = "country1",
                LogoId = null,
                Status = 0
            });
            arrangement.GetRegionalDirectoryApi(regionId1)
                .SetupRequest(HttpMethod.Get, $"customers")
                .ReturnsJson(customersInRegion1);
        }
    }
}
