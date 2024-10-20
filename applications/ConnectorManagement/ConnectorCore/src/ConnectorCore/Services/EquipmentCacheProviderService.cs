namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class EquipmentCacheProviderService : IEquipmentCacheProviderService
    {
        private readonly IServiceScopeFactory scopeFactory;

        private readonly TimeSpan cacheLifetime;

        private readonly ConcurrentDictionary<Guid, EquipmentCache> siteCaches = new ConcurrentDictionary<Guid, EquipmentCache>();
        private readonly Dictionary<Guid, SemaphoreSlim> siteLocks = new Dictionary<Guid, SemaphoreSlim>();

        public EquipmentCacheProviderService(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            this.scopeFactory = scopeFactory;

            var cacheExpirationString = config["CacheExpiration"];
            var cacheExpirationHours = string.IsNullOrEmpty(cacheExpirationString)
                ? 1.0
                : double.Parse(cacheExpirationString);

            cacheLifetime = TimeSpan.FromHours(cacheExpirationHours);
        }

        public async Task RefreshAllAsync()
        {
            var existingCacheKeys = siteCaches.Keys.ToList();
            foreach (var existingCacheKey in existingCacheKeys)
            {
                var semaphore = GetSemaphore(existingCacheKey);
                await semaphore.WaitAsync();
                try
                {
                    siteCaches.Remove(existingCacheKey, out _);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public async Task<EquipmentCache> GetCacheAsync(Guid siteId, bool force = false)
        {
            if (siteCaches.TryGetValue(siteId, out var cache) && !force && !await IsCacheOutdated(cache))
            {
                return cache;
            }

            var semaphore = GetSemaphore(siteId);
            await semaphore.WaitAsync();
            try
            {
                if (siteCaches.TryGetValue(siteId, out cache) && !force && !await IsCacheOutdated(cache))
                {
                    return cache;
                }

                var newCache = await RecreateCacheAsync(siteId);
                siteCaches[siteId] = newCache;
                return newCache;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private SemaphoreSlim GetSemaphore(Guid siteId)
        {
            lock (siteLocks)
            {
                if (!siteLocks.TryGetValue(siteId, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    siteLocks[siteId] = semaphore;
                }

                return semaphore;
            }
        }

        private async Task<bool> IsCacheOutdated(EquipmentCache cache)
        {
            var lastImport = await GetLastImportAsync(cache.SiteId);
            var expired = DateTime.UtcNow - cache.RefreshTimestamp > cacheLifetime;
            var imported = lastImport.HasValue && lastImport.Value > cache.RefreshTimestamp;

            return expired || imported;
        }

        private async Task<DateTime?> GetLastImportAsync(Guid siteId)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var connectorRepository = scope.ServiceProvider.GetRequiredService<IConnectorsRepository>();
                return await connectorRepository.GetLastImportBySiteAsync(siteId);
            }
        }

        private async Task<EquipmentCache> RecreateCacheAsync(Guid siteId)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var equipmentRepository = scope.ServiceProvider.GetRequiredService<IEquipmentsRepository>();
                var tagsRepository = scope.ServiceProvider.GetRequiredService<ITagsRepository>();
                var categoriesRepository = scope.ServiceProvider.GetRequiredService<ITagCategoriesRepository>();

                var equipment = await equipmentRepository.GetBySiteIdAsync(siteId);
                var categories = await categoriesRepository.GetTagCategoriesAsync(true);
                var equipmentTags = await tagsRepository.GetEquipmentTagsBySiteIdAsync(siteId);

                foreach (var equipmentEntity in equipment)
                {
                    if (equipmentTags.TryGetValue(equipmentEntity.Id, out var tags))
                    {
                        equipmentEntity.Tags = tags;
                    }
                }

                var cache = new EquipmentCache(siteId, equipment, categories);

                return cache;
            }
        }
    }

    /// <summary>
    /// Represents a cache of equipment data.
    /// </summary>
    public class EquipmentCache
    {
        private static readonly CategoryEntity UncategorizedCategory = new()
        {
            Id = Guid.Empty,
            Name = "Miscellaneous",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentCache"/> class.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        /// <param name="dbEquipments">Rhe equipment entities.</param>
        /// <param name="dbCategories">The category entities.</param>
        public EquipmentCache(Guid siteId, ICollection<EquipmentEntity> dbEquipments, ICollection<CategoryEntity> dbCategories)
        {
            SiteId = siteId;
            RefreshTimestamp = DateTime.UtcNow;

            Equipments = new List<EquipmentEntity>();
            var categoriesDictionary = new Dictionary<Guid, CategoryEntity>();

            foreach (var equipmentEntity in dbEquipments)
            {
                foreach (var categoryEntity in dbCategories)
                {
                    if (categoryEntity.Tags.All(t => equipmentEntity.Tags != null && equipmentEntity.Tags.Any(et => et != null && et.Id == t.Id)))
                    {
                        if (!categoriesDictionary.ContainsKey(categoryEntity.Id))
                        {
                            categoriesDictionary.Add(categoryEntity.Id, categoryEntity);
                        }

                        equipmentEntity.Categories.Add(categoryEntity);
                    }
                }

                if (equipmentEntity.Categories.Count == 0)
                {
                    if (!categoriesDictionary.ContainsKey(UncategorizedCategory.Id))
                    {
                        categoriesDictionary.Add(UncategorizedCategory.Id, UncategorizedCategory);
                    }

                    equipmentEntity.Categories.Add(UncategorizedCategory);
                }

                Equipments.Add(equipmentEntity);
            }

            Categories = [.. categoriesDictionary.Values];
        }

        /// <summary>
        /// Gets or sets the site ID.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the cache was last refreshed.
        /// </summary>
        public DateTime RefreshTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the list of equipment entities.
        /// </summary>
        public List<EquipmentEntity> Equipments { get; set; }

        /// <summary>
        /// Gets or sets the list of category entities.
        /// </summary>
        public List<CategoryEntity> Categories { get; set; }
    }
}
