using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using Moq;
using Willow.Data;
using Willow.KPI.Service;
using Willow.Platform.Models;
using PlatformPortalXL.Features;
using PlatformPortalXL.Features.KPI;
using PlatformPortalXL.Http;
using PlatformPortalXL.Services;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using System.Threading;
using Polly.Registry;
using NSubstitute;
using Polly;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.SiteApi;

namespace PlatformPortalXL.Test.Unit_Tests
{
    //[Trait("KPI", "PortalXL")]
    public class KPIControllerTests
    {
        private readonly IKPIServiceFactory _svcFactory;
        private readonly Mock<IReadRepository<Guid, Site>> _siteRepo = new Mock<IReadRepository<Guid, Site>>();
        private readonly Mock<IDigitalTwinApiService> _dtApiService = new Mock<IDigitalTwinApiService>();
        private readonly Mock<IAccessControlService> _accessControl = new Mock<IAccessControlService>();
        private readonly Mock<IHttpRequestHeaders> _requestHeaders = new Mock<IHttpRequestHeaders>();
        private readonly Mock<ISiteApiService> _siteApiService = new Mock<ISiteApiService>();
        private IResiliencePipelineService _resiliencePipelineService;

        public KPIControllerTests()
        {
            _svcFactory = new FakeKPIServiceFactory("bob", _siteRepo.Object, _dtApiService.Object);

            ResiliencePipelineProvider<string> pipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();
            // Mock the pipeline provider to return an empty pipeline for testing
            pipelineProvider
                .GetPipeline("retry-pipeline")
                .Returns(ResiliencePipeline.Empty);
            _resiliencePipelineService = new ResiliencePipelineService(pipelineProvider);
            _accessControl.Setup( a=> a.EnsureAccessPortfolio(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()) );
        }

        [Fact]
        public async Task KPIController_Get_success()
        {
            _requestHeaders.Setup( r=> r.Get(It.IsAny<HttpContext>(), It.IsAny<string>(), true) ).Returns(Guid.NewGuid().ToString());

            var controller = new KPIController(_svcFactory, _accessControl.Object, _resiliencePipelineService, _requestHeaders.Object, _dtApiService.Object, _siteApiService.Object);

            var result = await controller.Get("operationaltrends");

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task KPIController_Get_filter()
        {
            _requestHeaders.Setup( r=> r.Get(It.IsAny<HttpContext>(), It.IsAny<string>(), true) ).Returns(Guid.NewGuid().ToString());

            var controller = new KPIController(_svcFactory, _accessControl.Object, _resiliencePipelineService, _requestHeaders.Object, _dtApiService.Object, _siteApiService.Object);

			var result = await controller.Get("operationaltrends", new KPIRequest
			{
				SiteIds = null,
				StartDate = DateTime.Parse("2021-01-01T00:00:00"),
				EndDate = DateTime.Parse("2021-05-01T00:00:00"),
				SelectedDayRange = new[] { "weekEnds" },
				SelectedBusinessHourRange = new[] { "inBusinessHours" }
			});

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task KPIController_Get_invalidheader()
        {
            var controller = new KPIController(_svcFactory, _accessControl.Object, _resiliencePipelineService, _requestHeaders.Object, _dtApiService.Object, _siteApiService.Object);

            await Assert.ThrowsAsync<Exception>( async ()=> await controller.Get("operationaltrends"));
        }

        [Fact]
        public async Task KPIController_Get_notfound()
        {
            _requestHeaders.Setup( r=> r.Get(It.IsAny<HttpContext>(), It.IsAny<string>(), true) ).Returns(Guid.NewGuid().ToString());
            var controller = new KPIController(_svcFactory, _accessControl.Object, _resiliencePipelineService, _requestHeaders.Object, _dtApiService.Object, _siteApiService.Object);

            await Assert.ThrowsAsync<NotFoundException>( async ()=> await controller.Get("bobsyouruncle"));
        }
    }
}
