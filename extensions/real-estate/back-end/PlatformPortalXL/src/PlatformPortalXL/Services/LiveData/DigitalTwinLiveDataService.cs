using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.LiveData
{
    public class DigitalTwinLiveDataService : ILiveDataService
    {
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        public DigitalTwinLiveDataService(IDigitalTwinApiService digitalTwinApiService)
        {
            _digitalTwinApiService = digitalTwinApiService;
        }

        public async Task<Asset> GetAssetByEquipmentIdAsync(Guid siteId, Guid equipmentId)
        {
            return await _digitalTwinApiService.GetAssetAsync(siteId, equipmentId);
        }

        public async Task<Device> GetDeviceAsync(Guid siteId, Guid deviceId)
        {
            var dto = await _digitalTwinApiService.GetDeviceAsync(siteId, deviceId);
            return dto.MapToModel();
        }

        public async Task<Point> GetPointAsync(Guid siteId, Guid pointEntityId)
        {
            var dto = await _digitalTwinApiService.GetPointByTrendIdAsync(siteId, pointEntityId);
            return dto.MapToModel(siteId);
        }

        public async Task<int> GetConnectorPointCount(Guid siteId, Guid connectorId)
            => await _digitalTwinApiService.GetPointCountByConnectorAsync(siteId, connectorId);
    }
}
