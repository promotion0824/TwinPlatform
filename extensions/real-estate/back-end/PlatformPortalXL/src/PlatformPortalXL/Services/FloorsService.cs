using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Workflow;

namespace PlatformPortalXL.Services
{
    public interface IFloorsService
    {
        Task<Floor> CreateFloorAsync(Guid siteId, CreateFloorRequest createFloorRequest);

        Task<Floor> UpdateFloorAsync(Guid siteId, Guid floorId, UpdateFloorRequest updateFloorRequest);

        Task DeleteFloorAsync(Guid siteId, Guid floorId);

        Task<Floor> UpdateFloorGeometryAsync(Guid siteId, Guid floorId, UpdateFloorGeometryRequest updateFloorGeometryRequest);

        Task<List<Floor>> GetFloorsAsync(Guid siteId, bool? hasBaseModule);

		Task<List<Floor>> GetFloorsAsync(List<Guid> floorIds);

		Task UpdateSortOrder(Guid siteId, Guid[] floorIds);

        void ClearCache(Guid siteId);

        Task<string> MapToTwinId(Guid siteId, Guid floorId);
        Task Broadcast(Guid siteId, Guid floorId, IEnumerable<string> urns);
		Task<List<Floor>> GetFloorsAsync(Guid siteId);
    }

    public class FloorsService : IFloorsService
    {
        private readonly IFloorsApiService _floorsApiService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IGeometryViewerMessagingService _geometryViewerMessagingService;

        public FloorsService(
            IFloorsApiService floorsApiService,
            IMemoryCache memoryCache,
            IDigitalTwinApiService digitalTwinApiService,
            IGeometryViewerMessagingService geometryViewerMessagingService)
        {
            _floorsApiService = floorsApiService;
            _memoryCache = memoryCache;
            _digitalTwinApiService = digitalTwinApiService ?? throw new ArgumentNullException(nameof(digitalTwinApiService));
            _geometryViewerMessagingService = geometryViewerMessagingService ?? throw new ArgumentNullException(nameof(geometryViewerMessagingService));
        }

        private static string GetCacheKey(Guid siteId, bool? hasBaseModule)
        {
            return $"floors_cache_{siteId}_{hasBaseModule}";
        }

		private static string GetCacheKey(Guid siteId)
		{
			return $"floors_cache_by_{siteId}";
		}

		private static string GetCacheKey(List<Guid> floorIds)
		{
			return $"floors_cache_{string.Join(",", floorIds)}";
		}

		public void ClearCache(Guid siteId)
        {
            var cacheKeys = new[]
            {
                GetCacheKey(siteId, null),
                GetCacheKey(siteId, false),
                GetCacheKey(siteId, true),
				GetCacheKey(siteId)
			};

            foreach (var cacheKey in cacheKeys)
            {
                _memoryCache.Remove(cacheKey);
            }
        }

        public async Task<string> MapToTwinId(Guid siteId, Guid floorId)
        {
            var twinSearchResponse = await _digitalTwinApiService.Search(new Features.Twins.TwinSearchRequest
            {
                ModelId = AdtConstants.FloorModelId,
                SiteIds = new[] { siteId }
            });

            return twinSearchResponse.Twins.FirstOrDefault(x => x.SiteId == siteId 
                && x.FloorId == (x.FloorId.HasValue ? floorId: x.FloorId))?.Id;
        }

        public async Task<string> MapToTwinIdOrDefault(Guid siteId, Guid floorId)
        {
            try
            {
                return await MapToTwinId(siteId, floorId);
            }
            catch
            {
                return floorId.ToString();
            }
        }

        public async Task Broadcast(Guid siteId, Guid floorId, IEnumerable<string> urns)
        {
            if (urns?.Any() ?? false)
            {
                var twinId = await MapToTwinIdOrDefault(siteId, floorId);

                foreach (var urn in urns)
                {
                    await _geometryViewerMessagingService.Send(siteId, twinId, urn);
                }
            }
        }

        public async Task<Floor> CreateFloorAsync(Guid siteId, CreateFloorRequest createFloorRequest)
        {
            var result = await _floorsApiService.CreateFloorAsync(siteId, createFloorRequest);

            ClearCache(siteId);

            return result;
        }

        public async Task<Floor> UpdateFloorAsync(Guid siteId, Guid floorId, UpdateFloorRequest updateFloorRequest)
        {
            var result = await _floorsApiService.UpdateFloorAsync(siteId, floorId, updateFloorRequest);

            ClearCache(siteId);

            return result;
        }

        public async Task DeleteFloorAsync(Guid siteId, Guid floorId)
        {
            await _floorsApiService.DeleteFloorAsync(siteId, floorId);

            ClearCache(siteId);
        }

        public async Task<Floor> UpdateFloorGeometryAsync(Guid siteId, Guid floorId, UpdateFloorGeometryRequest updateFloorGeometryRequest)
        {
            var result = await _floorsApiService.UpdateFloorGeometryAsync(siteId, floorId, updateFloorGeometryRequest);

            ClearCache(siteId);

            return result;
        }

        public async Task<List<Floor>> GetFloorsAsync(Guid siteId, bool? hasBaseModule)
        {
            var key = GetCacheKey(siteId, hasBaseModule);
            var floors = await _memoryCache.GetOrCreateLockedAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await GetFloorsInternalAsync(siteId, hasBaseModule);
            });

            return floors;
        }

		public async Task<List<Floor>> GetFloorsAsync(Guid siteId)
		{
			var key = GetCacheKey(siteId);
			var floors = await _memoryCache.GetOrCreateLockedAsync(key, async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
				return await _floorsApiService.GetFloorsAsync(siteId, false);
			});

			return floors;
		}

		public async Task<List<Floor>> GetFloorsAsync(List<Guid> floorIds)
		{
			var key = GetCacheKey(floorIds);
			var floors = await _memoryCache.GetOrCreateLockedAsync(key, async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
				return await _floorsApiService.GetFloorsAsync(floorIds);
			});

			return floors;
		}



		public async Task UpdateSortOrder(Guid siteId, Guid[] floorIds)
        {
            await _floorsApiService.UpdateSortOrder(siteId, floorIds);

            ClearCache(siteId);
        }
        private async Task<List<Floor>> GetFloorsInternalAsync(Guid siteId, bool? hasBaseModule)
        {
            var floors = await _floorsApiService.GetFloorsAsync(siteId, hasBaseModule);

            return floors.OrderByDescending(f => f.SortOrder).ToList();
        }
    }
}
