using AutoFixture;
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using Microsoft.Extensions.Caching.Memory;
using Willow.Platform.Statistics;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.Models;
using PlatformPortalXL.Features.Twins;
using System.Linq;
using System.Collections.Generic;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Unit_Tests
{
    public class FloorServiceTests
    {
        private readonly FloorsService _service;
        private readonly Mock<IFloorsApiService> _floorsApiService = new();
        private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly Mock<IDigitalTwinApiService> _digitalTwinApiService = new();
        private readonly Mock<IGeometryViewerMessagingService> _geometryViewerMessagingService = new();

        private readonly Guid _siteId = Guid.NewGuid();

        public FloorServiceTests()
        {
            _service = new FloorsService(
                _floorsApiService.Object, 
                _memoryCache,
                _digitalTwinApiService.Object,
                _geometryViewerMessagingService.Object);
        }

        [Fact]
        public async Task FloorsService_UpdateSortOrder_success()
        {
            var floorIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            _floorsApiService.Setup(f => f.UpdateSortOrder(_siteId, floorIds));

            await _service.UpdateSortOrder(_siteId, floorIds);

            _floorsApiService.Verify(f => f.UpdateSortOrder(_siteId, floorIds), Times.Once);

        }

        [Fact]
        public async Task FloorsService_Broadcast_success()
        {
            var fixture = new Fixture();
            var floorId = Guid.NewGuid();
            var twinId = "dummyTwinId";
            var urns = fixture.Build<string>().CreateMany(1).ToList();

            var searchTwins = fixture.Build<TwinSearchResponse.SearchTwin>()
                .With(x => x.SiteId, _siteId)
                .With(x => x.FloorId, floorId)
                .With(x => x.Id, twinId)
                .CreateMany(1).ToArray();

            var twinSearchResponse = fixture.Build<TwinSearchResponse>()
                .With(x => x.Twins, searchTwins)
                .Create();

            _geometryViewerMessagingService.Setup(f => f.Send(_siteId, twinId, urns[0]));
            _digitalTwinApiService.Setup(f => f.Search(It.IsAny<TwinSearchRequest>())).ReturnsAsync(twinSearchResponse);

            await _service.Broadcast(_siteId, floorId, urns);

            _geometryViewerMessagingService.Verify(f => f.Send(_siteId, twinId, urns[0]), Times.Once);

        }

		[Fact]
		public async Task FloorsService_GetFloors_success()
		{
			var floorIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
			_floorsApiService.Setup(f => f.GetFloorsAsync(floorIds));

			await _service.GetFloorsAsync(floorIds);

			_floorsApiService.Verify(f => f.GetFloorsAsync(floorIds), Times.Once);
		}
	}
}
