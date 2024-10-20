using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
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
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Site = MobileXL.Models.Site;

namespace MobileXL.Test.Features.Zones
{
    public class SubmitCheckRecordTests : BaseInMemoryTest
    {
        public SubmitCheckRecordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CheckRecordExists_SubmitCheckRecord_ByCustomerUser_ReturnsThisInspectionRecord()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();
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
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}")
                    .ReturnsJson(new WorkflowSubmitCheckRecordResponse());
				server.Arrange().GetDirectoryApi()
					.SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/users/{userId}")
					.ReturnsJson(customerUser);
				server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}", new WorkflowSubmitCheckRecordRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task GivenCheckRecordWillTriggerInsight_SubmitCheckRecord_ByCustomerUser_InsightIsGenerated()
        {
            var utcNow = DateTime.UtcNow;
            var customer = Fixture.Create<Customer>();
            var site = Fixture.Create<Site>();
            var inspectionId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var submitCheckRecordResponse = Fixture.Create<WorkflowSubmitCheckRecordResponse>();
            var expectedAsset = Fixture.Build<DigitalTwinAsset>()
                                       .Without(e => e.PointTags)
                                       .With(e => e.TwinId, submitCheckRecordResponse.RequiredInsight.TwinId)
                                       .Create();
            var customerUser = Fixture.Build<CustomerUser>()
	            .With(x => x.Id, userId)
	            .Create();
			var expectedCreateInsightRequest = new CreateInsightCoreRequest
            {
                CustomerId = site.CustomerId,
                SequenceNumberPrefix = site.Code,
                TwinId = submitCheckRecordResponse.RequiredInsight.TwinId,
                Type = submitCheckRecordResponse.RequiredInsight.Type,
                Name = submitCheckRecordResponse.RequiredInsight.Name,
                Description = submitCheckRecordResponse.RequiredInsight.Description + $"\r\nAsset: {expectedAsset.Name}",
                Priority = submitCheckRecordResponse.RequiredInsight.Priority,
                State = InsightState.Active,
                OccurredDate = utcNow,
                DetectedDate = utcNow,
                SourceType = InsightSourceType.Inspection,
                SourceId = null,
                ExternalId = string.Empty,
                ExternalStatus = string.Empty,
                ExternalMetadata = string.Empty,
                OccurrenceCount = 1,
                AnalyticsProperties = new Dictionary<string, string>()
                {
                    { "Site", site.Name },
                    { "Company", customer.Name }
                },
				CreatedUserId = userId
            };
            var createInsightResponse = Fixture.Create<Insight>();
            var expectedUpdateCheckRecordInsightRequest = new WorkflowApiService.UpdateCheckRecordInsightRequest
            {
                InsightId = createInsightResponse.Id
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, site.Id))
            {
                server.Arrange().SetCurrentDateTime(utcNow);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}")
                    .ReturnsJson(submitCheckRecordResponse);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}")
                    .ReturnsJson(customer);
                server.Arrange().GetDirectoryApi()
	                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/users/{userId}")
	                .ReturnsJson(customerUser);
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/assets/twinId/{expectedAsset.TwinId}")
                    .ReturnsJson(expectedAsset);
                server.Arrange().GetInsightApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Post, $"sites/{site.Id}/insights", expectedCreateInsightRequest)
                    .ReturnsJson(createInsightResponse);
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedBody(HttpMethod.Put, $"sites/{site.Id}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}/insight", expectedUpdateCheckRecordInsightRequest)
                    .ReturnsJson(submitCheckRecordResponse);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}", new SubmitCheckRecordRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_SubmitCheckRecord_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}", new WorkflowSubmitCheckRecordRequest());

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task CheckRecordExistsButOverdued_ByCustomerUser_SubmitCheckRecord_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var inspectionId = Guid.NewGuid();
            var checkRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var site = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .Create();

            var expectedResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = $"Resource(checkRecord: {checkRecordId}) cannot be found.",
                Data = new[] { new { ResourceType = "checkRecord", ResourceId = checkRecordId } }
            };
            var customerUser = Fixture.Build<CustomerUser>()
	            .With(x => x.Id, userId)
	            .Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}")
                    .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                    { Content = new StringContent(JsonSerializer.Serialize(expectedResponse), Encoding.UTF8, "application/problem+json") }));
                server.Arrange().GetDirectoryApi()
	                .SetupRequest(HttpMethod.Get, $"customers/{site.CustomerId}/users/{userId}")
	                .ReturnsJson(customerUser);
				server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync($"sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}", new WorkflowSubmitCheckRecordRequest());

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("An error has occurred. Please refresh.");
            }
        }
	}
}
