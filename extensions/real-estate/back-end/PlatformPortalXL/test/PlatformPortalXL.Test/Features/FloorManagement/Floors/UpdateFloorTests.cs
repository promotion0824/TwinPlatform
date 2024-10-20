using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using Willow.Api.DataValidation;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.FloorManagement.Floors
{
    public class UpdateFloorTests : BaseInMemoryTest
    {
        public UpdateFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, null)]
        [InlineData(false, null)]
        public async Task FloorExists_UpdateFloor_UpdateSiteWide_ReturnsUpdatedFloor(bool isSiteWide, bool? newSiteWide)
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var modelReference = "d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b";
            var updateRequest = Fixture.Build<UpdateFloorRequest>()
                                        .Without(x => x.Code)
                                        .With(x => x.ModelReference, modelReference)
                                        .With(x => x.IsSiteWide, newSiteWide)
                                        .Create();
            var expectedFloor = Fixture.Build<Floor>()
                .With(x => x.Id, floorId)
                .With(x => x.Name, updateRequest.Name)
                .With(x => x.IsSiteWide, newSiteWide.HasValue ? newSiteWide.Value : isSiteWide)
                .With(x => x.ModelReference,Guid.Parse(updateRequest.ModelReference))
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/floors/{floorId}", updateRequest)
                    .ReturnsJson(expectedFloor);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Theory]
        [InlineData("d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b", "bbbb5e46-686a-4ff3-94ef-cd9cf1cfffff")]
        [InlineData("d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b", "")]
        [InlineData("d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b", null)]
        public async Task FloorExists_UpdateFloor_UpdateModelReference_ReturnsUpdatedFloor(string currentModelReference, string newModelReference)
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            Guid? expectedModelReference =  newModelReference==""?Guid.Empty:( newModelReference==null? Guid.Parse(currentModelReference):Guid.Parse(newModelReference));
                
            var updateRequest = Fixture.Build<UpdateFloorRequest>()
                .Without(x => x.Code)
                .With(x => x.ModelReference, newModelReference)
                .With(x => x.IsSiteWide, true)
                .Create();
            var expectedFloor = Fixture.Build<Floor>()
                .With(x => x.Id, floorId)
                .With(x => x.Name, updateRequest.Name)
                .With(x => x.IsSiteWide, true)
                .With(x => x.ModelReference, expectedModelReference)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/floors/{floorId}", updateRequest)
                    .ReturnsJson(expectedFloor);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>();
                result.Should().BeEquivalentTo(FloorDetailDto.MapFrom(expectedFloor));
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateFloor_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{Guid.NewGuid()}", new UpdateFloorRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidModelReference_UpdateFloor_ReturnsValidationError()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var updateRequest = Fixture.Build<UpdateFloorRequest>()
                .Without(x=>x.Code)
                .With(x => x.ModelReference, "invalidGuid")
                .With(x => x.IsSiteWide, true)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
               

                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items.First().Message.Should().Be("Model Reference is not valid");
            }

        }

        [Fact]
        public async Task InvalidCode_UpdateFloor_ReturnsValidationError()
        {
            var siteId = Guid.NewGuid();
            var floorId = Guid.NewGuid();
            var updateRequest = Fixture.Build<UpdateFloorRequest>()
                .Without(x => x.ModelReference)
                .With(x => x.Code, "asdfghjkloi")
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {


                var response = await client.PutAsJsonAsync($"sites/{siteId}/floors/{floorId}", updateRequest);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items.First().Name.Should().Be("Code");
            }

        }

    }
}
