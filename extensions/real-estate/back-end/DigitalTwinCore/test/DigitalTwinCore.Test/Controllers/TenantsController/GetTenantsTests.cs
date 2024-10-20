using AutoFixture.Xunit2;
using Azure.DigitalTwins.Core;
using DigitalTwinCore.Database;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using DigitalTwinCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Tests.Infrastructure.MockServices;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using static Workflow.Tests.ServerFixtureConfigurations;

namespace DigitalTwinCore.Test.Controllers.TenantsController
{
	public class GetTenantsTests : BaseInMemoryTest
	{
		private Mock<IAdxHelper> _adxHelperMock;
		private Mock<IAdtApiService> _adtApiServiceMock;
		private ServerFixtureConfiguration _serverFixtureConfiguration;

		public GetTenantsTests(ITestOutputHelper output) : base(output)
		{
			_adxHelperMock = new Mock<IAdxHelper>();
			_adtApiServiceMock = new Mock<IAdtApiService>();

			_serverFixtureConfiguration = ServerFixtureConfigurations.InMemoryDb;
			_serverFixtureConfiguration.MainServicePostConfigureServices = (services) =>
			{
				if (TestEnvironment.UseInMemoryDatabase)
				{
					var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
					services.ReplaceScoped(GetInMemoryOptions<DigitalTwinDbContext>(databaseName));
					services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
					services.ReplaceSingleton<IAdxDatabaseInitializer, DummyAdxInitializer>();
					services.ReplaceScoped<IDigitalTwinService, TestDigitalTwinService>();
					services.ReplaceSingleton<IAdtApiService, InMemoryAdtApiService>();
					services.ReplaceSingleton<IAssetService, MockCachelessAssetService>();
				}

				services.ReplaceScoped<IAdxHelper>(provider => _adxHelperMock.Object);
			};
		}

		[Fact]
		public async Task NoUser_GetTenants_ReturnsUnAuthorized()
		{
			using var server = CreateServerFixture(_serverFixtureConfiguration);
			using var client = server.CreateClient();

			var response = await client.GetAsync($"tenants");
			response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}

		[Fact]
		public async Task NoSites_GetTenants_ReturnsEmpty()
		{
			var userId = Guid.NewGuid();

			using var server = CreateServerFixture(_serverFixtureConfiguration);
			using var client = server.CreateClient(null, userId);

			var response = await client.GetAsync($"tenants");
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<TenantDto>>();
			Assert.False(result.Any());
		}

		[Theory]
		[AutoData]
		public async Task HasSite_GetTenants_ReturnsTenants(TenantDto[] tenants)
		{
			var userId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			foreach (var tenant in tenants)
			{
				tenant.SiteId = siteId;
			}

			using var server = CreateServerFixture(_serverFixtureConfiguration);

			var serverArrangement = server.Arrange();
			var context = serverArrangement.CreateDbContext<DigitalTwinDbContext>();
			context.SiteSettings.Add(new SiteSettingEntity { SiteId = siteId, InstanceUri = "https://localhost", AdxDatabase = "testAdxDatabase" });
			context.SaveChanges();

			var dataReader = Helpers.CreateDataReader(tenants);
			_adxHelperMock.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), default))
				.ReturnsAsync(dataReader.Object);

			using var client = server.CreateClient(null, userId);

			var response = await client.GetAsync($"tenants?useAdx=true&siteIds={siteId}");
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var result = await response.Content.ReadAsAsync<List<TenantDto>>();
			result.Should().BeEquivalentTo(tenants);
		}

		[Theory]
		[AutoData]
		public async Task Service_GetTenants_ReturnsTenants(TenantDto[] tenants)
		{
			var siteIds = tenants.Select(x => x.SiteId).ToArray();
			var siteSettings = tenants.Select(x => new SiteAdtSettings { SiteId = x.SiteId, AdxDatabase = x.SiteId.ToString() }).ToList();

			var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();

			var dataReader = Helpers.CreateDataReader(tenants);

			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings);

			_adxHelperMock.SetupSequence(x => x.Query(It.IsAny<string>(), It.IsAny<string>(), default))
				.ReturnsAsync(dataReader.Object);

			var crossdatabasetwindata = AdxExtensions.CrossDatabaseTable(siteSettings.Select(x => x.AdxDatabase), AdxConstants.ActiveTwinsFunction);
			var modelIds = new[] { "dtmi:com:willowinc:TenantUnit;1", "dtmi:com:willowinc:Lease;1", "dtmi:com:willowinc:Company;1" };

			var expectedquery = $@"
