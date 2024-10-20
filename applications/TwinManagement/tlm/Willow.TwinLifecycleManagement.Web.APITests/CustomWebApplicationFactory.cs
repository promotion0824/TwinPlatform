using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.Api.Authentication;
using WireMock.Server;

namespace Willow.TwinLifecycleManagement.Web.APITests
{
    public class CustomWebApplicationFactory<TEntryPoint>
		: WebApplicationFactory<Program> where TEntryPoint : Program
	{

		public WireMockServer twinsApi;
		public WireMockServer graphApi;
		public CustomWebApplicationFactory()
		{
			twinsApi = WireMockServer.StartWithAdminInterface(3333);
			var logEntries = twinsApi.LogEntries;
			MockingEndpoints.MockExportTwinsGetModels(twinsApi);
			MockingEndpoints.MockExportTwinsNoModelsId(twinsApi);
			MockingEndpoints.MockExportTwinsMultipleModels(twinsApi);
			MockingEndpoints.MockExportTwinsLocationFiltering(twinsApi);
			MockingEndpoints.MockExportTwinsLocationWithZeroTwins(twinsApi);
			MockingEndpoints.MockImportSearchID(twinsApi);
			MockingEndpoints.MockImportSearchID_NotFound(twinsApi);
			MockingEndpoints.MockImportCancelJob(twinsApi);
			MockingEndpoints.MockImportSearch(twinsApi);
			MockingEndpoints.MockCancelJobNoJobId(twinsApi);
			MockingEndpoints.MockImportCancelJob_NotFound(twinsApi);
			MockingEndpoints.MockFileImporterPostTwinsWithRelationships(twinsApi);
			MockingEndpoints.MockFileImporterPostTwinsNoTwins(twinsApi);
			MockingEndpoints.MockFileImporterPostTwinsNoRelationshipsCSV(twinsApi);
			MockingEndpoints.MockFileImporterGetModels(twinsApi);
			MockingEndpoints.MockGitImporterRealEstateNoUserData(twinsApi);
			MockingEndpoints.MockGitImporterRailWithUserData(twinsApi);
			MockingEndpoints.MockRelationships_noRelationships(twinsApi);
			MockingEndpoints.MockDeleteAllModels(twinsApi);
			MockingEndpoints.MockDeleteAllModelsNoUserData(twinsApi);
			MockingEndpoints.MockDeleteTwinsBySiteId(twinsApi);
			MockingEndpoints.MockDeleteTwinsBySiteIdWithoutUserData(twinsApi);
			MockingEndpoints.MockDeleteAllTwinsWithUserDataAndUserId(twinsApi);
			MockingEndpoints.MockDeleteAllTwinsWithOutUserDataAndWithUserId(twinsApi);
			MockingEndpoints.MockDeleteTwinsByFileXLSXnoRelationships(twinsApi);
			MockingEndpoints.MockDeleteTwinsByFileCSVwithRelationships(twinsApi);
			MockingEndpoints.MockDeleteTwinsNoTwinsNoRelationships(twinsApi);
			MockingEndpoints.MockDeleteTwinsByFileNoSiteId(twinsApi);
			MockingEndpoints.MockDeleteTwinsByFileRelationshipsOnly(twinsApi);
			MockingEndpoints.MockPostModel(twinsApi);
			MockingEndpoints.MockPostModel_NoModels(twinsApi);
			MockingEndpoints.MockPostModel_NoID(twinsApi);
			MockingEndpoints.MockModelsGetModel(twinsApi);
			MockingEndpoints.MockModelsGetModel_NotFound(twinsApi);
			MockingEndpoints.MockCreateModels(twinsApi);
			MockingEndpoints.MockCreateModels_ErrorParsing(twinsApi);
			MockingEndpoints.MockCreateModels_NoModels(twinsApi);
			MockingEndpoints.MockModelsDeleteModel_ModelNotFound(twinsApi);
			MockingEndpoints.MockTwinsDeleteTwin(twinsApi);
			MockingEndpoints.MockTwinsDeleteTwin_TwinNotFound(twinsApi);
			MockingEndpoints.MockTwinsUpdateTwin(twinsApi);
			MockingEndpoints.MockTwinsGetTwinById(twinsApi);
			MockingEndpoints.MockTwinsGetTwinById_NotFound(twinsApi);
			MockingEndpoints.MockTwinsUpdateTwin(twinsApi);
			MockingEndpoints.MockTwinsPatchTwin(twinsApi);
			MockingEndpoints.MockTwinsPatchTwin_NotFound(twinsApi);
			MockingEndpoints.MockTwinsGetTwins(twinsApi);
			//Console.ReadLine();
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureAppConfiguration(config =>
			{
				var integrationConfig = new ConfigurationBuilder()
					.AddJsonFile("appsettings.TestApi.json")
					.Build();

				config.AddConfiguration(integrationConfig);
			});
			builder
				.UseEnvironment("TestApi")
				.UseTestServer()
				.ConfigureTestServices(collection =>
				{
					collection
						.AddAuthentication(x =>
						{
							x.DefaultAuthenticateScheme = "FakeBearer";
							x.DefaultChallengeScheme = "FakeBearer";
						})
						.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
							"FakeBearer", options => { }); 
					collection.AddControllers().AddApplicationPart(typeof(Program).Assembly);
					collection.AddSingleton<IClientCredentialTokenService, FakeClientCredentialService>();
				});
		}
	}
}
