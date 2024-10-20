using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Twins
{
    public  class GetTwinRelationshipsTests : BaseInMemoryTest
    {
        public GetTwinRelationshipsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinRelationships_ReturnsRelationships()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twinId";
            var outgoingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(3).ToList();
            var locationRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(2).ToList();
            var incomingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Source, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Target).CreateMany(3).ToList();
            var siteTwin = Fixture.Build<TwinDto>().With(t => t.Id, twinId).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships")
                    .ReturnsJson(outgoingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships/query?relationshipNames=locatedIn&relationshipNames=isPartOf&hops=5")
                    .ReturnsJson(locationRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/incomingrelationships?excludingRelationshipNames=isCapabilityOf")
                    .ReturnsJson(incomingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/{siteId}")
                    .ReturnsJson(siteTwin);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/relationships");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinRelationshipDto>>();
                result.Should().BeEquivalentTo(outgoingRels.Concat(locationRels).Concat(incomingRels).ToList());
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTwinRelationships_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twinId";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/relationships");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinRelationships_HavingDuplicatedLocationRelationships_ReturnsUniqueRelationships()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twinId";
            var outgoingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(3).ToList();
            var locationRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(2).ToList();
            var incomingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Source, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Target).CreateMany(3).ToList();
            var siteTwin = Fixture.Build<TwinDto>().With(t => t.Id, twinId).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships")
                    .ReturnsJson(outgoingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships/query?relationshipNames=locatedIn&relationshipNames=isPartOf&hops=5")
                    .ReturnsJson(locationRels.Append(outgoingRels.First()));

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/incomingrelationships?excludingRelationshipNames=isCapabilityOf")
                    .ReturnsJson(incomingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/{siteId}")
                    .ReturnsJson(siteTwin);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/relationships");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinRelationshipDto>>();
                result.Should().BeEquivalentTo(outgoingRels.Concat(locationRels).Concat(incomingRels).ToList());
            }
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinRelationships_HavingMultipleSameTargetLocationRelationships_ReturnsUniqueRelationships()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twinId";
            var outgoingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(3).ToList();
            var locationRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(2).ToList();
            var incomingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Source, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Target).CreateMany(3).ToList();
            var siteTwin = Fixture.Build<TwinDto>().With(t => t.Id, twinId).Create();
            var sameTargetLocationRelationship = Fixture.Build<TwinRelationshipDto>().With(r => r.TargetId, locationRels.First().TargetId).With(r => r.Target, locationRels.First().Target).Without(r => r.Source).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships")
                    .ReturnsJson(outgoingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships/query?relationshipNames=locatedIn&relationshipNames=isPartOf&hops=5")
                    .ReturnsJson(locationRels.Append(sameTargetLocationRelationship));

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/incomingrelationships?excludingRelationshipNames=isCapabilityOf")
                    .ReturnsJson(incomingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/{siteId}")
                    .ReturnsJson(siteTwin);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/relationships");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinRelationshipDto>>();
                result.Should().BeEquivalentTo(outgoingRels.Concat(locationRels).Concat(incomingRels).ToList());
            }
        }

        [Fact]
        public async Task UserWithPermissions_GetTwinRelationships_HavingSiteTwinInIncomingRelationships_ReturnsUniqueRelationshipsWithOneSiteTwin()
        {
            var siteId = Guid.NewGuid();
            var twinId = "twinId";
            var outgoingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(3).ToList();
            var locationRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Target, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Source).CreateMany(2).ToList();
            var incomingRels = Fixture.Build<TwinRelationshipDto>().With(r => r.Source, Fixture.Build<TwinDto>().Without(t => t.CustomProperties).Without(t => t.Metadata).Create()).Without(r => r.Target).CreateMany(3).ToList();
            var siteTwin = Fixture.Build<TwinDto>().With(t => t.Id, incomingRels.First().SourceId).Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships")
                    .ReturnsJson(outgoingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/relationships/query?relationshipNames=locatedIn&relationshipNames=isPartOf&hops=5")
                    .ReturnsJson(locationRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/{twinId}/incomingrelationships?excludingRelationshipNames=isCapabilityOf")
                    .ReturnsJson(incomingRels);

                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/{siteId}")
                    .ReturnsJson(siteTwin);

                var response = await client.GetAsync($"sites/{siteId}/twins/{twinId}/relationships");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinRelationshipDto>>();
                result.Should().BeEquivalentTo(outgoingRels.Concat(locationRels).Concat(incomingRels).ToList());
            }
        }
    }
}
