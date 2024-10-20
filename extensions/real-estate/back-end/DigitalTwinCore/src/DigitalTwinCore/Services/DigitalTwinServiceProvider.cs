using DigitalTwinCore.Services.AdtApi;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Services
{
    public interface IDigitalTwinServiceProvider
    {
        Task<IDigitalTwinService> GetForSiteAsync(Guid siteId);
        Task<IDigitalTwinService> GetForSitesAsync(List<Guid> siteIds);
    }

    public class DigitalTwinServiceProvider : IDigitalTwinServiceProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISiteAdtSettingsProvider _instanceSettingsProvider;
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly ILogger<DigitalTwinServiceProvider> _logger;

        public DigitalTwinServiceProvider(
            IMemoryCache memoryCache,
            ISiteAdtSettingsProvider instanceSettingsProvider,
            IDigitalTwinService digitalTwinService,
            ILogger<DigitalTwinServiceProvider> logger)
        {
            _memoryCache = memoryCache;
            _instanceSettingsProvider = instanceSettingsProvider;
            _digitalTwinService = digitalTwinService;
            _logger = logger;
        }

        public async Task<IDigitalTwinService> GetForSiteAsync(Guid siteId)
        {
            await _digitalTwinService.Load(await _instanceSettingsProvider.GetForSiteAsync(siteId), _memoryCache);
            return _digitalTwinService;
        }

        public async Task<IDigitalTwinService> GetForSitesAsync(List<Guid> siteIds)
        {
            if(siteIds==null || !siteIds.Any())
                return null;
            foreach (var siteId in siteIds)
            {
                try
                {
                   return await GetForSiteAsync(siteId);
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load Azure Digital Twin settings for site {SiteId}", siteId);
                   
                }
            }
            throw new ResourceNotFoundException("site", string.Join(',',siteIds), "Azure Digital Twin configuration not found for these sites"); ;
        }
    }
}
