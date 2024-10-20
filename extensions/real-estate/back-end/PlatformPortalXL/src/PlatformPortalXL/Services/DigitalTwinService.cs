using System;
using System.Threading.Tasks;
using PlatformPortalXL.Services.LiveData;

namespace PlatformPortalXL.Services
{
    /// <summary>
    /// This interface and class are deprecated and useless and should be deleted.
    /// https://dev.azure.com/willowdev/Unified/_workitems/edit/136645
    /// </summary>
    public interface IDigitalTwinService
    {
        Task<ILiveDataService> GetLiveDataServiceAsync(Guid siteId);
	}

    public class DigitalTwinService : IDigitalTwinService
    {
        private readonly DigitalTwinLiveDataService _digitalTwinLiveDataService;

        public DigitalTwinService(
            DigitalTwinLiveDataService digitalTwinPointsService,
            ConnectorLiveDataService connectorPointsService)
        {
            _digitalTwinLiveDataService = digitalTwinPointsService;
        }

        public async Task<ILiveDataService> GetLiveDataServiceAsync(Guid siteId)
        {
            return _digitalTwinLiveDataService;
        }
	}
}
