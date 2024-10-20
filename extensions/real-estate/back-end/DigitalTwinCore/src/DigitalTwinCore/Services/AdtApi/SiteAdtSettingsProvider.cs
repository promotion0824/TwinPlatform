using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;
using DigitalTwinCore.Extensions;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DigitalTwinCore.Constants;

namespace DigitalTwinCore.Services.AdtApi
{
    public interface ISiteAdtSettingsProvider
    {
		Task<List<SiteAdtSettings>> GetForSitesAsync(IEnumerable<Guid> siteIds);
		Task<SiteAdtSettings> GetForSiteAsync(Guid siteId);
        Task AddSettingsForSiteAsync(Guid siteId, string instanceUri, string siteCode, string adxDatabase, CancellationToken cancellationToken);
		Task<string> GetFirstConfiguredAdxDatabaseOrDefault(IEnumerable<Guid> siteIds);

	}

    public class SiteAdtSettingsProvider : ISiteAdtSettingsProvider
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private readonly DigitalTwinDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly AzureDigitalTwinsSettings _azureDigitalTwinsSettings;
        private readonly AzureDataExplorerSettings _azureDataExplorerSettings;
        private readonly ILogger<SiteAdtSettingsProvider> _logger;

		public SiteAdtSettingsProvider(
			IMemoryCache memoryCache,
			DigitalTwinDbContext context,
			IOptions<AzureDigitalTwinsSettings> azureDigitalTwinsSettingsOptions,
            IOptions<AzureDataExplorerSettings> azureDataExplorerSettingsSettingsOptions,
            ILogger<SiteAdtSettingsProvider> logger)
		{
			_memoryCache = memoryCache;
			_context = context;
            _azureDigitalTwinsSettings = azureDigitalTwinsSettingsOptions.Value;
            _azureDataExplorerSettings = azureDataExplorerSettingsSettingsOptions.Value;
            _logger = logger;
		}

		public async Task<List<SiteAdtSettings>> GetForSitesAsync(IEnumerable<Guid> siteIds)
		{
			var settings = new List<SiteAdtSettings>();
			foreach (var siteId in siteIds)
			{
				try
				{
					settings.Add(await GetForSiteAsync(siteId));
				}
				catch (ResourceNotFoundException e)
				{
					_logger.LogWarning(e, "Could not load Azure Digital Twin settings for site {SiteId}", siteId);
				}

			}

			return settings;
		}

		public async Task<SiteAdtSettings> GetForSiteAsync(Guid siteId)
        {
            // Check if the ADT/ADX config settings exist and use them directly
            if (_azureDigitalTwinsSettings.InstanceUri != null
                && !string.IsNullOrEmpty(_azureDataExplorerSettings.DatabaseName))
            {
                return SiteAdtSettings.CreateInstance(siteId, null, _azureDigitalTwinsSettings, _azureDataExplorerSettings.DatabaseName);
            }

            try
            {
                await Semaphore.WaitAsync();

                var settings = await _memoryCache.GetOrCreateAsync($"SiteAdtSettings_{siteId}", async (c) =>
                {
                    var siteSettingEntity = await _context.SiteSettings.FindAsync(siteId);

                    if (siteSettingEntity == null)
                    {
                        throw new ResourceNotFoundException("site", siteId.ToString(), "Azure Digital Twin configuration not found for this site");
                    }

                    return SiteAdtSettings.CreateInstance(siteId, siteSettingEntity, _azureDigitalTwinsSettings);
                });
                
                return settings;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async Task AddSettingsForSiteAsync(Guid siteId, string instanceUri, string siteCode, string adxDatabase, CancellationToken cancellationToken)
        {
            var settings = new SiteSettingEntity
            {
                SiteId = siteId,
                InstanceUri = instanceUri,
                SiteCodeForModelId = siteCode,
                AdxDatabase = adxDatabase
            };

            await _context.SiteSettings.AddAsync(settings, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

		/// <summary>
		/// Get the database name for multi sites that belongs to same customer
		/// each customer has one database 
		/// </summary>
		/// <param name="siteIds"></param>
		/// <returns>database name or null if no database found</returns>
		public async Task<string> GetFirstConfiguredAdxDatabaseOrDefault(IEnumerable<Guid> siteIds)
		{
			foreach (var siteId in siteIds)
			{
				try
				{
					var setting  = await GetForSiteAsync(siteId);
					if (!string.IsNullOrEmpty(setting?.AdxDatabase))
					{
						return setting.AdxDatabase;
					}
				}
				catch (ResourceNotFoundException e)
				{
					_logger.LogWarning(e, "Could not load Azure Data Explorer settings for site {SiteId}", siteId);
				}

			}

			return null;
		}
	}
}
