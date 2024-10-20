using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class GetCustomerTicketStatusTests : BaseInMemoryTest
    {
        public GetCustomerTicketStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerHasTicketStatus_GetCustomerTicketStatus_ReturnsTicketStatus()
        {
            var customerId = Guid.NewGuid();
            var expectedTicketStatus = Fixture.CreateMany<CustomerTicketStatus>().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{customerId}/ticketstatus")
                    .ReturnsJson(expectedTicketStatus);

                var response = await client.GetAsync($"customers/{customerId}/ticketstatuses");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<CustomerTicketStatusDto>>();
                var expectedResult = CustomerTicketStatusDto.MapFromModels(expectedTicketStatus);
                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