let siteTwins = find in ({crossdatabasetwindata}) where {AdxExtensions.OrExpansion("SiteId", siteIds)}
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

			var tenantsService = new TenantsService(siteAdtSettingsProvider.Object, _adxHelperMock.Object, _adtApiServiceMock.Object);

			var response = await tenantsService.GetTenants(siteIds, useAdx: true);

			_adxHelperMock.Verify(x => x.Query(It.IsAny<string>(), expectedquery, default));

			response.Should().BeEquivalentTo(tenants);
		}

		[Theory]
		[AutoData]
		public async Task FromADT_GetTenants_ReturnsTenants(TenantDto[] tenants)
		{
			var siteIds = tenants.Select(x => x.SiteId).ToArray();

			var siteSettings = tenants.Select(x => new SiteAdtSettings
			{
				SiteId = x.SiteId,
				InstanceSettings = new AzureDigitalTwinsSettings() { InstanceUri = new Uri("https://test.ca") }
			}).ToList();

			var siteAdtSettingsProvider = new Mock<ISiteAdtSettingsProvider>();

			var setupSequence = siteAdtSettingsProvider.SetupSequence(x => x.GetForSitesAsync(It.IsAny<Guid[]>()));
			setupSequence.ReturnsAsync(siteSettings);

			var siteTwinIds = tenants.Select(x => new BasicDigitalTwin() { Id = x.SiteId.ToString() });
			_adtApiServiceMock.SetupQueryTwinsSingle(siteTwinIds, @$"
SELECT T.$dtId
FROM DIGITALTWINS T
WHERE IS_OF_MODEL(T, 'dtmi:com:willowinc:Building;1')
AND (T.siteID = '::Id::' OR T.siteId = '::Id::')");

			_adtApiServiceMock.SetupQueryTwinsSingle(tenants, $@"
SELECT B.siteID AS SiteId, TU.$dtId AS TenantUnitId, L.$dtId AS LeaseId, T.$dtId AS TenantId, T.name AS TenantName, T.code AS TenantCode, T.uniqueID AS TenantUniqueId
FROM DIGITALTWINS MATCH (B)<-[:isPartOf*..3]-(U)-[:includedIn]->(TU)-[:hasLease]->(L)-[:leasee]->(T)
WHERE B.$dtId = '::SiteId::'
AND IS_OF_MODEL(U, 'dtmi:com:willowinc:Room;1')
AND IS_OF_MODEL(TU, 'dtmi:com:willowinc:TenantUnit;1')
AND IS_OF_MODEL(L, 'dtmi:com:willowinc:Lease;1')
AND IS_OF_MODEL(T, 'dtmi:com:willowinc:Company;1')");

			var tenantsService = new TenantsService(siteAdtSettingsProvider.Object, _adxHelperMock.Object, _adtApiServiceMock.Object);

			var response = await tenantsService.GetTenants(siteIds, useAdx: false);

			foreach (var tenant in tenants)
			{
				var siteTwinIdQuery = $@"
SELECT T.$dtId
FROM DIGITALTWINS T
WHERE IS_OF_MODEL(T, 'dtmi:com:willowinc:Building;1')
AND (T.siteID = '{tenant.SiteId}' OR T.siteId = '{tenant.SiteId}')";

				var tenantsQquery = $@"
SELECT B.siteID AS SiteId, TU.$dtId AS TenantUnitId, L.$dtId AS LeaseId, T.$dtId AS TenantId, T.name AS TenantName, T.code AS TenantCode, T.uniqueID AS TenantUniqueId
FROM DIGITALTWINS MATCH (B)<-[:isPartOf*..3]-(U)-[:includedIn]->(TU)-[:hasLease]->(L)-[:leasee]->(T)
WHERE B.$dtId = '{tenant.SiteId}'
AND IS_OF_MODEL(U, 'dtmi:com:willowinc:Room;1')
AND IS_OF_MODEL(TU, 'dtmi:com:willowinc:TenantUnit;1')
AND IS_OF_MODEL(L, 'dtmi:com:willowinc:Lease;1')
AND IS_OF_MODEL(T, 'dtmi:com:willowinc:Company;1')";

				_adtApiServiceMock.Verify(x => x.QueryTwins<BasicDigitalTwin>(It.IsAny<AzureDigitalTwinsSettings>(), siteTwinIdQuery), Times.Once);
				_adtApiServiceMock.Verify(x => x.QueryTwins<TenantDto>(It.IsAny<AzureDigitalTwinsSettings>(), tenantsQquery), Times.Once);
			}

			response.Should().BeEquivalentTo(tenants);
		}
	}

	public static class MockExtensions
	{
		public static void SetupQueryTwins<T>(this Mock<IAdtApiService> mock, IEnumerable<T> twinsdata, string query = null)
		{
			var twinsPage = Azure.Page<T>.FromValues(twinsdata.ToList().AsReadOnly(), continuationToken: null, Mock.Of<Azure.Response>());
			var twinsPageable = Azure.AsyncPageable<T>.FromPages(new[] { twinsPage });
			mock.Setup(x => x.QueryTwins<T>(It.IsAny<AzureDigitalTwinsSettings>(), query ?? It.IsAny<string>()))
				.Returns(twinsPageable);
		}

		public static void SetupQueryTwinsSingle<T>(this Mock<IAdtApiService> mock, IEnumerable<T> twinsdata, string query = null)
		{
			foreach (var data in twinsdata)
			{
				mock.SetupQueryTwins(new List<T>() { data }, query?.FillIn(data));
			}
		}

		public static string FillIn<T>(this string query, T data)
		{
			foreach (var prop in data.GetType().GetProperties())
			{
				query = query.Replace($"::{prop.Name}::", prop.GetValue(data)?.ToString());
			}

			return query;
		}
	}
}
