using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Customers
{
    public class CreateOrUpdateTicketStatusTests : BaseInMemoryTest
    {
        public CreateOrUpdateTicketStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateCustomerTicketStatus_ReturnsForbidden()
        {
            var customerId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                var response = await client.PostAsJsonAsync($"customers/{customerId}/ticketStatus", new WorkflowCreateTicketStatusRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_CreateCustomerTicketStatus_ReturnsCreatedTicketStatus()
        {
            var customerId = Guid.NewGuid();
            var request = Fixture.Create<WorkflowCreateTicketStatusRequest>();
            var createdTicketStatus = Fixture.Build<CustomerTicketStatus>().With(x => x.CustomerId, customerId).CreateMany().ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnCustomer(null, Permissions.ManageUsers, customerId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"customers/{customerId}/ticketstatus")
                    .ReturnsJson(createdTicketStatus);

                var response = await client.PostAsJsonAsync($"customers/{customerId}/ticketstatus", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<CustomerTicketStatusDto>>();
                var expectedResult = CustomerTicketStatusDto.MapFromModels(createdTicketStatus);
                result.Should().BeEquivalentTo(expectedResult);
            }
        }
    }
}
