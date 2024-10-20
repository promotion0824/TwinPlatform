using Azure.DigitalTwins.Core;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinCore.Services
{
	public interface ITenantsService
	{
		Task<IEnumerable<TenantDto>> GetTenants(IEnumerable<Guid> siteIds, bool useAdx = false);
	}

	public class TenantsService : ITenantsService
	{
		private readonly ISiteAdtSettingsProvider _siteAdtSettingsProvider;
		private readonly IAdxHelper _adxHelper;
		private readonly IAdtApiService _adtApiService;

		public TenantsService(ISiteAdtSettingsProvider siteAdtSettingsProvider, IAdxHelper adxHelper, IAdtApiService adtApiService)
		{
			_siteAdtSettingsProvider = siteAdtSettingsProvider;
			_adxHelper = adxHelper;
			_adtApiService = adtApiService;
		}

		public async Task<IEnumerable<TenantDto>> GetTenants(IEnumerable<Guid> siteIds, bool useAdx = false)
		{
			if (siteIds == null || !siteIds.Any())
			{
				return new List<TenantDto>();
			}

			var siteSettings = await _siteAdtSettingsProvider.GetForSitesAsync(siteIds);

			return useAdx ? await GetTenantsFromAdx(siteSettings) : await GetTenantsFromAdt(siteSettings);
		}

		private async Task<IEnumerable<TenantDto>> GetTenantsFromAdt(IEnumerable<SiteAdtSettings> siteSettings)
		{
			var tenants = new List<TenantDto>();

			foreach (var settings in siteSettings)
			{
				tenants.AddRange(await GetTenantsFromAdt(settings.InstanceSettings, await GetSiteTwinId(settings.InstanceSettings, settings.SiteId)));
			}

			return tenants;
		}

		private async Task<string> GetSiteTwinId(AzureDigitalTwinsSettings settings, Guid siteId)
		{
			var query = $@"
SELECT T.$dtId
FROM DIGITALTWINS T
WHERE IS_OF_MODEL(T, 'dtmi:com:willowinc:Building;1')
AND (T.siteID = '{siteId}' OR T.siteId = '{siteId}')";

			try
			{
				return (await _adtApiService.QueryTwins<BasicDigitalTwin>(settings, query).SingleOrDefaultAsync())?.Id;
			}
			catch (AdtApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		private async Task<IEnumerable<TenantDto>> GetTenantsFromAdt(AzureDigitalTwinsSettings settings, string siteTwinId)
		{
			if (string.IsNullOrEmpty(siteTwinId))
			{
				return new List<TenantDto>();
			}

			var query = $@"
SELECT B.siteID AS SiteId, TU.$dtId AS TenantUnitId, L.$dtId AS LeaseId, T.$dtId AS TenantId, T.name AS TenantName, T.code AS TenantCode, T.uniqueID AS TenantUniqueId
FROM DIGITALTWINS MATCH (B)<-[:isPartOf*..3]-(U)-[:includedIn]->(TU)-[:hasLease]->(L)-[:leasee]->(T)
WHERE B.$dtId = '{siteTwinId}'
AND IS_OF_MODEL(U, 'dtmi:com:willowinc:Room;1')
AND IS_OF_MODEL(TU, 'dtmi:com:willowinc:TenantUnit;1')
AND IS_OF_MODEL(L, 'dtmi:com:willowinc:Lease;1')
AND IS_OF_MODEL(T, 'dtmi:com:willowinc:Company;1')";

			try
			{
				return await _adtApiService.QueryTwins<TenantDto>(settings, query).ToListAsync();
			}
			catch (AdtApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return new List<TenantDto>();
			}
		}

		private async Task<IEnumerable<TenantDto>> GetTenantsFromAdx(IEnumerable<SiteAdtSettings> siteSettings)
		{
			var databases = siteSettings.GetDatabases();

			if (!databases.Any())
			{
				return new List<TenantDto>();
			}

			var crossdatabasetwindata = AdxExtensions.CrossDatabaseTable(databases, AdxConstants.ActiveTwinsFunction);
			var modelIds = new[] { "dtmi:com:willowinc:TenantUnit;1", "dtmi:com:willowinc:Lease;1", "dtmi:com:willowinc:Company;1" };

			var query = $@"
let siteTwins = find in ({crossdatabasetwindata}) where {AdxExtensions.OrExpansion("SiteId", siteSettings.GetSites())}
| project SiteId, Id, Name, ModelId, UniqueId;

let tenantTwins = find in ({crossdatabasetwindata}) where {AdxExtensions.OrExpansion("ModelId", modelIds)}
| project SiteId, Id, Name, ModelId, UniqueId;

let tenantUnits = siteTwins
| project Id, SiteId
| join ({AdxConstants.ActiveRelationshipsFunction}
    | where Name == 'includedIn'
    | project SourceId, TargetId
    | join (tenantTwins
            | where ModelId == 'dtmi:com:willowinc:TenantUnit;1'
            | project Name, TenantUnitId = Id)
        on $left.TargetId == $right.TenantUnitId)
    on $left.Id == $right.SourceId
    | project SiteId, TenantUnitId;

let leases = tenantUnits
| join ({AdxConstants.ActiveRelationshipsFunction}
    | where Name == 'hasLease'
    | project SourceId, TargetId
    | join (tenantTwins
            | where ModelId == 'dtmi:com:willowinc:Lease;1'
            | project Name, LeaseId = Id)
        on $left.TargetId == $right.LeaseId)
    on $left.TenantUnitId == $right.SourceId
    | project SiteId, TenantUnitId, LeaseId;

leases
| join ({AdxConstants.ActiveRelationshipsFunction}
    | where Name == 'leasee'
    | project SourceId, TargetId
    | join (tenantTwins
            | where ModelId == 'dtmi:com:willowinc:Company;1'
            | project TenantName = Name, TenantId = Id, TenantUniqueId = UniqueId) 
        on $left.TargetId == $right.TenantId)
    on $left.LeaseId == $right.SourceId
    | project SiteId, TenantUnitId, LeaseId, TenantId, TenantName, TenantUniqueId;";

			using var reader = await _adxHelper.Query(databases.First(), query);
			return reader.Parse<TenantDto>().ToList();
		}
	}
}
