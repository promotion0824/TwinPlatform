using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi.Responses;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.Test.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.SiteStructure.Sites
{
    public class GetSitesTests : BaseInMemoryTest
    {
        public GetSitesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(false)]
        public async Task UserCanViewSites_GetSites_ReturnsThoseSites(bool includeWeather)
        {

            var expectedCustomer = Fixture.Build<Customer>()
                .With(x => x.Features, new CustomerFeatures()
                {
                    IsConnectivityViewEnabled = true
                })
                .Create();

            var expectedSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(32)
               .ToList();

            var userAssignments = new List<UserAssignment>();
            foreach(var site in expectedSites)
            {
                userAssignments.Add(new UserAssignment(Permissions.ViewSites, site.Id));
            }
            var userDetails = Fixture.Build<GetUserDetailsResponse>()
                .With(x => x.CustomerId, expectedCustomer.Id)
                .With(x=> x.Customer, expectedCustomer)
                .With(x=>x.UserAssignments, userAssignments)
                .Create();




            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, expectedSites[0].Id)
                        .With(x => x.CustomerId, expectedCustomer.Id)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, expectedSites[0].Id)
                            .With(x => x.ClientId, expectedCustomer.Id)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();
            var expectedLastUpdated = DateTime.UtcNow;

            var expectedInsightStatsResponse = new InsightStatisticsResponse();
            var expectedTicketStatsResponse = new TicketStatisticsResponse();

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 0)
                    .Create();
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userDetails.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                var insightCoreHandler = server.Arrange().GetInsightApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userDetails.Id}/userDetails")
                    .ReturnsJson(userDetails);


                siteApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/customer/{userDetails.CustomerId}/extend")
                    .ReturnsJson(expectedSites);

                var index = 2;


                foreach (var site in expectedSites)
                {
                    // set up ticket stats response
                    var ticketStatisticsByPriority =  Fixture.Build<TicketStatisticsByPriority>().With(x => x.Id , site.Id).Create();
                    var ticketStatisticsByStatus = Fixture.Build<TicketStatisticsByStatus>().With(x => x.Id, site.Id).Create();
                    expectedTicketStatsResponse.StatisticsByPriority.Add(ticketStatisticsByPriority);
                    expectedTicketStatsResponse.StatisticsByStatus.Add(ticketStatisticsByStatus);

                    // set insight stats response
                    var siteInsightStatisticsByStatus = Fixture.Build<SiteInsightStatisticsByStatus>().With(x => x.Id, site.Id).Create();
                    var siteInsightStatistics = Fixture.Build<SiteInsightStatistics>().With(x => x.Id, site.Id).Create();

                    expectedInsightStatsResponse.StatisticsByPriority.Add(siteInsightStatistics);
                    expectedInsightStatsResponse.StatisticsByStatus.Add(siteInsightStatisticsByStatus);

                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                        .ReturnsJson(gateways);
                    index += 2;
                }

                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors=True&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                }

                var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSites, server.Assert().GetImageUrlHelper());

                index = 2;

                foreach (var site in expectedSiteDetailDtos)
                {
                    site.Features = new SiteFeaturesDto();
                    site.IsOnline = true;
                    site.InsightsStats = SiteInsightStatistics.MapTo(expectedInsightStatsResponse.StatisticsByPriority.FirstOrDefault(x => x.Id == site.Id));
                    site.InsightsStatsByStatus = SiteInsightStatisticsByStatus.MapTo(expectedInsightStatsResponse.StatisticsByStatus.FirstOrDefault(x => x.Id == site.Id));
                    site.TicketStats = TicketStatisticsByPriority.MapTo(expectedTicketStatsResponse.StatisticsByPriority.FirstOrDefault(x => x.Id == site.Id));
                    site.TicketStatsByStatus = TicketStatisticsByStatus.MapTo(expectedTicketStatsResponse.StatisticsByStatus.FirstOrDefault(x => x.Id == site.Id));


                    site.ArcGisLayers = new List<ArcGisLayerDto>();

                    if (includeWeather)
                    {
                        site.Weather = MockWeatherbitApiService.ExpectedDto;
                    }


                    index += 2;
                }
                // get ticket and insight status
                insightCoreHandler
                      .SetupRequest(HttpMethod.Post, $"insights/statistics")
                      .ReturnsJson(expectedInsightStatsResponse);
                workflowApiHandler
                    .SetupRequest(HttpMethod.Post, $"tickets/statistics")
                    .ReturnsJson(expectedTicketStatsResponse);


                // Act
                // (Note: includeStatsByStatus param is temporary)
                var parms = new Dictionary<string, string>();

                if (includeWeather)
                {
                    parms["includeWeather"] = "true";
                }
                var path = QueryHelpers.AddQueryString("/me/sites", parms);
                var response = await client.GetAsync(path);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();

                result.Should().BeEquivalentTo(expectedSiteDetailDtos);
            }
        }

        [Fact]
        public async Task InsightsStatsFail_GetSites_ReturnsThoseSitesWithInsightStats()
        {
            var expectedCustomer = Fixture.Build<Customer>()
                 .With(x => x.Features, new CustomerFeatures()
                 {
                     IsConnectivityViewEnabled = true
                 })
                 .Create();

            var expectedSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(32)
               .ToList();

            var userAssignments = new List<UserAssignment>();
            foreach (var site in expectedSites)
            {
                userAssignments.Add(new UserAssignment(Permissions.ManageFloors, site.Id));
            }
            var userDetails = Fixture.Build<GetUserDetailsResponse>()
                .With(x => x.CustomerId, expectedCustomer.Id)
                .With(x => x.Customer, expectedCustomer)
                .With(x => x.UserAssignments, userAssignments)
                .Create();


            var gateways = new List<Gateway>
                {
                    Fixture.Build<Gateway>()
                        .With(x => x.IsEnabled, true)
                        .With(x => x.IsOnline, true)
                        .With(x => x.SiteId, expectedSites[0].Id)
                        .With(x => x.CustomerId, expectedCustomer.Id)
                        .With(x => x.Connectors, Fixture.Build<Connector>()
                            .With(x => x.IsArchived, false)
                            .With(x => x.IsEnabled, true)
                            .With(x => x.SiteId, expectedSites[0].Id)
                            .With(x => x.ClientId, expectedCustomer.Id)
                            .CreateMany(2)
                            .ToList())
                        .Create()
                };

            var expectedConnectorLogRecords = new Dictionary<Guid, ConnectorLogRecord>();
            var expectedLastUpdated = DateTime.UtcNow;


            var expectedTicketStatsResponse = new TicketStatisticsResponse();

            foreach (var connector in gateways.Single().Connectors)
            {
                expectedConnectorLogRecords[connector.Id] = Fixture.Build<ConnectorLogRecord>()
                    .With(x => x.ConnectorId, connector.Id)
                    .With(x => x.StartTime, expectedLastUpdated)
                    .With(x => x.CreatedAt, expectedLastUpdated)
                    .With(x => x.EndTime, expectedLastUpdated.AddMinutes(5))
                    .With(x => x.PointCount, 1000)
                    .With(x => x.ErrorCount, 0)
                    .Create();
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userDetails.Id))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var siteApiHandler = server.Arrange().GetSiteApi();
                var connectorApiHandler = server.Arrange().GetConnectorApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                var insightCoreHandler = server.Arrange().GetInsightApi();


                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userDetails.Id}/userDetails")
                    .ReturnsJson(userDetails);

                siteApiHandler
                    .SetupRequest(HttpMethod.Get, $"sites/customer/{userDetails.CustomerId}/extend")
                    .ReturnsJson(expectedSites);

                var index = 2;


                foreach (var site in expectedSites)
                {
                    // set up ticket stats response
                    var ticketStatisticsByPriority = Fixture.Build<TicketStatisticsByPriority>().With(x => x.Id, site.Id).Create();
                    var ticketStatisticsByStatus = Fixture.Build<TicketStatisticsByStatus>().With(x => x.Id, site.Id).Create();
                    expectedTicketStatsResponse.StatisticsByPriority.Add(ticketStatisticsByPriority);
                    expectedTicketStatsResponse.StatisticsByStatus.Add(ticketStatisticsByStatus);

                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/gateways")
                        .ReturnsJson(gateways);
                    index += 2;
                }

                foreach (var connector in gateways.Single().Connectors)
                {
                    connectorApiHandler
                        .SetupRequest(HttpMethod.Get, $"connectors/{connector.Id}/logs/latest?count=1&includeErrors=True&source=Connector")
                        .ReturnsJson(new ConnectorLogRecord[] { expectedConnectorLogRecords[connector.Id] });
                }

                var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSites, server.Assert().GetImageUrlHelper());

                index = 2;

                foreach (var site in expectedSiteDetailDtos)
                {
                    site.Features = new SiteFeaturesDto();
                    site.UserRole = "admin";
                    site.IsOnline = true;
                    site.TicketStats = TicketStatisticsByPriority.MapTo(expectedTicketStatsResponse.StatisticsByPriority.FirstOrDefault(x => x.Id == site.Id));
                    site.TicketStatsByStatus = TicketStatisticsByStatus.MapTo(expectedTicketStatsResponse.StatisticsByStatus.FirstOrDefault(x => x.Id == site.Id));


                    site.ArcGisLayers = new List<ArcGisLayerDto>();
                    index += 2;
                }
                // get ticket and insight status
                insightCoreHandler
                      .SetupRequest(HttpMethod.Post, $"insights/statistics")
                      .Throws(new Exception());
                workflowApiHandler
                    .SetupRequest(HttpMethod.Post, $"tickets/statistics")
                    .ReturnsJson(expectedTicketStatsResponse);


                // Act
                // (Note: includeStatsByStatus param is temporary)
                var parms = new Dictionary<string, string>();

                var path = QueryHelpers.AddQueryString("/me/sites", parms);
                var response = await client.GetAsync(path);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();

                result.Should().BeEquivalentTo(expectedSiteDetailDtos);
            }
        }


        [Fact]
        public async Task Unauthorized_GetSites_ReturnUnauthorized()
        {
            var userDetails = Fixture.Create<GetUserDetailsResponse>();
            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient();
            var response = await client.GetAsync("/me/sites");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UserCanViewSiteWithAdminRoleOnOthers_GetSites_ReturnSites()
        {
            var expectedCustomer = Fixture.Build<Customer>()
             .With(x => x.Features, new CustomerFeatures()
             {
                 IsConnectivityViewEnabled = false
             })
             .Create();
            var expectedSitesResponse = new List<Site>();
            var expectedSitesResult = new List<Site>();
            var canViewSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(5)
               .ToList();

            var adminSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(7)
               .ToList();


            var otherInaccessibleSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(30)
               .ToList();

            expectedSitesResponse.AddRange(canViewSites);
            expectedSitesResponse.AddRange(adminSites);
            expectedSitesResponse.AddRange(otherInaccessibleSites);

            expectedSitesResult.AddRange(canViewSites);
            expectedSitesResult.AddRange(adminSites);

            var userAssignments = new List<UserAssignment>();
            foreach (var site in canViewSites)
            {
                userAssignments.Add(new UserAssignment(Permissions.ViewSites, site.Id));
            }

            foreach (var site in adminSites)
            {
                userAssignments.Add(new UserAssignment(Permissions.ManageFloors, site.Id));
            }
            var userDetails = Fixture.Build<GetUserDetailsResponse>()
                .With(x => x.CustomerId, expectedCustomer.Id)
                .With(x => x.Customer, expectedCustomer)
                .With(x => x.UserAssignments, userAssignments)
                .Create();




            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userDetails.Id);
            var directoryApiHandler = server.Arrange().GetDirectoryApi();
            var siteApiHandler = server.Arrange().GetSiteApi();

            var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSitesResult, server.Assert().GetImageUrlHelper());
            foreach (var site in expectedSiteDetailDtos)
            {
                site.Features = new SiteFeaturesDto();
                site.ArcGisLayers = new List<ArcGisLayerDto>();
                var adminSite = adminSites.FirstOrDefault(x => x.Id == site.Id);
                if (adminSite is not null)
                    site.UserRole = "admin";
            }
            directoryApiHandler
                   .SetupRequest(HttpMethod.Get, $"users/{userDetails.Id}/userDetails")
                   .ReturnsJson(userDetails);


            siteApiHandler
                .SetupRequest(HttpMethod.Get, $"sites/customer/{userDetails.CustomerId}/extend")
                .ReturnsJson(expectedSitesResponse);


            var response = await client.GetAsync("/me/sites");
            response.StatusCode.Should().Be(HttpStatusCode.OK);


            var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();
            result.Count.Should().Be(12);
            result.Should().BeEquivalentTo(expectedSiteDetailDtos);
        }

        [Fact]
        public async Task CustomerAdminUser_GetSites_ReturnSites()
        {
            var expectedCustomer = Fixture.Build<Customer>()
             .With(x => x.Features, new CustomerFeatures()
             {
                 IsConnectivityViewEnabled = false
             })
             .Create();
            var expectedSitesResponse = new List<Site>();

            var adminSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(7)
               .ToList();


            var otherSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(30)
               .ToList();


            expectedSitesResponse.AddRange(adminSites);
            expectedSitesResponse.AddRange(otherSites);

            var userAssignments = new List<UserAssignment>
            {
                new UserAssignment(Permissions.ManageFloors, expectedCustomer.Id)
            };

            var userDetails = Fixture.Build<GetUserDetailsResponse>()
                .With(x => x.CustomerId, expectedCustomer.Id)
                .With(x => x.Customer, expectedCustomer)
                .With(x => x.UserAssignments, userAssignments)
                .Create();




            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userDetails.Id);
            var directoryApiHandler = server.Arrange().GetDirectoryApi();
            var siteApiHandler = server.Arrange().GetSiteApi();

            var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSitesResponse, server.Assert().GetImageUrlHelper());
            foreach (var site in expectedSiteDetailDtos)
            {
                site.Features = new SiteFeaturesDto();
                site.ArcGisLayers = new List<ArcGisLayerDto>();
                site.UserRole = "admin";
            }
            directoryApiHandler
                   .SetupRequest(HttpMethod.Get, $"users/{userDetails.Id}/userDetails")
                   .ReturnsJson(userDetails);
            siteApiHandler
                .SetupRequest(HttpMethod.Get, $"sites/customer/{userDetails.CustomerId}/extend")
                .ReturnsJson(expectedSitesResponse);


            var response = await client.GetAsync("/me/sites");
            response.StatusCode.Should().Be(HttpStatusCode.OK);


            var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();
            result.Should().BeEquivalentTo(expectedSiteDetailDtos);
        }

        [Fact]
        public async Task ProfileAdminUserAndCanViewOtherSites_GetSites_ReturnSites()
        {
            var profileId = Guid.NewGuid();
            var expectedCustomer = Fixture.Build<Customer>()
              .With(x => x.Features, new CustomerFeatures()
              {
                  IsConnectivityViewEnabled = false
              })
              .Create();
            var expectedSitesResponse = new List<Site>();
            var expectedSitesResult = new List<Site>();
            var canViewSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(5)
               .ToList();

            var profileAdminSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.PortfolioId, profileId)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(7)
               .ToList();


            var otherInaccessibleSites = Fixture.Build<Site>()
               .With(x => x.CustomerId, expectedCustomer.Id)
               .With(x => x.Status, SiteStatus.Operations)
               .With(x => x.Features, new SiteFeatures())
               .With(x => x.ArcGisLayers, new List<ArcGisLayer>())
               .CreateMany(30)
               .ToList();

            expectedSitesResponse.AddRange(canViewSites);
            expectedSitesResponse.AddRange(profileAdminSites);
            expectedSitesResponse.AddRange(otherInaccessibleSites);

            expectedSitesResult.AddRange(canViewSites);
            expectedSitesResult.AddRange(profileAdminSites);

            var userAssignments = new List<UserAssignment>
            {
                new UserAssignment(Permissions.ManageFloors, profileId)
            };
            foreach (var site in canViewSites)
            {
                userAssignments.Add(new UserAssignment(Permissions.ViewSites, site.Id));
            }
            var userDetails = Fixture.Build<GetUserDetailsResponse>()
                .With(x => x.CustomerId, expectedCustomer.Id)
                .With(x => x.Customer, expectedCustomer)
                .With(x => x.UserAssignments, userAssignments)
                .Create();

            using var server = CreateServerFixture(ServerFixtureConfigurations.Default);
            using var client = server.CreateClient(null, userDetails.Id);
            var directoryApiHandler = server.Arrange().GetDirectoryApi();
            var siteApiHandler = server.Arrange().GetSiteApi();

            var expectedSiteDetailDtos = SiteDetailDto.Map(expectedSitesResult, server.Assert().GetImageUrlHelper());
            foreach (var site in expectedSiteDetailDtos)
            {
                site.Features = new SiteFeaturesDto();
                site.ArcGisLayers = new List<ArcGisLayerDto>();
                var adminSite = profileAdminSites.FirstOrDefault(x => x.Id == site.Id);
                if (adminSite is not null)
                    site.UserRole = "admin";
            }
            directoryApiHandler
                   .SetupRequest(HttpMethod.Get, $"users/{userDetails.Id}/userDetails")
                   .ReturnsJson(userDetails);


            siteApiHandler
                .SetupRequest(HttpMethod.Get, $"sites/customer/{userDetails.CustomerId}/extend")
                .ReturnsJson(expectedSitesResponse);


            var response = await client.GetAsync("/me/sites");
            response.StatusCode.Should().Be(HttpStatusCode.OK);


            var result = await response.Content.ReadAsAsync<List<SiteDetailDto>>();
            result.Count.Should().Be(12);
            result.Should().BeEquivalentTo(expectedSiteDetailDtos);
        }
    }
}
