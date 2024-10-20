using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using AdminPortalXL.Dto;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Collections.Generic;

namespace AdminPortalXL.Test.Features.Directory.Customers
{
    public class GetCustomersTests : BaseInMemoryTest
    {
        public GetCustomersTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomersExistInMultiRegion_GetCustomers_ReturnsCustomersInAllRegions()
        {
            var regionId0 = ServerFixtureConfigurations.Default.RegionIds[0];
            var regionId1 = ServerFixtureConfigurations.Default.RegionIds[1];
            var customersInRegion0 = Fixture.Build<Customer>().CreateMany(10).ToList();
            var customersInRegion1 = Fixture.Build<Customer>().CreateMany(10).ToList();
            var customerDtosInRegion0 = CustomerDto.Map(customersInRegion0);
            customerDtosInRegion0.ForEach(c => c.RegionId = regionId0);
            var customerDtosInRegion1 = CustomerDto.Map(customersInRegion1);
            customerDtosInRegion1.ForEach(c => c.RegionId = regionId1);
            var expectedCustomers = new List<CustomerDto>();
            expectedCustomers.AddRange(customerDtosInRegion0);
            expectedCustomers.AddRange(customerDtosInRegion1);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().GetRegionalDirectoryApi(regionId0)
                    .SetupRequest(HttpMethod.Get, $"customers?active=true")
                    .ReturnsJson(customersInRegion0);
                server.Arrange().GetRegionalDirectoryApi(regionId1)
                    .SetupRequest(HttpMethod.Get, $"customers?active=true")
                    .ReturnsJson(customersInRegion1);

                var response = await client.GetAsync($"customers");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<CustomerDto>>();
                result.Should().BeEquivalentTo(expectedCustomers);
            }
        }

    }
}
