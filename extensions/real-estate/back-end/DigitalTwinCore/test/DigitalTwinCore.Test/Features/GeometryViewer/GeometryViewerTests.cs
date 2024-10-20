using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Features.GeometryViewer;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Features.SiteAdmin
{
    public class GeometryViewerTests : BaseInMemoryTest
    {
        private GeometryViewerModelEntity _defaultGeometryViewerModelEntity;

        public GeometryViewerTests(ITestOutputHelper output) : base(output)
        {
            _defaultGeometryViewerModelEntity = new GeometryViewerModelEntity()
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUwwMS1CQmFfMjAyMTA5MDMwMzA1NTkubndk",
                References = new List<GeometryViewerReferenceEntity>()
                {
                    new GeometryViewerReferenceEntity() { GeometryViewerId = "0/1/" }
                }
            };
        }

        [Theory]
        [AutoData]
        public async Task HasModel_AddModel_ReturnsPreconditionFailed(GeometryViewerModel request)
        {
            request.Urn = _defaultGeometryViewerModelEntity.Urn;

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            context.GeometryViewerModels.Add(new GeometryViewerModelEntity
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = request.Urn
            });
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("admin/geometryviewer", request);

            response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        }
        
        [Theory]
        [AutoData]
        public async Task NoModel_AddModel_ReturnsCreated(GeometryViewerModel request)
        {
            request.Urn = _defaultGeometryViewerModelEntity.Urn;

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            
            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("admin/geometryviewer", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            Assert.True(context.GeometryViewerModels.Any(x => x.Urn == request.Urn));
            Assert.True(context.GeometryViewerReferences.Count() == request.References.Count);
            GeometryViewerReference.MapFrom(context.GeometryViewerReferences.ToList()).Should().BeEquivalentTo(request.References);
        }

        [Fact]
        public async Task HasModel_RemoveModel_ReturnsNoContent()
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            context.GeometryViewerModels.Add(_defaultGeometryViewerModelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.DeleteAsync($"admin/geometryviewer/{_defaultGeometryViewerModelEntity.Urn}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.False(context.GeometryViewerModels.Any());
            Assert.False(context.GeometryViewerReferences.Any());
        }

        [Fact]
        public async Task NoModel_RemoveModel_ReturnsNoContent()
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            context.GeometryViewerModels.Add(_defaultGeometryViewerModelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.DeleteAsync($"admin/geometryviewer/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.True(context.GeometryViewerModels.Any());
            Assert.True(context.GeometryViewerReferences.Any());
        }

        [Fact]
        public async Task HasModel_ExistsModel_ReturnsNoContent()
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            context.GeometryViewerModels.Add(_defaultGeometryViewerModelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/{_defaultGeometryViewerModelEntity.Urn}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task NoModel_ExistsModel_ReturnsNotFound()
        {
            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            context.GeometryViewerModels.Add(_defaultGeometryViewerModelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            Assert.True(context.GeometryViewerModels.Any());
            Assert.True(context.GeometryViewerReferences.Any());
        }

        [Fact]
        public async Task HasModel_GetModelByUrn_ReturnsOK()
        {
            var urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUwwMS1CQmFfMjAyMTA5MDMwMzA1NTkubndk";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            var modelEntity = new GeometryViewerModelEntity
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = urn, 
                References = new List<GeometryViewerReferenceEntity>()
                {
                    new GeometryViewerReferenceEntity() { GeometryViewerId = "0/1/" }
                }
            };
            context.GeometryViewerModels.Add(modelEntity);
            await context.SaveChangesAsync();  

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/urns/{urn}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<GeometryViewerModel>>();
            result.First().Should().BeEquivalentTo(GeometryViewerModel.MapFrom(modelEntity));
        }

        [Fact]
        public async Task HasModel_GetModelByTwinId_ReturnsOK()
        {
            var urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUwwMS1CQmFfMjAyMTA5MDMwMzA1NTkubndk";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            var modelEntity = new GeometryViewerModelEntity
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = urn,
                References = new List<GeometryViewerReferenceEntity>()
                {
                    new GeometryViewerReferenceEntity() { GeometryViewerId = "0/1/" }
                }
            };
            context.GeometryViewerModels.Add(modelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/twins/{modelEntity.TwinId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<List<GeometryViewerModel>>();
            result.First().Should().BeEquivalentTo(GeometryViewerModel.MapFrom(modelEntity));
        }

        [Fact]
        public async Task NoModel_GetModelByUrn_ReturnsNotFound()
        {
            var urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUwwMS1CQmFfMjAyMTA5MDMwMzA1NTkubndk";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            var modelEntity = new GeometryViewerModelEntity
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = urn,
                References = new List<GeometryViewerReferenceEntity>()
                {
                    new GeometryViewerReferenceEntity() { GeometryViewerId = "0/1/" }
                }
            };
            context.GeometryViewerModels.Add(modelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/urn/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task NoModel_GetModelByTwinId_ReturnsNotFound()
        {
            var urn = "dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUwwMS1CQmFfMjAyMTA5MDMwMzA1NTkubndk";

            using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);

            var context = server.Arrange().CreateDbContext<DigitalTwinDbContext>();
            var modelEntity = new GeometryViewerModelEntity
            {
                Id = Guid.NewGuid(),
                TwinId = "INV-L3",
                Urn = urn,
                References = new List<GeometryViewerReferenceEntity>()
                {
                    new GeometryViewerReferenceEntity() { GeometryViewerId = "0/1/" }
                }
            };
            context.GeometryViewerModels.Add(modelEntity);
            await context.SaveChangesAsync();

            using var client = server.CreateClient(null);
            var response = await client.GetAsync($"admin/geometryviewer/twins/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
