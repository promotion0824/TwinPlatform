using LazyCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Requests;
using SiteCore.Services.DigitalTwinCore;
using SiteCore.Services.ImageHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Infrastructure;
using NotFoundException = Willow.ExceptionHandling.Exceptions.NotFoundException;

namespace SiteCore.Services
{
    public class SiteService : ISiteService
    {
        private const string SitesCacheKey = "SiteCore.ServicesGetAllSites";
        private const string SitesPreferencesCacheKey = "SiteCore.ServicesGetSitesPreferences";
        private const int CacheDurationInHours = 1;

        private readonly SiteDbContext _dbContext;
        private readonly IImageHubService _imageHub;
        private readonly IDigitalTwinCoreApiService _digitalTwinCoreApi;
        private readonly IAppCache _appCache;

        public SiteService(
            SiteDbContext dbContext,
            IImageHubService imageHub,
            IDigitalTwinCoreApiService digitalTwinCoreApi,
            IAppCache appCache)
        {
            _dbContext = dbContext;
            _imageHub = imageHub;
            _digitalTwinCoreApi = digitalTwinCoreApi;
            _appCache = appCache;
        }

        public async Task<List<Site>> GetAllSites()
        {
            var sites = await _appCache.GetOrAddAsync(SitesCacheKey, async entry =>
            {
                entry.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(CacheDurationInHours));

                var siteEntities = await _dbContext.Sites.ToListAsync();

                return SiteEntity.MapToDomainObjects(siteEntities);
            });

            return sites;
        }

        public async Task<List<Site>> GetAllSitesByIdsAsync(List<Guid> siteIds)
        {
            var sites = await GetAllSites();
            return sites.Where(c => siteIds.Contains(c.Id)).ToList();
        }
        public async Task<List<Site>> GetSitesForPortfolio(Guid portfolioId)
        {
            var sites = await GetAllSites();
            return sites.Where(x=> x.PortfolioId == portfolioId).ToList();
        }

        public async Task<List<Site>> GetSites(Guid customerId, Guid? portfolioId = null)
        {
            var sites = await GetAllSites();
            var siteQuery = sites.Where(s => s.CustomerId == customerId);
            if (portfolioId.HasValue)
            {
                siteQuery = siteQuery.Where(s => s.PortfolioId == portfolioId.Value);
            }

            return siteQuery.ToList();
        }

        public async Task<Site> GetSite(Guid siteId)
        {
            var sites = await GetAllSites();
            var site =  sites.FirstOrDefault(s => s.Id == siteId);
            return site ?? throw new NotFoundException(new { SiteId = siteId });
        }

