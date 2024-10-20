using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Willow.TwinLifecycleManagement.Web.APITests
{
	public class MockingEndpoints
	{
		public static string baseDir = Directory.GetCurrentDirectory() + "/TestData/";
		public static void MockImportSearchID(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ImportSearchGetId_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/import/search")
					.WithParam("Id", "5a6545e9-e7a9-41cc-bda0-b6907a1a0be0")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockImportSearchID_NotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/import/search")
					.WithParam("Id", new ExactMatcher("5a6545e9-e7a9-41cc-bda0-b6907a1a0be1"))
					.WithParam("FullDetails",new ExactMatcher("True"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody("[]")
				);
		}
		public static void MockImportSearch(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ImportSearch_response.json";

			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/import/search")
					.WithParam("FullDetails", "True")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockCancelJobNoJobId(WireMockServer server)
		{

			server
				.Given(
					Request.Create()
					.WithPath("/import/search")
					.WithParam("Id", "cancel")
					.WithParam("FullDetails", "True")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Job not found")
				);
		}
		public static void MockImportCancelJob(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/import/cancel/01bb0e04-ccd0-43e5-b776-e7f2ae9b1485")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
				);
		}

		public static void MockImportCancelJob_NotFound(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "cancelJobNotFound.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/import/cancel/01bb0e04-ccd0-43e4-b776-e7f2ae9b1485")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockFileImporterPostTwinsWithRelationships(WireMockServer server)
		{
			string body;
			string mockResponse;
			string path = baseDir + "fileImporterPostTwinsWIthRelationships_response.json";
			string path2 = baseDir + "fileImporterPostTwinsWithRelationships_body.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			using (var r = new StreamReader(path2))
			{
				body = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/import/twins")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockFileImporterPostTwinsNoRelationshipsCSV(WireMockServer server)
		{
			string body;
			string mockResponse;
			string path = baseDir + "fileImporterNoRelationships_response.json";
			string path2 = baseDir + "fileImporterNoRelationships_body.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			using (var r = new StreamReader(path2))
			{
				body = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("NoRelationships"))
					.WithPath("/import/twins")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockFileImporterPostTwinsNoTwins(WireMockServer server)
		{
			string body;
			string path2 = baseDir + "fileImporterNoTwins_body.json";

			using (var r = new StreamReader(path2))
			{
				body = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("NoTwins"))
					.WithPath("/import/twins")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "application/json")
				);
		}


		public static void MockRelationships_noRelationships(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/relationships")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Relationships are required")
				);
		}

		public static void MockGitImporterRealEstateNoUserData(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "gitImporterValidJobStatus.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models/upgrade/repo")
					.UsingPost()
					.WithHeader("User-Data", new ExactMatcher("GitTest")
					))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockGitImporterRailWithUserData(WireMockServer server)
		{
			string req = "[{\"Owner\":\"WillowInc\",\"Repository\":\"opendigitaltwins-rail\",\"Ref\":\"\",\"Path\":\"Ontology\",\"Submodules\":null}]";
			string mockResponse;
			string path = baseDir + "gitImporterRailWithUserData.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models/upgrade/repo")
					.WithBody(req)
					.UsingPost()
					.WithHeader("User-Data", new ExactMatcher("RailTest")
					))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockPostModel(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "getModel_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models?includeModelDefinitions=true")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockPostModel_NoModels(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Models are required")
				);
		}

		public static void MockPostModel_NoID(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "postModel_no_id_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models")
					.UsingPost()
					.WithBody("No iD body"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Missing ids in models")
				);
		}

		public static void MockFileImporterGetModels(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ModelsGetModels.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models")
					.WithParam("includeModelDefinitions", new ExactMatcher("true"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		
		public static void MockFileImporterNoModels(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ModelsGetModels.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockModelsGetModel(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ModelsGetModel.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models/guid123")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockModelsGetModel_NotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models/guid124")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockCreateModels(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models/createmodels")
					.UsingPost()
					.WithBody("Models placeholder"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockCreateModels_ErrorParsing(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models/createmodels")
					.UsingPost()
					.WithBody("Parsing Error"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Parsing error message")
				);
		}

		public static void MockCreateModels_NoModels(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models/createmodels")
					.UsingPost())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Not FOund error message")
				);
		}
		public static void MockDeleteAllModels(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteAllModels_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}

			var httpRequestBody = new { DeleteAll = true, IncludeDependencies = false };
			string body = JsonConvert.SerializeObject(httpRequestBody);
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteAllModels"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/models")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteAllModelsNoUserData(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteAllModelsNoUserData_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}

			var httpRequestBody = new { DeleteAll = true, IncludeDependencies = false };
			string body = JsonConvert.SerializeObject(httpRequestBody);
			server
				.Given(
					Request.Create()
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/models")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteTwinsBySiteId(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteTwinsBySiteId_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}

			string body = "{\"DeleteAll\":true,\"Filters\":{\"siteID\":\"uidDTHP\"}}";
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteTwinsWithUserData"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteTwinsBySiteIdWithoutUserData(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteTwinsBySiteIdWithoutUserData_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			string body = "{\"DeleteAll\":true,\"Filters\":{\"siteID\":\"uidDTNUD\"}}";
			server
				.Given(
					Request.Create()
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockDeleteTwinsByFileXLSXnoRelationships(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteTwinsByFileXLSXnoRelationships_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			string body = "{\"DeleteAll\":false,\"TwinIds\":[\"AXA-STO-GFR_B_SS1_003\"],\"Filters\":{}}";
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteTwinsByFileXLSXnoRelationships"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteTwinsByFileCSVwithRelationships(WireMockServer server)
		{

			string mockResponse;
			string path = baseDir + "MockDeleteTwinsByFileCSVwithRelationships_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteTwinsByFileCSVFileWithRelationships"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteTwinsByFileRelationshipsOnly(WireMockServer server)
		{

			string mockResponse;
			string path = baseDir + "MockDeleteTwinsByFileRelationshipsOnly_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteTwinsByFileOnlyRelationships"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/relationships")
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteTwinsNoTwinsNoRelationships(WireMockServer server)
		{

			string body = "{\"DeleteAll\":false,\"TwinIds\":[]}";
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteTwinsNoTwins"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Provide target relationships to delete")
				);
		}
		public static void MockDeleteTwinsByFileNoSiteId(WireMockServer server)
		{

			string body = "{\"DeleteAll\":false,\"TwinIds\":[],\"RelationshipIds\":[]}";
			server
				.Given(
					Request.Create()
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(400)
					.WithHeader("Content-Type", "text/json")
					.WithBody("Provide target relationships to delete")
				);
		}
		public static void MockDeleteAllTwinsWithUserDataAndUserId(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteAllTwinsWithUserDataAndUserId.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			var httpRequestBody = new { DeleteAll = true };
			string body = JsonConvert.SerializeObject(httpRequestBody);
			server
				.Given(
					Request.Create()
					.WithHeader("User-Data", new ExactMatcher("DeleteAllTwinsWithUserData"))
					.WithHeader("User-Id", new ExactMatcher("DeleteAllTwinsWithUserDataAndUserId"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockDeleteAllTwinsWithOutUserDataAndWithUserId(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "MockDeleteAllTwinsWithOutUserDataAndWIthUserId.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			var httpRequestBody = new { DeleteAll = true };
			string body = JsonConvert.SerializeObject(httpRequestBody);
			server
				.Given(
					Request.Create()
					.WithHeader("User-Id", new ExactMatcher("DeleteAllTwinsWithoutUserData"))
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithPath("/import/twins")
					.WithBody(body)
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}
		public static void MockModelsDeleteModel_ModelNotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/models/guid124")
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
				);
		}
		public static void MockExportTwinsGetModels(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "ExportTwinsGetAllModels.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/models")
					.WithParam("includeModelDefinitions", new ExactMatcher("true"))
					.WithParam("includeTwinCount", new ExactMatcher("true"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithBody(mockResponse)
				);
		}
		public static void MockExportTwinsNoModelsId(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "exportTwinsNoModelsIdReturnTwins.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins")
					.WithParam("exactModelMatch",new ExactMatcher("True"))
					.WithParam("includeRelationships", new ExactMatcher("False"))
					.WithParam("includeIncomingRelationships", new ExactMatcher("False"))
					.WithParam("pageSize", new ExactMatcher("10000"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json; charset=utf-8")
				.WithBody(mockResponse)

				);
		}

		public static void MockExportTwinsMultipleModels(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "exportTwinsMultipleModelsReturnTwins.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins")
					.WithParam("exactModelMatch", new ExactMatcher("True"))
					.WithParam("includeRelationships", new ExactMatcher("False"))
					.WithParam("includeIncomingRelationships", new ExactMatcher("False"))
					.WithParam("pageSize", new ExactMatcher("10000"))
					.WithParam("modelId", new ExactMatcher("dtmi:com:willowinc:Busway"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithBody(mockResponse)
				);
		}
		public static void MockExportTwinsLocationFiltering(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "exportTwinsLocationFilteringReturnTwins.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins")
					.WithParam("locationId",new ExactMatcher("4e5fc229-ffd9-462a-882b-16b4a63b2a8a"))
					.WithParam("exactModelMatch", new ExactMatcher("False"))
					.WithParam("includeRelationships", new ExactMatcher("True"))
					.WithParam("includeIncomingRelationships", new ExactMatcher("True"))
					.WithParam("pageSize", new ExactMatcher("10000"))
					.WithParam("modelId", new ExactMatcher("dtmi:com:willowinc:ActiveElectricalPowerSensor"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "application/json; charset=utf-8")
					.WithBody(mockResponse)
				);
		}
		public static void MockExportTwinsLocationWithZeroTwins(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins")
					.WithParam("locationId", new ExactMatcher("incorrect"))
					.WithParam("exactModelMatch", new ExactMatcher("False"))
					.WithParam("includeRelationships", new ExactMatcher("True"))
					.WithParam("includeIncomingRelationships", new ExactMatcher("True"))
					.WithParam("pageSize", new ExactMatcher("10000"))
					.WithParam("modelId", new ExactMatcher("dtmi:com:willowinc:ActiveElectricalPowerSensor"))
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "application/json; charset=utf-8")
				);
		}

		public static void MockTwinsGetTwins(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "getTwins.json";

			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins/")
					.UsingGet()
					)
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				); ;
		}

		public static void MockTwinsPatchTwin(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins/PatchTwin/guid123")
					.UsingPatch()
					.WithBody("placeholder for twins patch"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockTwinsPatchTwin_NotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins/PatchTwin/guid124")
					.UsingPatch()
					.WithBody("placeholder for twins patch"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockTwinsGetTwinById(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "getTwinById_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins/guid123")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				); ;
		}

		public static void MockTwinsGetTwinById_NotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins/guid124")
					.UsingGet())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockTwinsUpdateTwin(WireMockServer server)
		{
			string mockResponse;
			string path = baseDir + "updateTwin_response.json";
			using (var r = new StreamReader(path))
			{
				mockResponse = r.ReadToEnd();
			}
			server
				.Given(
					Request.Create()
					.WithPath("/twins/UpdateTwin/guid123")
					.UsingPut()
					.WithBody("placeholder for twins update"))
				.RespondWith(
					Response.Create()
					.WithStatusCode(200)
					.WithHeader("Content-Type", "text/json")
					.WithBody(mockResponse)
				);
		}

		public static void MockTwinsDeleteTwin(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins/guid123")
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(204)
					.WithHeader("Content-Type", "text/json")
				);
		}

		public static void MockTwinsDeleteTwin_TwinNotFound(WireMockServer server)
		{
			server
				.Given(
					Request.Create()
					.WithPath("/twins/guid124")
					.UsingDelete())
				.RespondWith(
					Response.Create()
					.WithStatusCode(404)
					.WithHeader("Content-Type", "text/json")
				);
		}
	}
}
