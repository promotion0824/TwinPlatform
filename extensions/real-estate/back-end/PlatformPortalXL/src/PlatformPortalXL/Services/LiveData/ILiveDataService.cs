using PlatformPortalXL.Models;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.LiveData
{
    public interface ILiveDataService
    {
        Task<Asset> GetAssetByEquipmentIdAsync(Guid siteId, Guid equipmentId);
        Task<Device> GetDeviceAsync(Guid siteId, Guid deviceId);
        Task<Point> GetPointAsync(Guid siteId, Guid pointEntityId);
        Task<int> GetConnectorPointCount(Guid siteId, Guid connectorId);
    }
}
