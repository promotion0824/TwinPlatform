using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

using Willow.Common;
using Willow.Data;
using Willow.KPI.Repository;
using Willow.KPI.Service;
using Willow.Platform.Models;

using PlatformPortalXL.Services;
using Microsoft.Extensions.Logging;
using Willow.ExceptionHandling.Exceptions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Features
{
    public class KPIServiceFactory : IKPIServiceFactory
    {
        private readonly IConfiguration _config;
        private readonly ConcurrentDictionary<string, IKPIService> _cache = new ConcurrentDictionary<string, IKPIService>();
        private readonly IReadRepository<Guid, Site> _siteRepo;
        private readonly IDigitalTwinApiService _dtApiService;
        private readonly IMemoryCache _viewCache;
        private readonly int _cacheDuration;
        private readonly ILogger<KPIServiceFactory> _logger;
        private readonly ILogger<KPIService> _kpiServiceLogger;

        public KPIServiceFactory(IConfiguration config,
                                 IReadRepository<Guid, Site> siteRepo,
                                 IDigitalTwinApiService dtApiService,
                                 IMemoryCache viewCache,
                                 int cacheDurationInHours,
                                 ILogger<KPIServiceFactory> logger,
                                 ILogger<KPIService> kpiServiceLogger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _siteRepo = siteRepo ?? throw new ArgumentNullException(nameof(siteRepo));
            _dtApiService = dtApiService ?? throw new ArgumentNullException(nameof(dtApiService));
            _viewCache = viewCache ?? throw new ArgumentNullException(nameof(viewCache));
            _cacheDuration = cacheDurationInHours;
            _logger = logger;
            _kpiServiceLogger = kpiServiceLogger;
        }

        public IKPIService GetService(Guid customerId)
        {
            if(_cache.TryGetValue(customerId.ToString(), out IKPIService service))
                return service;

            // We want to have at least the password retrieved from a Key Vault secret,
            // but Key Vault secrets cannot have periods in them. So we've updated this to first look
            // for variables using a colon as a separator (so container environment variables like
            // `Snowflake__Account__c04cff69-615a-42c8-8b14-6f1a189e9e31` will work - since the
            // framework translates double underscores to colons), and a secret with a name like
            // `Common--Snowflake--Password--c04cff69-615a-42c8-8b14-6f1a189e9e31`, since our
            // library translates double hyphens to colons.
            //
            // This works in with the existing App Services because they have their own way of
            // having an environment variable that references a Key Vault secret with an arbitrary name,
            // but we can't rely on that any more.
            foreach (var sep in new[] { ":", "." }) {
                var account    = _config[$"Snowflake{sep}Account{sep}{customerId}"];
                var userName   = _config[$"Snowflake{sep}UserName{sep}{customerId}"];
                var dbName     = _config[$"Snowflake{sep}Database{sep}{customerId}"];
                var schemaName = _config[$"Snowflake{sep}Schema{sep}{customerId}"];
                var password   = _config[$"Snowflake{sep}Password{sep}{customerId}"];

                if (new List<string> { account, userName, dbName, schemaName, password }.Any(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var connectionString = $"account={account};user={userName};password={password};db={dbName};schema={schemaName}";
                var repo             = new SnowflakeRepository(connectionString);
                var svc              = new KPIService(customerId, new KPIAPI(repo, schemaName), _siteRepo, _dtApiService, _viewCache, _cacheDuration, _kpiServiceLogger);

                _cache.TryAdd(customerId.ToString(), svc);

                return svc;
            }

            // We didn't find the settings with either colon or period separators, so now we throw.
            _logger.LogWarning("Snowflake configuration for customer {CustomerId} not found. "
                + "You need Snowflake:Account:{CustomerId}, Snowflake:UserName:{CustomerId}, "
                + "Snowflake:Database:{CustomerId}, Snowflake:Schema:{CustomerId}, "
                + " and Snowflake:Password:{CustomerId}",
                customerId, customerId, customerId, customerId, customerId, customerId);
            throw new NotFoundException("Snowflake configuration for customer not found");
        }
    }
}
