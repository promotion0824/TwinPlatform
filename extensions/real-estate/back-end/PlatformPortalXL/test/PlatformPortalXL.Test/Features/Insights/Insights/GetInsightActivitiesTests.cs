using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Insights.Insights;
public class GetInsightActivitiesTests : BaseInMemoryTest
{
    private static Guid guid = Guid.Parse("4dadee63-74f6-4df1-bfb4-eee75dddfa13");

    public GetInsightActivitiesTests(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
	public async Task UnauthorizedUser_GetInsightActivities_ReturnUnauthorized()
	{
		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClient())
		{
			var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/activities");
			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

		}
	}

	[Fact]
	public async Task UserDoesNotHaveCorrectPermission_GetInsightActivities_ReturnsForbidden()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
		using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
		{
			var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/activities");
			response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}
	}

	[Fact]
	public async Task InsightActivitiesExist_GetInsightActivities_ReturnInsightActivities()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightTicketActivity = Fixture.Build<InsightTicketActivity>().CreateMany().ToList();
		var expectedInsightActivities = Fixture.Build<InsightActivity>()
                                               .CreateMany(3).ToList();

        // first activity always has PreviouslyIgnored & PreviouslyResolved = false
        expectedInsightActivities[0].StatusLog.Status = InsightStatus.New;
        expectedInsightActivities[0].StatusLog.CreatedDateTime = new DateTime(2023, 7, 1, 1, 1, 1, DateTimeKind.Utc);
        expectedInsightActivities[0].StatusLog.PreviouslyIgnored = false;
        expectedInsightActivities[0].StatusLog.PreviouslyResolved = false;


        expectedInsightActivities[1].StatusLog.Status = InsightStatus.Open;
        expectedInsightActivities[1].StatusLog.CreatedDateTime = new DateTime(2023, 7, 2, 1, 1, 1, DateTimeKind.Utc);

        expectedInsightActivities[2].StatusLog.Status = InsightStatus.Ignored;
        expectedInsightActivities[2].StatusLog.CreatedDateTime = new DateTime(2023, 7, 3, 1, 1, 1, DateTimeKind.Utc);


        // set the insight occurrence start and end date
        foreach (var activity in expectedInsightActivities)
        {
            activity.InsightOccurrence.Started = new DateTime(2023, 8, 20, 1, 1, 1, DateTimeKind.Utc);
            activity.InsightOccurrence.Ended = new DateTime(2023, 8, 21, 1, 1, 1, DateTimeKind.Utc);
        }


        var expectedResult = InsightActivityDto.MapFromInsightTicketActivities(expectedInsightTicketActivity);
		expectedResult.AddRange(InsightActivityDto.MapFromInsightActivities(expectedInsightActivities));
		expectedResult = expectedResult.OrderBy(x => x.ActivityDate).ToList();

		var expectedUsers = new List<FullNameDto>();
		var expectedApps = new List<App>();
		foreach (var activity  in expectedResult)
		{
			if (activity.UserId.HasValue)
			{
				var user = Fixture.Build<FullNameDto>().With(x => x.UserId, activity.UserId).Create();
				expectedUsers.Add(user);
				activity.FullName = $"{user.FirstName} {user.LastName}";
			}
			if (activity.SourceId.HasValue)
			{
				var app = Fixture.Build<App>().With(x => x.Id, activity.SourceId).Create();
				expectedApps.Add(app);
			}
		}
		
		using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
		using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

		server.Arrange().GetWorkflowApi()
			.SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/activities")
			.ReturnsJson(expectedInsightTicketActivity);

		server.Arrange().GetInsightApi()
			.SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}/activities")
			.ReturnsJson(expectedInsightActivities);

		server.Arrange().GetMarketPlaceApi()
			.SetupRequest(HttpMethod.Get, $"apps")
			.ReturnsJson(expectedApps);

		server.Arrange().GetDirectoryApi()
			.SetupRequest(HttpMethod.Post, $"users/fullNames")
			.ReturnsJson(expectedUsers);

		var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/activities");
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var result = await response.Content.ReadAsAsync<List<InsightActivityDto>>();
		result.Should().BeInAscendingOrder(x => x.ActivityDate);
		result.Should()
			.BeEquivalentTo(expectedResult, config =>
											config.Using<string>(ctx => ctx.Subject.Should().BeEquivalentTo(ctx.Expectation)).WhenTypeIs<string>());

	}


	[Fact]
	public async Task InsightNotExist_GetInsightActivities_ReturnNotFound()
	{
		var siteId = Guid.NewGuid();
		var insightId = Guid.NewGuid();
		var expectedInsightTicketActivity = Fixture.Build<InsightTicketActivity>().CreateMany().ToList();
		var expectedInsightActivities = Fixture.Build<InsightActivity>().CreateMany().ToList();

		var expectedResult = InsightActivityDto.MapFromInsightTicketActivities(expectedInsightTicketActivity);
		expectedResult.AddRange(InsightActivityDto.MapFromInsightActivities(expectedInsightActivities));
		expectedResult = expectedResult.OrderBy(x => x.ActivityDate).ToList();

		using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
		using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

		server.Arrange().GetWorkflowApi()
			.SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/activities")
			.ReturnsJson(expectedInsightTicketActivity);

		server.Arrange().GetInsightApi()
			.SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}/activities")
			.ReturnsResponse(HttpStatusCode.NotFound);

		var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/activities");
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		
	}

    /// <summary>
    /// Only show the activities with New status if:
    ///  it's first time the insight is created OR
    ///  if the insight has been resolved or ignored
    ///  example: New => Resolved => New => Open => New => Ignored => New => New
    ///  should be filtered to New => Resolved => New => Open => Ignored => New 
    /// </summary>
    /// <param name="insightActivities"></param>
    [Theory]
    [MemberData(nameof(TestData))]
    public async Task InsightActivitiesWithRedundantNewStatus_GetInsightActivities_ReturnInsightFilteredActivities(StatusLogTestData[] testData)
    {
        var siteId = Guid.NewGuid();
        var insightId = Guid.NewGuid();
       
        var expectedInsightTicketActivity = Fixture.Build<InsightTicketActivity>().CreateMany().ToList();
        var expectedInsightActivitiesResponse = Fixture.Build<InsightActivity>()
                                               .CreateMany(7).ToList();

        for(var i=0; i< testData.Length; i++)
        {
            expectedInsightActivitiesResponse[i].StatusLog.Status = testData[i].status;
            expectedInsightActivitiesResponse[i].StatusLog.CreatedDateTime = new DateTime(2023, 7, i+1, 1, 1, 1, DateTimeKind.Utc);
            expectedInsightActivitiesResponse[i].StatusLog.Id = testData[i].statusId;
        }

        // set the insight occurrence start and end date
        foreach (var activity in expectedInsightActivitiesResponse)
        {
            activity.InsightOccurrence.Started = new DateTime(2023, 8, 20, 1, 1, 1, DateTimeKind.Utc);
            activity.InsightOccurrence.Ended = new DateTime(2023, 8, 21, 1, 1, 1, DateTimeKind.Utc);
        }


        // filter out the activity with id = toBeRemovedActivityId
        var expectedInsightActivities = expectedInsightActivitiesResponse.Where(x => x.StatusLog.Id != guid).ToList();

        var expectedResult = InsightActivityDto.MapFromInsightTicketActivities(expectedInsightTicketActivity);
        expectedResult.AddRange(InsightActivityDto.MapFromInsightActivities(expectedInsightActivities));
        expectedResult = expectedResult.OrderBy(x => x.ActivityDate).ToList();

        var expectedUsers = new List<FullNameDto>();
        var expectedApps = new List<App>();
        foreach (var activity in expectedResult)
        {
            if (activity.UserId.HasValue)
            {
                var user = Fixture.Build<FullNameDto>().With(x => x.UserId, activity.UserId).Create();
                expectedUsers.Add(user);
                activity.FullName = $"{user.FirstName} {user.LastName}";
            }
        }

        using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
        using var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId);

        server.Arrange().GetWorkflowApi()
            .SetupRequest(HttpMethod.Get, $"insights/{insightId}/tickets/activities")
            .ReturnsJson(expectedInsightTicketActivity);

        server.Arrange().GetInsightApi()
            .SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{insightId}/activities")
            .ReturnsJson(expectedInsightActivitiesResponse);

        server.Arrange().GetMarketPlaceApi()
            .SetupRequest(HttpMethod.Get, $"apps")
            .ReturnsJson(expectedApps);

        server.Arrange().GetDirectoryApi()
            .SetupRequest(HttpMethod.Post, $"users/fullNames")
            .ReturnsJson(expectedUsers);

        var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/activities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<List<InsightActivityDto>>();
        result.Should().BeInAscendingOrder(x => x.ActivityDate);
        result.Should()
            .BeEquivalentTo(expectedResult, config =>
                                            config.Using<string>(ctx => ctx.Subject.Should().BeEquivalentTo(ctx.Expectation)).WhenTypeIs<string>());
        
    }

    public static IEnumerable<object[]> TestData()
    {
        // status with guid value should be deleted in the result
        var newGuid = Guid.NewGuid();
        yield return new object[]
        {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.New, guid ) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
            },

        };

        yield return new object[]
        {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
            },

        };

        yield return new object[]
        {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
            },

        };

        yield return new object[]
        {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Resolved, newGuid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
            },

        };

        yield return new object[]
        {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.Resolved, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.Resolved, newGuid) ,
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.InProgress, newGuid) ,
            },

        };
        yield return new object[]
      {
            new List<StatusLogTestData>
            {
                new StatusLogTestData(InsightStatus.New, newGuid) ,
                new StatusLogTestData(InsightStatus.Ignored, newGuid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.New, guid) ,
                new StatusLogTestData(InsightStatus.Open, newGuid) ,
                new StatusLogTestData(InsightStatus.New, newGuid) ,
            },

      };
        yield return new object[] {new List<StatusLogTestData>() };
       
    }
    public record StatusLogTestData(InsightStatus status, Guid statusId);


}