        public async Task<Site> UpdateSiteLogo(Guid siteId, byte[] logoImageContent)
        {
            var siteEntity = await _dbContext.Sites.AsTracking().FirstOrDefaultAsync(c => c.Id == siteId);
            if (siteEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId });
            }

            var imageResult = await _imageHub.CreateSiteLogo(siteEntity.CustomerId, siteId, logoImageContent);
            siteEntity.LogoId = imageResult.ImageId;
            await _dbContext.SaveChangesAsync();

            RemoveSitesCache();

            return SiteEntity.MapToDomainObject(siteEntity);
        }

        public async Task<Site> CreateSite(Guid customerId, Guid portfolioId, CreateSiteRequest createSiteRequest)
        {
            var siteEntity = new SiteEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                PortfolioId = portfolioId,
                Name = createSiteRequest.Name,
                Code = createSiteRequest.Code,
                Suburb = createSiteRequest.Suburb ?? string.Empty,
                Address = createSiteRequest.Address,
                State = createSiteRequest.State,
                Postcode = string.Empty,
                Country = createSiteRequest.Country,
                NumberOfFloors = 0,
                Area = createSiteRequest.Area,
                LogoId = null,
                Latitude = createSiteRequest.Latitude,
                Longitude = createSiteRequest.Longitude,
                TimezoneId = createSiteRequest.TimeZoneId,
                Status = createSiteRequest.Status,
                Type = createSiteRequest.Type,
                ConstructionYear = createSiteRequest.ConstructionYear,
                SiteCode = createSiteRequest.SiteCode,
                SiteContactEmail = createSiteRequest.SiteContactEmail,
                SiteContactName = createSiteRequest.SiteContactName,
                SiteContactPhone = createSiteRequest.SiteContactPhone,
                SiteContactTitle = createSiteRequest.SiteContactTitle,
                CreatedDate = DateTime.UtcNow,
                DateOpened = createSiteRequest.DateOpened?.ToDateTime(TimeOnly.MinValue),
                FeaturesJson = string.Empty
            };
            _dbContext.Sites.Add(siteEntity);
            await _dbContext.SaveChangesAsync();

            RemoveSitesCache();

            return SiteEntity.MapToDomainObject(siteEntity);
        }

        public async Task<Site> UpdateSite(
            Guid customerId,
            Guid portfolioId,
            Guid siteId,
            UpdateSiteRequest updateSiteRequest)
        {
            var siteEntity = await _dbContext.Sites.AsTracking()
                .Where(x => x.Id == siteId && x.CustomerId == customerId && x.PortfolioId == portfolioId)
                .FirstOrDefaultAsync();

            if (siteEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId });
            }

            siteEntity.Name = updateSiteRequest.Name;
            siteEntity.Address = updateSiteRequest.Address;
            siteEntity.Suburb = updateSiteRequest.Suburb;
            siteEntity.Country = updateSiteRequest.Country;
            siteEntity.State = updateSiteRequest.State;
            siteEntity.TimezoneId = updateSiteRequest.TimeZoneId;
            siteEntity.Latitude = updateSiteRequest.Latitude;
            siteEntity.Longitude = updateSiteRequest.Longitude;
            siteEntity.Status = updateSiteRequest.Status;
            siteEntity.Type = updateSiteRequest.Type;
            siteEntity.Area = updateSiteRequest.Area;
            siteEntity.ConstructionYear = updateSiteRequest.ConstructionYear;
            siteEntity.SiteCode = updateSiteRequest.SiteCode;
            siteEntity.SiteContactEmail = updateSiteRequest.SiteContactEmail;
            siteEntity.SiteContactName = updateSiteRequest.SiteContactName;
            siteEntity.SiteContactPhone = updateSiteRequest.SiteContactPhone;
            siteEntity.SiteContactTitle = updateSiteRequest.SiteContactTitle;
            siteEntity.DateOpened = updateSiteRequest.DateOpened?.ToDateTime(TimeOnly.MinValue);

            await _dbContext.SaveChangesAsync();

            RemoveSitesCache();

            return SiteEntity.MapToDomainObject(siteEntity);
        }

        public async Task SoftDeleteSite(Guid siteId)
        {
            var siteEntity = await _dbContext.Sites.AsTracking()
                .Where(x => x.Id == siteId)
                .FirstOrDefaultAsync();

            if (siteEntity == null)
            {
                throw new NotFoundException(new { SiteId = siteId });
            }

            siteEntity.Status = SiteStatus.Deleted;

            await _dbContext.SaveChangesAsync();

            RemoveSitesCache();
        }

        public async Task<SitePreferences> GetSitePreferences(Guid siteId)
        {
            var sitesPreferences = await GetSitesPreferences();
            var sitePreferences = sitesPreferences.FirstOrDefault(x => x.SiteId == siteId);
            if (sitePreferences == null)
            {
                sitePreferences = new SitePreferencesEntity();
            }

            return SitePreferencesEntity.MapTo(sitePreferences);
        }

        public async Task CreateOrUpdateSitePreferences(Guid siteId, SitePreferencesRequest sitePreferencesRequest)
        {
            var sitePreferencesEntity = await _dbContext.SitePreferences
                .AsTracking()
                .Where(x => x.SiteId == siteId)
                .FirstOrDefaultAsync();

            var timeMachinePreferences = sitePreferencesRequest.TimeMachine.ValueKind == JsonValueKind.Undefined ?
                                                    JsonSerializerExtensions.Serialize("{}") :
                                                    JsonSerializerExtensions.Serialize(sitePreferencesRequest.TimeMachine);

            var moduleGroupsPreferences = sitePreferencesRequest.ModuleGroups.ValueKind == JsonValueKind.Undefined ?
                                                    JsonSerializerExtensions.Serialize("{}") :
                                                    JsonSerializerExtensions.Serialize(sitePreferencesRequest.ModuleGroups);
            if (sitePreferencesEntity == null)
            {
                var sitePreferences = new SitePreferencesEntity();
                sitePreferences.SiteId = siteId;
                sitePreferences.TimeMachine = timeMachinePreferences;
                sitePreferences.ModuleGroups = moduleGroupsPreferences;
                sitePreferences.ScopeId = string.Empty;
                await _dbContext.SitePreferences.AddAsync(sitePreferences);
            }
            else
            {
                if (timeMachinePreferences != JsonSerializerExtensions.Serialize("{}"))
                {
                    sitePreferencesEntity.TimeMachine = timeMachinePreferences;
                }

                if (moduleGroupsPreferences != JsonSerializerExtensions.Serialize("{}"))
                {
                    sitePreferencesEntity.ModuleGroups = moduleGroupsPreferences;
                }
            }

            await _dbContext.SaveChangesAsync();

            RemoveSitesPreferencesCache();
        }

        public async Task<SitePreferences> GetSitePreferencesByScope(string scopeId)
        {
            var sitesPreferences = await GetSitesPreferences();

            var sitePreferences = sitesPreferences.FirstOrDefault(x => x.ScopeId == scopeId);
            if (sitePreferences == null)
            {
                sitePreferences = new SitePreferencesEntity();
            }

            return SitePreferencesEntity.MapTo(sitePreferences);
        }


        public async Task CreateOrUpdateSitePreferencesByScope(string scopeId, SitePreferencesRequest sitePreferencesRequest)
        {
            var sitePreferencesEntity = await _dbContext.SitePreferences
                .AsTracking()
                .Where(x => x.ScopeId == scopeId)
                .FirstOrDefaultAsync();

            var timeMachinePreferences = sitePreferencesRequest.TimeMachine.ValueKind == JsonValueKind.Undefined ?
                                                    JsonSerializer.Serialize(new { }) :
                                                    JsonSerializer.Serialize(sitePreferencesRequest.TimeMachine);

            var moduleGroupsPreferences = sitePreferencesRequest.ModuleGroups.ValueKind == JsonValueKind.Undefined ?
                                                    JsonSerializer.Serialize(new { }) :
                                                    JsonSerializer.Serialize(sitePreferencesRequest.ModuleGroups);
            if (sitePreferencesEntity == null)
            {
                var sitePreferences = new SitePreferencesEntity();
                sitePreferences.SiteId = Guid.Empty;
                sitePreferences.TimeMachine = timeMachinePreferences;
                sitePreferences.ModuleGroups = moduleGroupsPreferences;
                sitePreferences.ScopeId = scopeId;
                await _dbContext.SitePreferences.AddAsync(sitePreferences);
            }
            else
            {
                if (timeMachinePreferences != JsonSerializer.Serialize(new { }))
                {
                    sitePreferencesEntity.TimeMachine = timeMachinePreferences;
                }

                if (moduleGroupsPreferences != JsonSerializer.Serialize(new { }))
                {
                    sitePreferencesEntity.ModuleGroups = moduleGroupsPreferences;
                }
            }

            await _dbContext.SaveChangesAsync();

            RemoveSitesPreferencesCache();
        }

        public async Task PopulateScopeIdToSitePreferences()
        {
            var sitePreferences = await _dbContext.SitePreferences.AsTracking().ToListAsync();
            // Check if all scopeIds are empty, only proceed when all records have no scopeId setup already.
            if (!sitePreferences.All(x => string.IsNullOrEmpty(x.ScopeId)))
            {
                return;
            }
            // Group the siteIds by customer because we have to query the digtitaltwincore at customer level
            var groupedSiteIds = await _dbContext.SitePreferences
                                .Join(_dbContext.Sites, sp => sp.SiteId, s => s.Id, (sp, s) => new { sp, s })
                                .GroupBy(x => x.s.CustomerId)
                                .Select(g => new { CustomerId = g.Key, SiteIds = g.Select(x => x.s.Id).ToList() })
                                .ToListAsync();

            var populateTasks = groupedSiteIds.Select(x => GetSitesTwinIds(x.SiteIds));
            var siteTwinIdsDict = (await Task.WhenAll(populateTasks)).SelectMany(t => t).ToDictionary(k => k.UniqueId, v => v.Id);

            await UpdateSitePreferencesScopeId(sitePreferences, siteTwinIdsDict);
        }

        /// <summary>
        /// Remove the sites cache
        /// </summary>
        public void RemoveSitesCache()
        {
            // Delete the cache so it will be refreshed
            _appCache.Remove(SitesCacheKey);
        }

        private async Task<List<TwinDto>> GetSitesTwinIds(List<Guid> siteIds)
        {
            // First siteId is used to determine the Adt/Adx instance
            return await _digitalTwinCoreApi.GetTwinIdsByUniqueIdsAsync(siteIds.First(), siteIds);
        }

        private async Task UpdateSitePreferencesScopeId(List<SitePreferencesEntity> sitePreferences, Dictionary<string,string> siteTwinIdsDict)
        {
            // EF doesn't support update PK, so need to delete original row and insert a new row with the updated value(scopeId)
            var toBeDeleted = new List<SitePreferencesEntity>();
            var toBeInserted = new List<SitePreferencesEntity>();
            sitePreferences.ForEach((existingRow) =>
            {
                if (siteTwinIdsDict.TryGetValue(existingRow.SiteId.ToString(), out var twinId))
                {
                    toBeDeleted.Add(existingRow);
                    var newRow = new SitePreferencesEntity
                    {
                        SiteId = existingRow.SiteId,
                        TimeMachine = existingRow.TimeMachine,
                        ModuleGroups = existingRow.ModuleGroups,
                        ScopeId = twinId
                    };
                    toBeInserted.Add(newRow);
                }
            });

            if (toBeDeleted.Count != 0)
            {
                _dbContext.SitePreferences.RemoveRange(toBeDeleted);
                await _dbContext.SitePreferences.AddRangeAsync(toBeInserted);
                await _dbContext.SaveChangesAsync();

                RemoveSitesPreferencesCache();
            }
        }

        /// <summary>
        /// Get Cached list if sites Preferences entities
        /// </summary>
        /// <returns></returns>
        private async Task<List<SitePreferencesEntity>> GetSitesPreferences()
        {
            var sitePreferencesCached = await _appCache.GetOrAddAsync(SitesPreferencesCacheKey, async (cache) =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddHours(CacheDurationInHours));
                List<SitePreferencesEntity> sitePreferencesEntities = await _dbContext.SitePreferences.ToListAsync();

                return sitePreferencesEntities;
            });

            return sitePreferencesCached;
        }

        /// <summary>
        /// Remove the sites preferences cache
        /// </summary>
        private void RemoveSitesPreferencesCache()
        {
            // Delete the cache so it will be refreshed
            _appCache.Remove(SitesPreferencesCacheKey);
        }
    }
}
