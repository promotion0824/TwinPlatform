using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services.MappedIntegration.Dtos.Responses;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.MappedIntegration;

public class GetCategoricalDataTests : BaseInMemoryTest
{
    private readonly Fixture fixture;
    public GetCategoricalDataTests(ITestOutputHelper output) : base(output)
    {
        fixture = new Fixture();

        // Remove any existing ThrowingRecursionBehavior instances
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        // Add OmitOnRecursionBehavior to handle recursive properties gracefully
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task TokenIsNotGiven_GetCategoricalData_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"api/mapped/categoricalData");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task CategoricalDataExists_GetCategoricalData_ReturnCategoricalData()
    {
        var existingStatus = fixture.Build<TicketStatusEntity>().CreateMany().ToList();
        var existingJobTypes = fixture.Build<JobTypeEntity>().CreateMany().ToList();
        var existingRequestTypes = fixture.Build<TicketCategoryEntity>()
                                          .Without(x=>x.SiteId)
                                          .Without(x=>x.Tickets)
                                          .CreateMany().ToList();
        var existingServiceNeeded = fixture.Build<ServiceNeededEntity>().CreateMany().ToList();
        var existingServiceNeededSpaceTwin = new List<ServiceNeededSpaceTwinEntity>();
        foreach (var item in existingServiceNeeded)
        {
            var serviceNeededSpaceTwin = fixture.Build<ServiceNeededSpaceTwinEntity>()
                                                 .With(x => x.ServiceNeededId, item.Id)
                                                 .With(x => x.SpaceTwinId, "TwinId")
                                                 .Without(x=>x.ServiceNeeded)
                                                 .Create();
            existingServiceNeededSpaceTwin.Add(serviceNeededSpaceTwin);
        }

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {

            var db = server.Assert().GetDbContext<WorkflowContext>();
         
            db.TicketStatuses.AddRange(existingStatus);
            db.JobTypes.AddRange(existingJobTypes);
            db.ServiceNeeded.AddRange(existingServiceNeeded);
            db.ServiceNeededSpaceTwin.AddRange(existingServiceNeededSpaceTwin);
            db.TicketCategories.AddRange(existingRequestTypes);
           
            db.SaveChanges();

            var result = await client.GetAsync($"api/mapped/categoricalData");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<TicketCategoricalDataResponse>();
            response.Should().NotBeNull();
            response.RequestTypes.Should().BeEquivalentTo(existingRequestTypes.Select(x => x.Name).ToList());
            response.JobTypes.Should().BeEquivalentTo(existingJobTypes.Select(x => new {x.Id,  x.Name }).ToList());
            response.ServicesNeeded.FirstOrDefault().Should().BeEquivalentTo(new { SpaceTwinId = "TwinId", ServiceNeededList = existingServiceNeeded.Select(x => new { x.Id, x.Name }) });
            response.AssigneeTypes.Should().BeEquivalentTo(Enum.GetNames<AssigneeType>());

        }
    }
    /// <summary>
    /// if there is no space twin - service needed mapping
    /// retun list of service needed with space twin = string.Empty
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task NoServiceNeededSpaceTwinMapping_GetCategoricalData_ReturnCategoricalData()
    {
        var existingStatus = fixture.Build<TicketStatusEntity>().CreateMany().ToList();
        var existingJobTypes = fixture.Build<JobTypeEntity>().CreateMany().ToList();
        var existingRequestTypes = fixture.Build<TicketCategoryEntity>()
                                          .Without(x => x.SiteId)
                                          .Without(x => x.Tickets)
                                          .CreateMany().ToList();
        var existingServiceNeeded = fixture.Build<ServiceNeededEntity>().CreateMany().ToList();
      

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {

            var db = server.Assert().GetDbContext<WorkflowContext>();

            db.TicketStatuses.AddRange(existingStatus);
            db.JobTypes.AddRange(existingJobTypes);
            db.ServiceNeeded.AddRange(existingServiceNeeded);
            db.TicketCategories.AddRange(existingRequestTypes);

            db.SaveChanges();

            var result = await client.GetAsync($"api/mapped/categoricalData");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadFromJsonAsync<TicketCategoricalDataResponse>();
            response.Should().NotBeNull();
            response.RequestTypes.Should().BeEquivalentTo(existingRequestTypes.Select(x => x.Name).ToList());
            response.JobTypes.Should().BeEquivalentTo(existingJobTypes.Select(x => new { x.Id, x.Name }).ToList());
            response.ServicesNeeded.SingleOrDefault().Should().BeEquivalentTo(new { SpaceTwinId = string.Empty, ServiceNeededList = existingServiceNeeded.Select(x => new { x.Id, x.Name }) });
            response.AssigneeTypes.Should().BeEquivalentTo(Enum.GetNames<AssigneeType>());

        }
    }
}

