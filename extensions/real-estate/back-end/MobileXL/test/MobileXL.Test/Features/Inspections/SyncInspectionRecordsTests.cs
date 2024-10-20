using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Features.Inspections;
using MobileXL.Features.Inspections.Requests;
using MobileXL.Models;
using MobileXL.Services.Apis.DigitalTwinApi;
using MobileXL.Services.Apis.InsightApi;
using MobileXL.Services.Apis.WorkflowApi;
using MobileXL.Services.Apis.WorkflowApi.Requests;
using MobileXL.Services.Apis.WorkflowApi.Responses;
using Moq.Contrib.HttpClient;
using Willow.Api.Client;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Zones
{
    public class SyncInspectionRecordsTests : BaseInMemoryTest
    {
        public SyncInspectionRecordsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SyncInspectionRecords_ByCustomerUser_ReturnsSuccess()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var inspectionRecordId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var inspectionRecordsRequest = new InspectionRecordsRequest()
            {
                InspectionRecords = new List<InspectionRecordRequest>() { new InspectionRecordRequest()
                { Id = inspectionRecordId, InspectionId = inspectionId, CheckRecords = new List<CheckRecordRequest>()
                { new CheckRecordRequest() { Id = checkRecordId } } } }
            };
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            var customerUser = Fixture.Build<CustomerUser>()
	            .With(x => x.Id, userId)
	            .Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/inspections/{inspectionId}/{inspectionRecordId}/checkRecords/{checkRecordId}")
                    .ReturnsJson(new WorkflowSubmitCheckRecordResponse());
                server.Arrange().GetDirectoryApi()
	                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/users/{userId}")
	                .ReturnsJson(customerUser);
				server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/syncinspectionrecords", inspectionRecordsRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task SyncInspectionRecords_ByCustomerUser_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var inspectionRecordId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var inspectionRecordsRequest = new InspectionRecordsRequest()
            {
                InspectionRecords = new List<InspectionRecordRequest>() { new InspectionRecordRequest()
                { Id = inspectionRecordId, InspectionId = inspectionId, CheckRecords = new List<CheckRecordRequest>()
                { new CheckRecordRequest() { Id = checkRecordId } } } }
            };
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();
            var customerUser = Fixture.Build<CustomerUser>()
	            .With(x => x.Id, userId)
	            .Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/inspections/{inspectionId}/{inspectionRecordId}/checkRecords/{checkRecordId}")
                    .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
	                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/users/{userId}")
	                .ReturnsJson(customerUser);
	            var response = await client.PostAsJsonAsync($"sites/{siteId}/syncinspectionrecords", inspectionRecordsRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
	}
}
