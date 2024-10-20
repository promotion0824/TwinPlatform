using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.LiveData
{
    public class ConnectorLiveDataService : ILiveDataService
    {
        private readonly IConnectorApiService _connectorApiService;

        public ConnectorLiveDataService(IConnectorApiService connectorApiService)
        {
            _connectorApiService = connectorApiService;
        }

        public async Task<Asset> GetAssetByEquipmentIdAsync(Guid siteId, Guid equipmentId)
        {
            var equipment = await _connectorApiService.GetEquipment(equipmentId, true, true);

            return new Asset
            {
                Id = equipmentId,
                EquipmentId = equipmentId,
                Name = equipment.Name,
                EquipmentName = equipment.Name,
                HasLiveData = true,
                Points = AssetPoint.MapFrom(equipment.Points),
                PointTags = equipment.PointTags
            };
        }

        public async Task<Device> GetDeviceAsync(Guid siteId, Guid deviceId)
        {
            return await _connectorApiService.GetDeviceAsync(siteId, deviceId);
        }

        public async Task<Point> GetPointAsync(Guid siteId, Guid pointEntityId)
        {
            return await _connectorApiService.GetPoint(pointEntityId);
        }

        public async Task<int> GetConnectorPointCount(Guid siteId, Guid connectorId)
            => (await _connectorApiService.GetConnectorById(siteId, connectorId)).PointsCount;
    }
}
