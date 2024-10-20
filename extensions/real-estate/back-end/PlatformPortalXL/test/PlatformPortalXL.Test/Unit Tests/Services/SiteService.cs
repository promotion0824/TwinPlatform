using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.DirectoryApi.Responses;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Sites;
using Willow.Platform.Models;
using Willow.Workflow;
using Willow.Workflow.Models;
using Xunit;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Test.UnitTests
{
    public class SiteServiceTests
    {
        private readonly ISiteService _siteService;
        private readonly Guid         _userId     = Guid.NewGuid();
        private readonly Guid         _customerId = Guid.NewGuid();
        private readonly Guid         _siteId1    = Guid.NewGuid();
        private readonly Guid         _siteId2    = Guid.NewGuid();
        private readonly Guid         _siteId3    = Guid.NewGuid();
        private readonly Guid         _siteId4    = Guid.NewGuid();

        private readonly Mock<IDirectoryApiService> _directoryApi = new();
        private readonly Mock<IInsightApiService> _insightApi = new();
		private readonly Mock<ISiteApiService> _siteApi = new ();
        private readonly Mock<IImageUrlHelper> _imageUrlHelper = new ();
        private readonly Mock<ITimeZoneService> _timeZoneService = new ();
        private readonly Mock<IWeatherService> _weatherService = new();
        private readonly Mock<IPortfolioDashboardService> _portfolioDashboard = new();
        private readonly Mock<IDigitalTwinApiService> _digitalTwinApiService = new();
        private readonly Mock<IGeometryViewerMessagingService> _geometryViewerMessagingService = new ();
        private readonly Mock<ILogger<SiteService>> _logger = new();
        private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly Mock<IWorkflowApiService> _workflowApiService = new();
        private readonly Mock<ISiteIdToTwinIdMatchingService>  _siteIdToTwinIdMatch = new();
        private readonly Mock<IUserAuthorizedSitesService> _userAuthorizedSitesService = new();

        public SiteServiceTests()
        {
            _siteApi.Setup( d=> d.GetSitesByCustomerAsync(_customerId, null,null,null)).ReturnsAsync( new List<Site>
            {
                new Site { Id = _siteId1, CustomerId = _customerId, Name = "Fred",    Country = "Australia", Address = "123 Elm St", Features = new SiteFeatures{IsArcGisEnabled = true } },
                new Site { Id = _siteId2, CustomerId = _customerId, Name = "Wilma",   Country = "USA",       Address = "456 Maple Ln", Features = new SiteFeatures()},
                new Site { Id = _siteId3, CustomerId = _customerId, Name = "Pebbles", Country = "Denmark",   Address = "789 Oak Dr" , Features = new SiteFeatures()},
                new Site { Id = _siteId4, CustomerId = _customerId, Name = "NotUserSite", Country = "Denmark",   Address = "789 Oak Dr" , Features = new SiteFeatures()}
            });


            _insightApi.Setup(s=> s.GetInsightStatisticsBySiteIds(It.IsAny<List<Guid>>())).ReturnsAsync(new InsightStatisticsResponse {
                StatisticsByPriority = new List<SiteInsightStatistics> { new SiteInsightStatistics {Id = _siteId1,UrgentCount = 11 } },
                StatisticsByStatus = new List<SiteInsightStatisticsByStatus> { new SiteInsightStatisticsByStatus { Id = _siteId1, OpenCount= 12} }

                });
            _timeZoneService.Setup( tz=> tz.GetTimeZoneType(It.IsAny<string>())).Returns("Narnia");
            _weatherService.Setup(w => w.GetWeather(It.IsAny<Guid>())).Returns(new Dto.WeatherDto { Temperature = 14.4m });


            _workflowApiService.Setup(s => s.GetTicketStatisticsBySiteIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new TicketStatisticsResponse
            {
                StatisticsByPriority = new List<TicketStatisticsByPriority> { new TicketStatisticsByPriority { Id = _siteId1, OverdueCount = 12 } },
                StatisticsByStatus = new List<TicketStatisticsByStatus> { new TicketStatisticsByStatus { Id = _siteId1, OpenCount = 8 } }
            }) ;

            _siteIdToTwinIdMatch.Setup(s => s.FindMatchToMostSignificantSpatialTwin(It.IsAny<Guid>())).ReturnsAsync("twinId");

            _siteService = new SiteService(_directoryApi.Object,
											_insightApi.Object,
											_siteApi.Object,
                                           _imageUrlHelper.Object,
                                           _timeZoneService.Object,
                                           _weatherService.Object,
                                           _portfolioDashboard.Object,
                                           _digitalTwinApiService.Object,
                                           _geometryViewerMessagingService.Object,
                                           _logger.Object,
                                           _memoryCache,
                                           _workflowApiService.Object,
                                           _siteIdToTwinIdMatch.Object);
        }

        [Theory]
        [InlineData(true, ServiceStatus.Online, true)]
        [InlineData(true, ServiceStatus.Offline, false)]
        [InlineData(false, ServiceStatus.Online, null)]
        public async Task SiteService_GetSites(bool isConnectivityViewEnabled, ServiceStatus status, bool? isOnline)
        {
            var fixture = new Fixture();
            var customer = new Models.Customer { Id = _customerId, Features = new Models.CustomerFeatures { IsConnectivityViewEnabled = isConnectivityViewEnabled } };

            var userAssignments = new List<UserAssignment>
            {
                new UserAssignment(Permissions.ViewSites, _siteId1),
                new UserAssignment(Permissions.ViewSites, _siteId2),
                new UserAssignment(Permissions.ManageFloors, _siteId3)
            };
            var userDetails = fixture.Build<GetUserDetailsResponse>()
                                      .With(x=>x.Id, _userId)
                                      .With(x => x.CustomerId, customer.Id)
                                      .With(x => x.Customer, customer)
                                      .With(x => x.UserAssignments, userAssignments)
                                      .Create();

            _directoryApi.Setup(d => d.GetUserDetailsAsync(It.IsAny<Guid>())).ReturnsAsync(userDetails);


            _portfolioDashboard.Setup(d => d.GetSiteSimpleDashboardDataAsync(It.IsAny<Site>())).ReturnsAsync(new PortfolioDashboardSiteStatus { Status = status });
            var sites = await _siteService.GetSites(_userId);

            Assert.NotNull(sites);
            Assert.Equal(3, sites.Count);

            Assert.Contains(sites, s => s.Id == _siteId1);
            Assert.Contains(sites, s => s.Id == _siteId2);
            Assert.Contains(sites, s => s.Id == _siteId3);

            Assert.Equal("Narnia", sites[0].TimeZone);
            Assert.Equal(isOnline, sites[0].IsOnline);
            Assert.NotEqual("admin",  sites.Single(s => s.Id == _siteId1).UserRole);
            Assert.NotEqual("admin",  sites.Single(s => s.Id == _siteId2).UserRole);
            Assert.Equal("admin",     sites.Single(s => s.Id == _siteId3).UserRole);
            Assert.Equal(12, sites[0].TicketStats.OverdueCount);
            Assert.Equal(11, sites[0].InsightsStats.UrgentCount);

            Assert.Equal("Australia", sites[0].Country);
            Assert.Equal("123 Elm St", sites[0].Address);
            sites = await _siteService.GetSites(_userId);

            Assert.NotNull(sites);
            Assert.Equal(3, sites.Count);

            Assert.Contains(sites, s => s.Id == _siteId1);
            Assert.Contains(sites, s => s.Id == _siteId2);
            Assert.Contains(sites, s => s.Id == _siteId3);

            Assert.Equal("Narnia", sites[0].TimeZone);
            Assert.Equal(isOnline, sites[0].IsOnline);
            Assert.NotEqual("admin",  sites.Single(s => s.Id == _siteId1).UserRole);
            Assert.NotEqual("admin",  sites.Single(s => s.Id == _siteId2).UserRole);
            Assert.Equal("admin",     sites.Single(s => s.Id == _siteId3).UserRole);
            Assert.Equal(12, sites[0].TicketStats.OverdueCount);
            Assert.Equal(11, sites[0].InsightsStats.UrgentCount);

            Assert.Equal("Australia", sites[0].Country);
            Assert.Equal("123 Elm St", sites[0].Address);
            Assert.True(sites[0].Features.IsArcGisEnabled);
        }
    }
}
