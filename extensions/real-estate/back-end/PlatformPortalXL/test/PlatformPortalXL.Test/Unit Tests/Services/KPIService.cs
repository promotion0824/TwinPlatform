using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Moq;
using Xunit;

using Willow.Api.Client;
using Willow.Data;
using Willow.KPI.Service;
using Willow.Platform.Models;

using PlatformPortalXL.Services;

using PlatformPortalXL.Test.Unit_Tests;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Services.Forge;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;

namespace Willow.PlatformPortal.XL.UnitTests
{
    [Trait("KPI", "PortalXL")]
    public class KPIServiceTests
    {
        private readonly Mock<ICoreKPIService> _api = new Mock<ICoreKPIService>();
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly Mock<IReadRepository<Guid, Site>> _siteRepo = new Mock<IReadRepository<Guid, Site>>();
        private readonly Mock<IDigitalTwinApiService> _dtApiService = new Mock<IDigitalTwinApiService>();
        private readonly Guid _portfolioId = Guid.NewGuid();
        private readonly ILogger<KPIService> _logger = new Mock<ILogger<KPIService>>().Object;

        // Sites
        private readonly (Guid SiteId, string Name) _site1 = (Guid.Parse("404bd33c-a697-4027-b6a6-677e30a53d07"), "123 Main St");
        private readonly (Guid SiteId, string Name) _site2 = (Guid.Parse("a6b78f54-9875-47bc-9612-aa991cc464f3"), "Central Tower");
        private readonly (Guid SiteId, string Name) _site3 = (Guid.Parse("934638e3-4bd7-4749-bd52-bd6e47d0fbb2"), "Downtown Tower");
        private readonly (Guid SiteId, string Name) _site4 = (Guid.Parse("e719ac18-192b-4174-91db-b3a624f1f1a4"), "67 Lincoln Blvd");
        private readonly (Guid SiteId, string Name) _site5 = (Guid.Parse("092D18F5-F5CE-400F-B904-FD4F7BAF104B"), "Centre Building");
        private readonly (Guid SiteId, string Name) _site6 = (Guid.Parse("EC327557-950A-45CA-9B54-8D36752DBB6B"), "Town Hall Tower");
        private readonly (Guid SiteId, string Name) _site7 = (Guid.Parse("180E1DCF-2F74-4D01-8019-E2D458CD7B87"), "Northern Hotel");
        private readonly (Guid SiteId, string Name) _site8 = (Guid.Parse("3A6D58E8-C8E0-4F30-AA58-38BAFB7937E2"), "592 Church Street");
        private readonly (Guid SiteId, string Name) _site9 = (Guid.Parse("E9AE3306-D6FE-497B-BF82-1971C891525A"), "54 Sunset Strip");
        private readonly (Guid SiteId, string Name) _site10 = (Guid.Parse("C90B5874-8E9E-4AFB-994F-26E7E8EB8818"), "45 George Street");

