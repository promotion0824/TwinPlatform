using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets;

public class GetTicketsCountsByCreatedDateTests : BaseInMemoryTest
{
    public GetTicketsCountsByCreatedDateTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetTicketsCountsByCreatedDate_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("tickets/twins/spaceTwinId/ticketCountsByDate");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }

    [Theory]
    [InlineData("2024-01-01", null)]
    [InlineData(null, "2024-01-01")]
    [InlineData(null, null)]
    public async Task StartOrEndDateIsMissing_GetTicketsCountsByCreatedDate_ReturnBadRequest(string startDate, string endDate)
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var url = $"tickets/twins/spaceTwinId/ticketCountsByDate";
            if (!string.IsNullOrEmpty(startDate))
            {
                url = QueryHelpers.AddQueryString(url, "startDate", startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                url = QueryHelpers.AddQueryString(url, "endDate", endDate);
            }
            var result = await client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var response = await result.Content.ReadAsStringAsync();
            response.Should().Contain("The start date and end date are required");
        }
    }

    [Fact]
    public async Task StartDateGreaterThanEndDate_GetTicketsCountsByCreatedDate_ReturnBadRequest()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var url = $"tickets/twins/spaceTwinId/ticketCountsByDate";

            url = QueryHelpers.AddQueryString(url, "startDate", "2025-01-01");
            url = QueryHelpers.AddQueryString(url, "endDate", "2024-01-01");

            var result = await client.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var response = await result.Content.ReadAsStringAsync();
            response.Should().Contain("The end date must be greater than start date");
        }
    }

    [Fact]
    public async Task TicketExistsWithinDateRange_GetTicketsCountsByCreatedDate_ReturnTicketCount()
    {
        var spaceTwinId = "spaceTwinId";
        var startDate = "2024-01-01";
        var endDate = "2024-01-31";
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var expectedResponse = Fixture.Build<TicketCountsByDateResponse>().Create();
            server.Arrange().GetWorkflowApi()
                 .SetupRequest(HttpMethod.Get, $"tickets/twins/{spaceTwinId}/ticketCountsByDate?startDate={startDate}&endDate={endDate}")
                 .ReturnsJson(expectedResponse);

            var response = await client.GetAsync($"tickets/twins/{spaceTwinId}/ticketCountsByDate?startDate={startDate}&endDate={endDate}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualResponse = await response.Content.ReadAsAsync<TicketCountsByDateResponse>();
            actualResponse.Should().BeEquivalentTo(expectedResponse);

        }
    }
}

