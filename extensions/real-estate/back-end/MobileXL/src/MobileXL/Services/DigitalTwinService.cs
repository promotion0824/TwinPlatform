using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MobileXL.Services.Apis.DirectoryApi;

namespace MobileXL.Services
{
    public interface IDigitalTwinService
    {
        Task<IAssetService> GetAssetServiceAsync(Guid siteId);
    }

    public class DigitalTwinService : IDigitalTwinService
    {
        private readonly ILogger<DigitalTwinService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IDirectoryApiService _directoryApi;
        private readonly DigitalTwinAssetService _digitalTwinAssetService;

        public DigitalTwinService(
            ILogger<DigitalTwinService> logger,
            IMemoryCache cache,
            IDirectoryApiService directoryApi,
            DigitalTwinAssetService digitalTwinAssetService)
        {
            _logger = logger;
            _cache = cache;
            _directoryApi = directoryApi;
            _digitalTwinAssetService = digitalTwinAssetService;
        }

        public async Task<IAssetService> GetAssetServiceAsync(Guid siteId)
        {
            // Yes this entire class is now pointless and will be deleted
            // very soon.
            return _digitalTwinAssetService;
        }
    }
}
