using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Management;
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
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Management.Workgroups
{
    public class UpdateWorkgroupTests : BaseInMemoryTest
    {
        public UpdateWorkgroupTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateWorkgroup_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var workgroupId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                var response = await client.PutAsJsonAsync($"management/sites/{siteId}/workgroups/{workgroupId}", new UpdateWorkgroupRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task NameIsEmpty_UpdateWorkgroup_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<UpdateWorkgroupRequest>()
                .Without(x => x.Name)
                .Create();
            var createdWorkgroup = Fixture.Create<Workgroup>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/workgroups/{createdWorkgroup.Id}", request)
                    .ReturnsJson(createdWorkgroup);

                var response = await client.PutAsJsonAsync($"management/sites/{siteId}/workgroups/{createdWorkgroup.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateWorkgroup_ReturnsUpdatedWorkgroup()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Create<UpdateWorkgroupRequest>();
            var createdWorkgroup = Fixture.Create<Workgroup>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageUsers, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{siteId}/workgroups/{createdWorkgroup.Id}", request)
                    .ReturnsJson(createdWorkgroup);

                var response = await client.PutAsJsonAsync($"management/sites/{siteId}/workgroups/{createdWorkgroup.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<WorkgroupDetailDto>();
                result.Should().BeEquivalentTo(WorkgroupDetailDto.MapFromModel(createdWorkgroup));
            }
        }
    }
}