        // Twins
        // DDK - North America
        private readonly TwinDto _twin1 = new TwinDto()
        {
            Id = "WIL-101RS",
            Name = "101 Ridley Square",
            SiteId = Guid.Parse("e3ea6775-50e5-4d19-afec-b103d08658a3"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twin3 = new TwinDto()
        {
            Id = "WIL-Retail-007",
            Name = "Retail Store #7",
            SiteId = Guid.Parse("bbb0dd63-656e-46e7-b523-1af465d24aa9"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };

        // DDK - Europe
        private readonly TwinDto _twin2 = new TwinDto()
        {
            Id = "WIL-220FA",
            Name = "220 Francis Avenue",
            SiteId = Guid.Parse("2bada6d2-ccd7-43dd-a42a-c8ab0873df64"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twin4 = new TwinDto()
        {
            Id = "WIL-104BDFD",
            Name = "104 Bedford Square",
            SiteId = Guid.Parse("a226929d-6e27-480f-b8dd-40ffbc47024c"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twin5 = new TwinDto()
        {
            Id = "WIL-57CM",
            Name = "Canary Wharf Underground Parking",
            SiteId = Guid.Parse("45ac7d4b-fd70-4f7c-a220-e944112159cc"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twin6 = new TwinDto()
        {
            Id = "Wil-CanaryWharf-Substructure",
            Name = "57 CJ Marina",
            SiteId = Guid.Parse("f46f061b-2070-4971-849e-93df84aaaf2e"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twin7 = new TwinDto()
        {
            Id = "WIL-CanaryWharf-JubileePark",
            Name = "Jubilee Park",
            SiteId = Guid.Parse("a598746e-66e4-497c-a04a-6a928178377a"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:Building;1" }
        };
        private readonly TwinDto _twinNotABuilding = new TwinDto()
        {
            Id = "WIL-101RS-PVAV-L13-05",
            Name = "PVAV-L13-05",
            SiteId = Guid.Parse("e3ea6775-50e5-4d19-afec-b103d08658a3"),
            Metadata = new TwinMetadataDto() { ModelId = "dtmi:com:willowinc:VAVBox;1" }
        };

        private readonly TwinDto _twinNotFound = new TwinDto()
        {
            Id = "TWIN-NOT-FOUND",
            Name = "TWIN-NOT-FOUND"
        };

        private readonly TwinDto _twinNoMetadata = new TwinDto() { Id = "TWIN-NO-METADATA", Name = "TWIN-NO-METADATA" };


        public KPIServiceTests()
        {
            _siteRepo.Setup(r => r.Get(_site1.SiteId)).ReturnsAsync(new Site { Name = _site1.Name });
            _siteRepo.Setup(r => r.Get(_site2.SiteId)).ReturnsAsync(new Site { Name = _site2.Name });
            _siteRepo.Setup(r => r.Get(_site3.SiteId)).ReturnsAsync(new Site { Name = _site3.Name });
            _siteRepo.Setup(r => r.Get(_site4.SiteId)).ReturnsAsync(new Site { Name = _site4.Name });
            _siteRepo.Setup(r => r.Get(_site5.SiteId)).ReturnsAsync(new Site { Name = _site5.Name });
            _siteRepo.Setup(r => r.Get(_site6.SiteId)).ReturnsAsync(new Site { Name = _site6.Name });
            _siteRepo.Setup(r => r.Get(_site7.SiteId)).ReturnsAsync(new Site { Name = _site7.Name });
            _siteRepo.Setup(r => r.Get(_site8.SiteId)).ReturnsAsync(new Site { Name = _site8.Name });
            _siteRepo.Setup(r => r.Get(_site9.SiteId)).ReturnsAsync(new Site { Name = _site9.Name });
            _siteRepo.Setup(r => r.Get(_site10.SiteId)).ReturnsAsync(new Site { Name = _site10.Name });

            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin1.Id)).ReturnsAsync(_twin1);
            _siteRepo.Setup(r => r.Get(_twin1.SiteId ?? new Guid())).ReturnsAsync(new Site { Name = _twin1.Name });
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin2.Id)).ReturnsAsync(_twin2);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin3.Id)).ReturnsAsync(_twin3);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin4.Id)).ReturnsAsync(_twin4);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin5.Id)).ReturnsAsync(_twin5);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin6.Id)).ReturnsAsync(_twin6);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twin7.Id)).ReturnsAsync(_twin6);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twinNotABuilding.Id)).ReturnsAsync(_twinNotABuilding);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twinNoMetadata.Id)).ReturnsAsync(_twinNoMetadata);
            _dtApiService.Setup(r => r.GetTwin<TwinDto>(_twinNotFound.Id)).ThrowsAsync(new RestException("Twin not found.", System.Net.HttpStatusCode.NotFound, null));

            //_dtApiService.Setup(r => r.IsBuildingScopeModelId(It.IsRegex(@"(Building|AirportTerminal|Substructure|OutdoorArea)"))).Returns(true);
            _dtApiService.Setup(r => r.IsBuildingScopeModelId(It.IsRegex("Building"))).Returns(true);
        }

        [Theory]
        [InlineData("get_building_data", 1)]
        [InlineData("building_data", 1)]
        [InlineData("kpis_data", 3)]
        [InlineData("kpis", 3)]
        [InlineData("get_kpis", 3)]
        public async Task KPIService_Get_success(string viewName, int numMetrics)
        {
            var svc = new KPIService(Guid.NewGuid(), new KPIAPI(new FakeQueryRepository("test"), "bob"), _siteRepo.Object, _dtApiService.Object, _cache, 2, _logger);

            var result = await svc.GetByMetric(_portfolioId, viewName, null);

            Assert.NotNull(result);

            var list = result.ToList();

            Assert.Equal(numMetrics, list.Count);
            Assert.NotEmpty(list[0].Values);
            Assert.Equal(10, list[0].Values.Count);
            Assert.Equal("123 Main St", list[0].Values[0].XValue);
            Assert.Equal(.98, list[0].Values[0].YValue);
        }

        [Theory]
        [InlineData("get_building_data", "building", false)]
        [InlineData("building_data", "building", false)]
        [InlineData("kpis_data", "kpis", false)]
        [InlineData("kpis", "kpis", false)]
        [InlineData("get_kpis", "kpis", false)]
        [InlineData("operationaltrends", "trends", true)]
        [InlineData("operational_trend_metrics", "trends", true)]
        [InlineData("overallperformance", "overall_performance", false)]
        [InlineData("overall_performance_data", "overall_performance", false)]
        public async Task KPIService_Get_check_params(string viewName, string actualViewName, bool sortByX)
        {
            var customerId = Guid.NewGuid();
            var svc = new KPIService(customerId, _api.Object, _siteRepo.Object, _dtApiService.Object, _cache, 2, _logger);

            var result = await svc.GetByMetric(_portfolioId, viewName, null);

            _api.Verify(a => a.GetByMetric(_portfolioId, actualViewName, null, sortByX,false), Times.Once);

            var filterHash = "";
            var cacheKey = $"{customerId}-{_portfolioId}-{actualViewName}-{filterHash}";

            Assert.True(_cache.TryGetValue(cacheKey, out object _));

            // Do it again
            result = await svc.GetByMetric(_portfolioId, viewName, null);

            // Make we got it from the cache
            _api.Verify(a => a.GetByMetric(_portfolioId, actualViewName, null, sortByX, false), Times.Once);
        }

        [Theory]
        [InlineData("get_building_data", 1)]
        [InlineData("building_data", 1)]
        [InlineData("kpis_data", 3)]
        [InlineData("kpis", 3)]
        [InlineData("get_kpis", 3)]
        public async Task KPIService_Get_cache(string viewName, int numMetrics)
        {
            var svc = new KPIService(Guid.NewGuid(), new KPIAPI(new FakeQueryRepository("test"), "bob"), _siteRepo.Object, _dtApiService.Object, _cache, 2, _logger);

            var result = await svc.GetByMetric(_portfolioId, viewName, null);

            Assert.NotNull(result);

            var list = result.ToList();

            Assert.Equal(numMetrics, list.Count);
            Assert.NotEmpty(list[0].Values);
            Assert.Equal(10, list[0].Values.Count);
            Assert.Equal("123 Main St", list[0].Values[0].XValue);
            Assert.Equal(.98, list[0].Values[0].YValue);
        }

        [Fact(Skip = "fix later")]
        public async Task KPIService_GetDatedBuildingScore()
        {
            var svc = new KPIService(Guid.NewGuid(), new KPIAPI(new FakeQueryRepository("test"), "bob"), _siteRepo.Object, _dtApiService.Object, _cache, 2, _logger);

            var score = "comfort";

            var kpiRequest = new KPIRequest
            {
                SiteIds = null,
                StartDate = DateTime.Parse("2024-07-01T00:00:00"),
                EndDate = DateTime.Parse("2024-07-08T00:00:00"),
                GroupBy = "date"
            };

            //var result = await svc.GetDatedBuildingScore(_portfolioId, _twin_nometadata.Id, score, null);
            var result = await svc.GetDatedBuildingScore(_portfolioId, _twin1.Id, score, kpiRequest);

            Assert.NotNull(result);
            Assert.False(true);

        }

        [Fact]
        public async Task KPIService_GetDatedBuildingScore_Throws_WhenTwinOrSiteIdsAreInError()
        {
            var svc = new KPIService(Guid.NewGuid(), new KPIAPI(new FakeQueryRepository("test"), "bob"), _siteRepo.Object, _dtApiService.Object, _cache, 2, _logger);

            var score = "comfort";

            var kpiRequest = new KPIRequest
            {
                SiteIds = null,
                StartDate = DateTime.Parse("2024-07-01T00:00:00"),
                EndDate = DateTime.Parse("2024-07-08T00:00:00"),
                GroupBy = "date"
            };

            // No twin found by dtcore
            await Assert.ThrowsAsync<RestException>(() => svc.GetDatedBuildingScore(_portfolioId, _twinNotFound.Id, score, kpiRequest));
            // No twinId provided and no siteIds provided
            await Assert.ThrowsAsync<ArgumentNullException>(() => svc.GetDatedBuildingScore(_portfolioId, null, score, kpiRequest));
            // Twin is not a building
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.GetDatedBuildingScore(_portfolioId, _twinNotABuilding.Id, score, kpiRequest));

        }
    }
}
