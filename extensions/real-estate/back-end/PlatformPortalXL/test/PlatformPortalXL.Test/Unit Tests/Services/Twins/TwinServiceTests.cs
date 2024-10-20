using Microsoft.Extensions.Configuration;
using Moq;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Twins;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Assets;
using Xunit;

namespace PlatformPortalXL.Test.Unit_Tests.Services.Twins
{
    public class TwinServiceTests
    {
        private readonly Mock<IDigitalTwinApiService> _digitalTwinApiService = new Mock<IDigitalTwinApiService>();
        private readonly Mock<IDigitalTwinAssetService> _digitalTwinService = new Mock<IDigitalTwinAssetService>();
        private readonly Mock<IConnectorService> _connectorService = new Mock<IConnectorService>();
        private readonly Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private readonly TwinService _service;

        public TwinServiceTests()
        {
            _service = new TwinService(_digitalTwinService.Object,_connectorService.Object,_digitalTwinApiService.Object,  _configuration.Object);
        }

        [Fact]
        public async Task DigitalTwinApiService_GetTwin_Points()
        {
            var points = new List<PointDto>
            {
                new PointDto
                {
                    Id = Guid.NewGuid()
                }
            };

            var deviceDto = new DeviceDto()
            {
                Id = Guid.NewGuid()
            };

            var connectors = new List<ConnectorDto>
            {
                new ConnectorDto
                {
                    Id= Guid.NewGuid()
                }
            };

            _digitalTwinService.Setup(s => s.GetDeviceAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(deviceDto);
            _connectorService.Setup(s => s.GetConnectors(It.IsAny<Guid>())).ReturnsAsync(connectors);
            _digitalTwinApiService.Setup(s => s.GetTwinPointsAsync(It.IsAny<Guid>(),It.IsAny<Guid>())).ReturnsAsync(points);
            
            var result = await _service.GetTwinPointsAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
