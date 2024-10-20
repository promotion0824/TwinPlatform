using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Inspections;

public class CreateInspectionZoneTests : BaseInMemoryTest
{
    public CreateInspectionZoneTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserDoesNotHaveCorrectPermission_CreateInspectionZone_ReturnsForbidden()
    {
        var siteId = Guid.NewGuid();
        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId);
        var response = await client.PostAsJsonAsync($"sites/{siteId}/inspectionZones", new CreateInspectionZoneRequest { Name = "bob" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NameIsEmpty_CreateInspectionZone_ReturnsError()
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<CreateInspectionZoneRequest>()
            .Without(x => x.Name)
            .Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId);
        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
            .ReturnsJson(Array.Empty<InspectionZone>());

        var response = await client.PostAsJsonAsync($"sites/{siteId}/inspectionZones", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadAsAsync<ValidationError>();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be(nameof(request.Name));
        result.Items[0].Message.Should().Contain("Name is required");
    }

    [Fact]
    public async Task DuplicateName_CreateInspectionZone_ReturnsError()
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Create<CreateInspectionZoneRequest>();
        var duplicateNameZone = Fixture.Build<InspectionZone>().With(x => x.Name, request.Name).Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId);
        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
            .ReturnsJson(new[] { duplicateNameZone });

        var response = await client.PostAsJsonAsync($"sites/{siteId}/inspectionZones", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadAsAsync<ValidationError>();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be(nameof(request.Name));
        result.Items[0].Message.Should().Contain("Duplicate");
    }

    [Fact]
    public async Task NameTooLong_CreateInspectionZone_ReturnsError()
    {
        var siteId = Guid.NewGuid();
        var request = Fixture.Build<CreateInspectionZoneRequest>()
            .With(x => x.Name, new string('n', 201))
            .Create();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId);
        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"sites/{siteId}/zones?includeStatistics=False")
            .ReturnsJson(Array.Empty<InspectionZone>());

        var response = await client.PostAsJsonAsync($"sites/{siteId}/inspectionZones", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var result = await response.Content.ReadAsAsync<ValidationError>();
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be(nameof(request.Name));
        result.Items[0].Message.Should().Contain("length");
    }

    [Fact]
    public async Task SiteDoesNotExist_CreateInspectionZone_ReturnsNotFound()
    {
        var site = Fixture.Create<Site>();
        var request = Fixture.Create<CreateInspectionZoneRequest>();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id);
        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/zones?includeStatistics=False")
            .ReturnsJson(Array.Empty<InspectionZone>());
        server.Arrange().GetSiteApi()
            .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
            .ReturnsJson((Site)null);

        var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspectionZones", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidInput_CreateInspectionZone_ReturnsCreatedInspectionZone()
    {
        var site = Fixture.Create<Site>();
        var request = Fixture.Create<CreateInspectionZoneRequest>();
        var createdZone = Fixture.Create<InspectionZone>();

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id);
        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/zones?includeStatistics=False")
            .ReturnsJson(Array.Empty<InspectionZone>());
        server.Arrange().GetSiteApi()
            .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
            .ReturnsJson(site);

        server.Arrange().GetWorkflowApi()
            .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/zones", request)
            .ReturnsJson(createdZone);

        var response = await client.PostAsJsonAsync($"sites/{site.Id}/inspectionZones", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<InspectionZoneDto>();
        result.Should().BeEquivalentTo(InspectionZoneDto.MapFromModel(createdZone));
    }
}
