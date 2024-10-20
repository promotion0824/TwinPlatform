using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.MappedIntegration.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets;

public class GetTicketPossibleAssigneeTests : BaseInMemoryTest
{
    public GetTicketPossibleAssigneeTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async Task TokenIsNotGiven_GetTicketPossibleAssignee_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"sites/{Guid.NewGuid()}/possibleTicketAssignees");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketPossibleAssigneeExists_GetTicketPossibleAssignee_ReturnTicketPossibleAssignee()
    {
        var siteId = Guid.NewGuid();
        var workgroups = Fixture.Build<WorkgroupEntity>()
                                .With(x => x.SiteId, siteId)
                                .CreateMany(3)
                                .ToList();

        var workgroupMembers = Fixture.Build<WorkgroupMemberEntity>()
                                .With(x => x.WorkgroupId, workgroups.First().Id)
                                .CreateMany(3)
                                .ToList();

        var otherSitesWorkgroups = Fixture.Build<WorkgroupEntity>()
                                .With(x => x.SiteId, Guid.NewGuid())
                                .CreateMany(3)
                                .ToList();

        var externalProfiles = Fixture.Build<ExternalProfileEntity>()
                                .CreateMany(3)
                                .ToList();

        var expectedTwinId = "TestTwinId";
        var expectedTwinName = "TestTwinName";
        var expectedTwinIds = new List<TwinIdDto>()
        {
            Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, siteId.ToString())
                    .With(x => x.Id, expectedTwinId)
                    .With(x => x.Name, expectedTwinName)
                    .Create(),
        };

        var expectedWorkgroup = WorkgroupDto.MapFromModels(WorkgroupEntity.MapToModels(workgroups)).Select(x => new WorkgroupDto
        {
            Id = x.Id,
            Name = $"{expectedTwinName} - { x.Name }",
            SiteId = x.SiteId,
            MemberIds = workgroupMembers.Where(m => m.WorkgroupId == x.Id).Select(x => x.MemberId).ToList()
        }).ToList();

        var expectedResult = new TicketAssigneeData
        {
            Workgroups = expectedWorkgroup,
            ExternalUserProfiles = MappedUserProfile.MapFrom(externalProfiles)
        };

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.Workgroups.AddRange(workgroups);
            db.Workgroups.AddRange(otherSitesWorkgroups);
            db.ExternalProfiles.AddRange(externalProfiles);
            db.WorkgroupMembers.AddRange(workgroupMembers);
            db.SaveChanges();

            server.Arrange().GetDigitalTwinApi()
                   .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={siteId}")
                   .ReturnsJson(expectedTwinIds);

            var result = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadAsAsync<TicketAssigneeData>();
            response.Should().BeEquivalentTo(expectedResult);
        }
    }

    [Fact]
    public async Task TicketPossibleAssigneeExists_GetTicketPossibleAssignee_ReturnTicketPossibleAssigneeWithoutMappedUser()
    {
        var siteId = Guid.NewGuid();
        var workgroups = Fixture.Build<WorkgroupEntity>()
                                .With(x => x.SiteId, siteId)
                                .CreateMany(3)
                                .ToList();

        var workgroupMembers = Fixture.Build<WorkgroupMemberEntity>()
                                .With(x => x.WorkgroupId, workgroups.First().Id)
                                .CreateMany(3)
                                .ToList();

        var otherSitesWorkgroups = Fixture.Build<WorkgroupEntity>()
                                .With(x => x.SiteId, Guid.NewGuid())
                                .CreateMany(3)
                                .ToList();

        var externalProfiles = Fixture.Build<ExternalProfileEntity>()
                                .CreateMany(3)
                                .ToList();

        var mappedConnectorUserProfile = Fixture.Build<ExternalProfileEntity>()
                                            .With(x => x.Name, ExternalProfileService.MappedConnectorUserProfile)
                                            .Create();


        var expectedTwinId = "TestTwinId";
        var expectedTwinName = "TestTwinName";
        var expectedTwinIds = new List<TwinIdDto>()
        {
            Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, siteId.ToString())
                    .With(x => x.Id, expectedTwinId)
                    .With(x => x.Name, expectedTwinName)
                    .Create(),
        };

        var expectedWorkgroup = WorkgroupDto.MapFromModels(WorkgroupEntity.MapToModels(workgroups)).Select(x => new WorkgroupDto
        {
            Id = x.Id,
            Name = $"{expectedTwinName} - {x.Name}",
            SiteId = x.SiteId,
            MemberIds = workgroupMembers.Where(m => m.WorkgroupId == x.Id).Select(x => x.MemberId).ToList()
        }).ToList();

        var expectedResult = new TicketAssigneeData
        {
            Workgroups = expectedWorkgroup,
            ExternalUserProfiles = MappedUserProfile.MapFrom(externalProfiles)
        };

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.Workgroups.AddRange(workgroups);
            db.Workgroups.AddRange(otherSitesWorkgroups);
            db.ExternalProfiles.AddRange(externalProfiles);
            db.ExternalProfiles.Add(mappedConnectorUserProfile);
            db.WorkgroupMembers.AddRange(workgroupMembers);
            db.SaveChanges();

            server.Arrange().GetDigitalTwinApi()
                   .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={siteId}")
                   .ReturnsJson(expectedTwinIds);

            var result = await client.GetAsync($"sites/{siteId}/possibleTicketAssignees");
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var response = await result.Content.ReadAsAsync<TicketAssigneeData>();
            response.Should().BeEquivalentTo(expectedResult);
        }
    }
}

