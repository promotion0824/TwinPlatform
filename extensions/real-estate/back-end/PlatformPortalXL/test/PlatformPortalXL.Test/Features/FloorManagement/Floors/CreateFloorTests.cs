using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Test;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Api.DataValidation;

namespace SiteCore.Test.Controllers.Floors
{
    public class CreateFloorTests : BaseInMemoryTest
    {
        public CreateFloorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateFloor_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var code = Fixture.Create<string>().Substring(1, 10);
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                .Without(x => x.ModelReference)
                .With(x => x.Code, code)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors", createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task EmptyName_CreateFloor_ReturnsValidationError()
        {
            var siteId = Guid.NewGuid();
            var code = Fixture.Create<string>().Substring(1, 10);
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                .Without(x => x.ModelReference)
                .Without(x => x.Name)
                .With(x => x.Code, code)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PostAsJsonAsync(
                    $"sites/{siteId}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items.First().Name.Should().Be("Name");
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("sdfghjkloiu")]
        public async Task InvalidCode_CreateFloor_ReturnsValidationError(string code)
        {
            var siteId = Guid.NewGuid();
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                .Without(x => x.ModelReference)
                .With(x => x.Code, code)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PostAsJsonAsync(
                    $"sites/{siteId}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items.First().Name.Should().Be("Code");
            }
        }

        [Fact]
        public async Task InvalidModelReference_CreateFloor_ReturnsValidationError()
        {
            var siteId = Guid.NewGuid();
            var code = Fixture.Create<string>().Substring(1, 10);
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                .With(c=>c.Code,code)
                .With(x => x.ModelReference, "invalidGuid")
                .With(x => x.IsSiteWide, true)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                var response = await client.PostAsJsonAsync(
                    $"sites/{siteId}/floors",
                    createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items.First().Message.Should().Be("Model Reference is not valid");
            }
        }
        [Theory]
        [InlineData("d7cb5e46-686a-4ff3-94ef-cd9cf1cf030b")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GivenValidInput_CreateFloor_FloorIsCreated(string modelReference)
        {
            var siteId = Guid.NewGuid();
            var code = Fixture.Create<string>().Substring(1, 10);
            var createFloorRequest = Fixture.Build<CreateFloorRequest>()
                .With(x=>x.Code, code)
                                            .With(x => x.ModelReference, modelReference)
                                            .With(x => x.IsSiteWide, true)
                                            .Create();

            var expectedFloor = Fixture.Build<Floor>()
                                       .With(x => x.ModelReference,string.IsNullOrEmpty(modelReference)?Guid.Empty : Guid.Parse(modelReference))
                                       .With(x => x.IsSiteWide, true)
                                       .Create();

            var expectedRequestToSiteApi = new CreateFloorRequest
            {
                Name = createFloorRequest.Name,
                Code = createFloorRequest.Code, 
                IsSiteWide = createFloorRequest.IsSiteWide, 
                ModelReference = createFloorRequest.ModelReference
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageFloors, siteId))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{siteId}/floors", expectedRequestToSiteApi)
                    .ReturnsJson(expectedFloor);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/floors", createFloorRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<FloorDetailDto>(); 
                var expectedFloorDto = FloorDetailDto.MapFrom(expectedFloor);
                result.Should().BeEquivalentTo(expectedFloorDto);
            }
        }
    }
}
