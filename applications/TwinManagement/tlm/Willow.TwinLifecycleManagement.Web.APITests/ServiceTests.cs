using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Willow.Model.Requests;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.APITests
{
	public class ServiceTests :
		IClassFixture<CustomWebApplicationFactory<Program>>
	{

		public HttpClient _client;
		public CustomWebApplicationFactory<Program> _factory;
		private string baseDir = Directory.GetCurrentDirectory() + "/TestData/";

		public ServiceTests(CustomWebApplicationFactory<Program> factory)
		{
			_factory = factory;
			_client = _factory.CreateClient(new WebApplicationFactoryClientOptions
			{
				AllowAutoRedirect = false,
				BaseAddress = new Uri("http://localhost/api/")
			});
			dynamic data = new ExpandoObject();
			data.sub = "1cfff495-cc1d-4c60-80d2-14fea5a997fb";
			data.role = new[] { "sub_role", "admin" };
			_client.SetFakeBearerToken((object)data);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImport_ShouldReturnValidJobStatuses()
		{
			string filename = baseDir + "FileImporterImportFIle.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("11111"), "siteId");
			httpContent.Add(new StringContent("true"), "includeRelationships");
			httpContent.Add(new StringContent("true"), "includeTwinProperties");
			_client.DefaultRequestHeaders.Add("User-Id", "SGrujicic@willowinc.com");
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("SGrujicic@willowinc.com.Twins.2022.08.26.16.00.07");
			content.Should().Contain("Twins");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImportCSVNoRelationships_ShouldReturnValidJobStatuses()
		{
			string filename = baseDir + "csvTestData.csv";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("22222"), "siteId");
			httpContent.Add(new StringContent("NoRelationships"), "userData");
			httpContent.Add(new StringContent("true"), "includeRelationships");
			httpContent.Add(new StringContent("true"), "includeTwinProperties");
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			//Console.ReadLine();
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("SGrujicic@willowinc.com.Twins.2022.08.31.17.07.50");
			content.Should().Contain("Twins");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImportNoSiteId_ShouldReturnBadRequest()
		{
			var httpContent = new MultipartFormDataContent();
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImportNoTwins_ShouldReturnFailedDependency()
		{
			string filename = baseDir + "/FileImporterNoTwins.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("22222"), "siteId");
			httpContent.Add(new StringContent("NoTwins"), "userData");
			httpContent.Add(new StringContent("true"), "includeRelationships");
			httpContent.Add(new StringContent("true"), "includeTwinProperties");
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.FailedDependency);
			var responseContent = await response.Content.ReadAsStringAsync();
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("Response status code does not indicate success: 400 (Bad Request).");
		}

		[Fact (Skip= "Included test for better visibility. Skipped because it needs wiremock states")]
		public async Task FileImportNoModels_ShouldReturnFailedDependency()
		{
			string filename = baseDir + "/FileImporterImportFIle.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("noModels"), "siteId");
			var response = await _client.PostAsync("FileImport/twins", httpContent);
			Console.ReadLine();
			response.Should().HaveStatusCode(HttpStatusCode.FailedDependency);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImportCorruptedFile_ShouldReturnUnprocessableEntity()
		{
			string filename = baseDir + "/corruptedFile.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("22222"), "siteId");
			httpContent.Add(new StringContent("Test"), "userData");
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GetJobStatus_ShouldReturnValidJobStatus()
		{
			var response = await _client.GetAsync("jobstatus/5a6545e9-e7a9-41cc-bda0-b6907a1a0be0");
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("\"jobId\":\"5a6545e9-e7a9-41cc-bda0-b6907a1a0be0\"");
			responseContent.Should().Contain("\"createTime\":\"2022-05-24T12:20:04.0501632Z\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GetJobStatus_ShouldReturnNotFound()
		{
			var response = await _client.GetAsync("jobstatus/5a6545e9-e7a9-41cc-bda0-b6907a1a0be1");
			response.Should().HaveStatusCode(HttpStatusCode.NotFound);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("Job not found");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FindJobStatuses_ShouldReturnValidJobStatuses()
		{	//will need more changes and adding new tcs - logic has changed
			var response = await _client.GetAsync("jobstatus/search");
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("\"jobId\":\"5a6545e9-e7a9-41cc-bda0-b6907a1a0be0\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task CancelJob_ShouldReturnOk()
		{
			var response = await _client.GetAsync("jobstatus/cancel/01bb0e04-ccd0-43e5-b776-e7f2ae9b1485");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task CancelJob_ShouldReturnNotFound()
		{
			var response = await _client.GetAsync("jobstatus/cancel/01bb0e04-ccd0-43e4-b776-e7f2ae9b1485");
			response.Should().HaveStatusCode(HttpStatusCode.FailedDependency);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("Response status code does not indicate success: 404 (Not Found).");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GitImporterPost_ShouldReturnValidJobStatus()
		{
			var json = new GitRepoRequest
			{
				FolderPath = "Building",
				BranchRef = "Latest",
				UserInfo = "GitTest"
			};
			var stringPayload = JsonConvert.SerializeObject(json);
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync("GitImport/models", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			string responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("SGrujicic@willowinc.com");
			responseContent.Should().Contain("SGrujicic@willowinc.com.Models.2022.09.02.10.48.42");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GitImporterPostNoBranchRef_ShouldReturnBadRequest()
		{
			var httpContent = new StringContent("{  \"folderPath\": \"Ontology\"}", Encoding.UTF8, "application/json");
			var response = await _client.PostAsync("GitImport/models", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			String responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("The BranchRef field is required.");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GitImporterPostNoFolderPath_ShouldReturnBadRequest()
		{
			var json = new GitRepoRequest
			{
				FolderPath = "",
				BranchRef = "Latest"
			};
			var stringPayload = JsonConvert.SerializeObject(json);
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync("GitImport/models", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("The FolderPath field is required.");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task GitImporterPostNoBody_ShouldReturnUnsupportedMediaType()
		{
			var httpContent = new StringContent("");
			var response = await _client.PostAsync("GitImport/models", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteAllModels_ShouldReturnValidJobStatus()
		{
			_client.DefaultRequestHeaders.Add("User-Data", "DeleteAllModels");
			var response = await _client.DeleteAsync("delete/models");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("SGrujicic@willowinc.com.Models.2022.09.06.17.16.28");
			responseContent.Should().Contain("Models");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteAllModelsNoUserData_ShouldReturnValidJobStatus()
		{
			var response = await _client.DeleteAsync("delete/models");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("SGrujicic@willowinc.com.Models.2022.09.06.17.16.28");
			responseContent.Should().Contain("Models");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsBySiteId_ShouldReturnValidJobStatus()
		{
			_client.DefaultRequestHeaders.Add("User-Data", "DeleteTwinsWithUserData");
			var response = await _client.DeleteAsync("delete/twins/uidDTHP");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("SGrujicic@willowinc.com.Twins.2022.09.08.09.37.18");
			responseContent.Should().Contain("\"userData\":\"DeleteTwinsWithUserData\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsBySiteIdWithoutUserData_ShouldReturnValidJobStatus()
		{
			var response = await _client.DeleteAsync("delete/twins/uidDTNUD");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("SGrujicic@willowinc.com.Twins.2022.09.08.09.37.18");
			responseContent.Should().Contain("\"userData\":\"\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsBySiteIdNoSiteId_ShouldReturnBadRequest()
		{
			_client.DefaultRequestHeaders.Add("User-Data", "DeleteTwinsBySiteIdNoSiteId");
			var response = await _client.DeleteAsync("delete/twins/");
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			var responseContent = await response.Content.ReadAsStringAsync();
			responseContent.Should().Contain("The userId field is required.");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileXLSXnoRelationships_ShouldReturnValidJobStatuses()
		{
			string filename = baseDir + "DeleteTwinsByFileXLSXnoRelationships.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("3333"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsByFileXLSXnoRelationships"), "userData");
			httpContent.Add(new StringContent("False"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("SGrujicic@willowinc.com.Relationships.2022.09.16.10.56.41");
			content.Should().Contain("\"userData\":\"DeleteTwinsByFileXLSXnoRelationships\"");
			content.Should().Contain("Relationships");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileNoSiteId_ShouldReturnBadRequest()
		{
			var response = await _client.DeleteAsync("Delete/twinsOrRelationshipsBasedOnFile/");
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("Empty formFiles");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileNoFileUploaded_ShouldReturnFailedDependency()
		{
			var httpContent = new MultipartFormDataContent();
			httpContent.Add(new StringContent("3333"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsNoTwins"), "userData");
			httpContent.Add(new StringContent("False"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("Empty formFiles");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileNoTwins_ShouldReturnFailedDependency()
		{
			string filename = baseDir + "MockDeleteTwinsByFileNoTwins.csv";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("3333"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsNoTwins"), "userData");
			httpContent.Add(new StringContent("False"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("File does not contain twins");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileCorruptedFile_ShouldReturnValidUnprocessableEntity()
		{
			string filename = baseDir + "/DeleteTwinsByFileCorruptedFile.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("11111"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsByFileCorruptedFile"), "userData");
			httpContent.Add(new StringContent("False"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("Unable to parse provided excel file");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileCSVFileWithRelationships_ShouldReturnValid()
		{
			string filename = baseDir + "/DeleteTwinsByFIleCSVwithRelationships.csv";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("11111"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsByFileCSVFileWithRelationships"), "userData");
			httpContent.Add(new StringContent("False"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("SGrujicic@willowinc.com.Relationships.2022.09.22.11.10.31");
			content.Should().Contain("DeleteTwinsByFileCSVFileWithRelationships");
			content.Should().Contain("Relationships");
		}
		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteTwinsByFileRelationshipsOnly_ShouldReturnValidJobStatuses()
		{
			string filename = baseDir + "/DeleteTwinsByFileOnlyRelationships.xlsx";
			var httpContent = new MultipartFormDataContent();
			var fileContent = new StreamContent(File.Open(filename, FileMode.Open));
			fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			httpContent.Add(fileContent, "formFiles", filename);
			httpContent.Add(new StringContent("3333"), "siteId");
			httpContent.Add(new StringContent("DeleteTwinsByFileOnlyRelationships"), "userData");
			httpContent.Add(new StringContent("True"), "deleteOnlyRelationships");

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				Content = httpContent,
				RequestUri = new Uri("Delete/twinsOrRelationshipsBasedOnFile/", UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("SGrujicic@willowinc.com.Relationships.2022.11.14.15.00.59");
			content.Should().Contain("2022-11-14T15:00:59.6492782Z");
			content.Should().Contain("Relationships");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task FileImport_NoFolderPath()
		{
			var httpContent = new MultipartFormDataContent();
			httpContent.Add(new StringContent("11111"), "siteId");
			httpContent.Add(new StringContent("true"), "includeRelationships");
			httpContent.Add(new StringContent("true"), "includeTwinProperties");
			_client.DefaultRequestHeaders.Add("User-Id", "SGrujicic@willowinc.com");
			var response = await _client.PostAsync("FileImport/ImportTwins", httpContent);
			response.Should().HaveStatusCode(HttpStatusCode.OK);

		}

		[Fact]
		public async Task GetJobStatus_NoJobId()
		{
			var response = await _client.GetAsync("jobstatus/");
			var responseContent = await response.Content.ReadAsStringAsync();
			response.Should().HaveStatusCode(HttpStatusCode.NotFound);
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteAllTwinsWithUserDataAndUserId_ShouldReturnValidJobStatuses()
		{
			_client.DefaultRequestHeaders.Add("User-Id", "DeleteAllTwinsWithUserDataAndUserId");
			_client.DefaultRequestHeaders.Add("User-Data", "DeleteAllTwinsWithUserData");
			var response = await _client.DeleteAsync("api/Delete/Twins");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("\"userData\":\"DeleteTwinsWithUserData\"");
			content.Should().Contain("Twins");
			content.Should().Contain("\"userId\":\"DeleteAllTwinsWithUserDataAndUserId\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteAllTwinsWithoutUserData_ShouldReturnInvalidJobStatuses()
		{
			_client.DefaultRequestHeaders.Add("User-Id", "DeleteAllTwinsWithoutUserData");
			var response = await _client.DeleteAsync("Delete/Twins");
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("\"userId\":\"DeleteAllTwinsWithoutUserData\"");
			content.Should().Contain("\"userData\":\"\"");
		}

		[Fact(Skip = "Error setting up ADT SDK Client - investigate")]
		public async Task DeleteAllTwinsWithoutUserID_ShouldReturnBadRequest()
		{
			_client.DefaultRequestHeaders.Add("User-Data", "DeleteAllTwinsWithoutUserID");
			var response = await _client.DeleteAsync("Delete/Twins");
			response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("One or more validation errors occurred.");
			content.Should().Contain("The userId field is required.");
		}

		[Fact(Skip = "This one will be covered in the future due to its complexity")]
		public async Task ExportTwinsModelsIdEmpty_ShouldReturnValidFile()
		{
			var stringPayload = "[]";
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var query = new Dictionary<string, string>()
			{
				["locationId"] = "",
				["exactModelMatch"] = "true",
				["includeRelationships"] = "true",
				["includeIncomingRelationships"] = "false"
			};
			var requestUriWithQuery = QueryHelpers.AddQueryString("/Export/Twins", query);

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				Content = httpContent,
				RequestUri = new Uri(requestUriWithQuery, UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
		}

		[Fact(Skip = "Error setting up httpClient before target is called - investigate")]
		public async Task ExportTwinsLocationWithTwins_ShouldReturnValidFile()
		{
			var stringPayload = "[\"dtmi:com:willowinc:ActiveElectricalPowerSensor;1\",\"dtmi:com:willowinc:AbsentState;1\"]";
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var query = new Dictionary<string, string>()
			{
				["locationId"] = "4e5fc229-ffd9-462a-882b-16b4a63b2a8a",
				["exactModelMatch"] = "false",
				["includeRelationships"] = "true",
				["includeIncomingRelationships"] = "true"
			};
			var requestUriWithQuery = QueryHelpers.AddQueryString("/Export/Twins", query);

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				Content = httpContent,
				RequestUri = new Uri(requestUriWithQuery, UriKind.Relative)
			};
			_client.DefaultRequestHeaders.Add("User-Data", "ExportTwinsLocation");
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			response.Content.Headers.ContentType.ToString().Should().Be("application/zip");
			response.Content.Headers.ContentDisposition.FileName.Should().Contain("ExportedTwins-");
			response.Content.Headers.ContentDisposition.DispositionType.ToString().Should().Be("attachment");
		}

		[Fact(Skip = "Error setting up httpClient before target is called - investigate")]
		public async Task ExportTwinsMultipleModelsOneWithTwins_ShouldReturnValidFileWithCorrectFiltering()
		{
			var stringPayload = "[\"dtmi:com:willowinc:Busway;1\"]";
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var query = new Dictionary<string, string>()
			{
				["locationId"] = "",
				["exactModelMatch"] = "true",
				["includeRelationships"] = "false",
				["includeIncomingRelationships"] = "false"
			};
			var requestUriWithQuery = QueryHelpers.AddQueryString("/Export/Twins", query);

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				Content = httpContent,
				RequestUri = new Uri(requestUriWithQuery, UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			response.Content.Headers.ContentType.ToString().Should().Be("application/zip");
			response.Content.Headers.ContentDisposition.FileName.Should().Contain("ExportedTwins-");
			response.Content.Headers.ContentDisposition.DispositionType.ToString().Should().Be("attachment");
		}

		[Fact(Skip = "Error setting up httpClient before target is called - investigate")]
		public async Task ExportTwinsModelWithZeroTwins_ShouldReturnEmptyArchive()
		{
			var stringPayload = "[\"dtmi:com:willowinc:AngleSensor;1\"]";
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var query = new Dictionary<string, string>()
			{
				["locationId"] = "",
				["exactModelMatch"] = "true",
				["includeRelationships"] = "false",
				["includeIncomingRelationships"] = "true"
			};
			var requestUriWithQuery = QueryHelpers.AddQueryString("/Export/Twins", query);

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				Content = httpContent,
				RequestUri = new Uri(requestUriWithQuery, UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.OK);
			response.Content.Headers.ContentType.ToString().Should().Be("application/zip");
			response.Content.Headers.ContentLength.Should().BeLessThan(500); // Approximate size of an archive w/o csv's
			response.Content.Headers.ContentDisposition.FileName.Should().Contain("ExportedTwins-");
			response.Content.Headers.ContentDisposition.DispositionType.ToString().Should().Be("attachment");
		}

		[Fact(Skip = "Error setting up httpClient before target is called - investigate")]
		public async Task ExportTwinsLocationWithZeroTwins_ShouldReturnNotFound()
		{
			var stringPayload = "[\"dtmi:com:willowinc:ActiveElectricalPowerSensor;1\"]";
			var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
			var query = new Dictionary<string, string>()
			{
				["locationId"] = "incorrect",
				["exactModelMatch"] = "false",
				["includeRelationships"] = "true",
				["includeIncomingRelationships"] = "true"
			};
			var requestUriWithQuery = QueryHelpers.AddQueryString("/Export/Twins", query);

			HttpRequestMessage requestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				Content = httpContent,
				RequestUri = new Uri(requestUriWithQuery, UriKind.Relative)
			};
			var response = await _client.SendAsync(requestMessage);
			response.Should().HaveStatusCode(HttpStatusCode.FailedDependency);
			String content = response.Content.ReadAsStringAsync().Result;
			content.Should().Contain("Not Found");
		}
	}
}
