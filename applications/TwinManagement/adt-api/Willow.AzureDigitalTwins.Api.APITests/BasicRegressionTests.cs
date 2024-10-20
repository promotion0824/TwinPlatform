
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.Api.Authentication;
using Willow.AzureDigitalTwins.Api.APITests.TestData;
using Willow.AzureDigitalTwins.Api.APITests.TestOrderer;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Requests;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Willow.AzureDigitalTwins.Api.APITests;

[TestCaseOrderer("Willow.AzureDigitalTwins.Api.APITests.TestOrderer.CustomTestCaseOrderer", "Willow.AzureDigitalTwins.Api.APITests")]
public class BasicRegressionTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
	private HttpClient _Client { get; set; }
	private CustomWebApplicationFactory<Program> _Factory { get; set; }
	private ILogger<BasicRegressionTests> _Logger { get; set; }

	public BasicRegressionTests(CustomWebApplicationFactory<Program> factory)
	{
		_Factory = factory;

		var loggerService = factory.Services.GetService<ILoggerFactory>();
		if (loggerService != null)
			_Logger = loggerService.CreateLogger<BasicRegressionTests>();

		_Client = _Factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false,
			HandleCookies = false
		});
		var clientCredentialTokenService = factory.Services.GetService(typeof(IClientCredentialTokenService)) as ClientCredentialTokenService;
		_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientCredentialTokenService.GetClientCredentialToken());
	}

	[Fact]
	[TestCaseOrder(1)]
	public async void GetAllModels_ShouldReturnNoModels()
	{
		_Logger.Log(LogLevel.Information, "Get All Models");
		ModelsClient modelsClient = new(_Client);

		var response = await modelsClient.GetModelsAsync();

		Assert.NotNull(response);
		Assert.Equal(0, response.Count);
	}

	[Fact]
	[TestCaseOrder(2)]
	public async void CreateModel_ShouldReturnSuccess()
	{
		_Logger.Log(LogLevel.Information, "Create Model");
		ModelsClient modelsClient = new(_Client);
		var modelDocument = TestDataProvider.GetSampleModel();

		var createModelTask = modelsClient.CreateModelsAsync(models: new List<JsonDocument>() { modelDocument });
		await createModelTask;

		Assert.Equal(TaskStatus.RanToCompletion, createModelTask.Status);
	}

	[Fact]
	[TestCaseOrder(3)]
	public async void GetModelById_ShouldReturnValidModel()
	{
		_Logger.Log(LogLevel.Information, "Get Model By Id");
		ModelsClient modelsClient = new(_Client);
		var modelDocument = TestDataProvider.GetSampleModel();
		var modelId = modelDocument.RootElement.GetProperty("@id").ToString();

		var createdModel = await modelsClient.GetModelAsync(modelId);

		Assert.NotNull(createdModel);
		Assert.Equal(modelId, createdModel.Id);

	}

	[Fact]
	[TestCaseOrder(4)]
	public async void GetAllTwins_ShouldReturnNoTwins()
	{
		_Logger.LogInformation("Get All Twins");
		TwinsClient twinsClient = new(_Client);
        GetTwinsInfoRequest gtir = new GetTwinsInfoRequest();

        var allTwins = await twinsClient.GetTwinsAsync(gtir, continuationToken: null);

		Assert.NotNull(allTwins);
		Assert.Empty(allTwins.Content);
	}

	[Fact]
	[TestCaseOrder(5)]
	public async void CreateTwinOne_ShouldReturnSuccess()
	{
		_Logger.LogInformation("Create Twin One");
		TwinsClient twinsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();

		var createdTwinOne = await twinsClient.UpdateTwinAsync(twinOne);

		Assert.NotNull(createdTwinOne);
		Assert.Equal(twinOne.Id, createdTwinOne.Id);
	}

	[Fact]
	[TestCaseOrder(6)]
	public async void GetTwinById_ShouldReturnTwin()
	{
		_Logger.LogInformation("Get Twin By One Id");
		TwinsClient twinsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();

		var retrievedTwin = await twinsClient.GetTwinByIdAsync(twinOne.Id);

		Assert.NotNull(retrievedTwin);
		Assert.Equal(twinOne.Id, retrievedTwin.Twin.Id);
	}

	[Fact]
	[TestCaseOrder(7)]
	public async void PatchTwin_ShouldReturnSuccess()
	{
		_Logger.LogInformation("Patch Twin One");
		TwinsClient twinsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();	
		var patchDocument = new Operation() { path = "/customproperties/name", op = OperationType.Replace.ToString(), value = "patched name" };

		var patchRequest =  twinsClient.PatchTwinAsync(twinOne.Id, new List<Operation>() { patchDocument});
		await patchRequest;

		Assert.Equal(TaskStatus.RanToCompletion, patchRequest.Status);
	}

	[Fact]
	[TestCaseOrder(8)]
	public async void UpdateTwinOne_ShouldReturnSuccess()
	{
		_Logger.LogInformation("Update Twin One");
		TwinsClient twinsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();
		twinOne.Contents["name"] = "updated name property";

		var upadtedTwinOne = await twinsClient.UpdateTwinAsync(twinOne);

		Assert.NotNull(upadtedTwinOne);
		Assert.Equal(twinOne.Id, upadtedTwinOne.Id);
		Assert.Equal(twinOne.Contents["name"], upadtedTwinOne.Contents["name"].ToString());
	}


	[Fact]
	[TestCaseOrder(9)]
	public async void CreateTwinTwo_ShouldReturnSuccess()
	{
		_Logger.LogInformation("Create Twin two");
		TwinsClient twinsClient = new(_Client);
		var twinTwo = TestDataProvider.GetSampleTwinTwo();

		var createdTwinTwo = await twinsClient.UpdateTwinAsync(twinTwo);

		Assert.NotNull(createdTwinTwo);
		Assert.Equal(twinTwo.Id, createdTwinTwo.Id);
	}

	[Fact]
	[TestCaseOrder(10)]
	public async void GetRelationship_ShouldReturnEmptySet()
	{
		_Logger.LogInformation("Get Incoming and Outgoing Relationship for Twin one");
		RelationshipsClient relationshipsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();

		var allRelationship = await relationshipsClient.GetRelationshipsAsync(twinOne.Id);
		var allIncomingRelationship = await relationshipsClient.GetIncomingRelationshipsAsync(twinOne.Id);
		
		Assert.NotNull(allRelationship);
		Assert.Empty(allRelationship);
		Assert.NotNull(allIncomingRelationship);
		Assert.Empty(allIncomingRelationship);
	}

	[Fact]
	[TestCaseOrder(11)]
	public async void CreateRelationshipOne_ShouldReturnRelationship()
	{
		_Logger.LogInformation("Create Relationship for Twin One");
		RelationshipsClient relationshipsClient = new(_Client);
		var relationshipOne = TestDataProvider.GetSampleRelationshipOne();

		var createdRelationshipOne = await relationshipsClient.UpsertRelationshipAsync(relationshipOne);

		Assert.NotNull(createdRelationshipOne);
		Assert.Equal(relationshipOne.Id, createdRelationshipOne.Id);
		Assert.Equal(relationshipOne.SourceId, createdRelationshipOne.SourceId);
		Assert.Equal(relationshipOne.TargetId, createdRelationshipOne.TargetId);
	}

	[Fact]
	[TestCaseOrder(12)]
	public async void GetRelationshipForTwinOne_ShouldReturnRelationship()
	{
		_Logger.LogInformation("Get Outgoing Relationship for Twin one");
		RelationshipsClient relationshipsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();

		var allRelationship = await relationshipsClient.GetRelationshipsAsync(twinOne.Id);

		Assert.NotNull(allRelationship);
		Assert.NotEmpty(allRelationship);
	}

	[Fact]
	[TestCaseOrder(13)]
	public async void CreateRelationshipTwo_ShouldReturnRelationship()
	{
		_Logger.LogInformation("Create Relationship Two");
		RelationshipsClient relationshipsClient = new(_Client);
		var relationshipTwo = TestDataProvider.GetSampleRelationshipTwo();

		var createdRelationshipTwo = await relationshipsClient.UpsertRelationshipAsync(relationshipTwo);

		Assert.NotNull(createdRelationshipTwo);
		Assert.Equal(relationshipTwo.Id, createdRelationshipTwo.Id);
		Assert.Equal(relationshipTwo.SourceId, createdRelationshipTwo.SourceId);
		Assert.Equal(relationshipTwo.TargetId, createdRelationshipTwo.TargetId);
	}

	[Fact]
	[TestCaseOrder(18)]
	public async void DeleteRelationships_ShouldReturnSuccess()
	{
		_Logger.Log(LogLevel.Information, "Delete Relationship One and Relationship Two");
		RelationshipsClient relationshipsClient = new(_Client);
		var relationshipOne = TestDataProvider.GetSampleRelationshipOne();
		var relationshipTwo = TestDataProvider.GetSampleRelationshipTwo();

		Task deleteRelOneTask = relationshipsClient.DeleteRelationshipAsync(relationshipOne.SourceId,relationshipOne.Id);
		await deleteRelOneTask;
		Task deleteRelTwoTask = relationshipsClient.DeleteRelationshipAsync(relationshipTwo.SourceId,relationshipTwo.Id);
		await deleteRelTwoTask;

		Assert.Equal(TaskStatus.RanToCompletion, deleteRelOneTask.Status);
		Assert.Equal(TaskStatus.RanToCompletion, deleteRelTwoTask.Status);
	}

	[Fact]
	[TestCaseOrder(19)]
	public async void DeleteTwins_ShouldReturnSuccess()
	{
		_Logger.Log(LogLevel.Information, "Delete Twin One and Twin Two");
		TwinsClient twinsClient = new(_Client);
		var twinOne = TestDataProvider.GetSampleTwinOne();
		var twinTwo = TestDataProvider.GetSampleTwinTwo();

		var twinIds1 = new List<string> { twinOne.Id };
		var twinIds2 = new List<string> { twinTwo.Id };

		Task deleteTwinOneTask = twinsClient.DeleteTwinsAndRelationshipsAsync(twinIds1);
		await deleteTwinOneTask;
		Task deleteTwinTwoTask = twinsClient.DeleteTwinsAndRelationshipsAsync(twinIds2);
		await deleteTwinTwoTask;

		Assert.Equal(TaskStatus.RanToCompletion, deleteTwinOneTask.Status);
		Assert.Equal(TaskStatus.RanToCompletion, deleteTwinTwoTask.Status);
	}

	[Fact]
	[TestCaseOrder(20)]
	public async void DeleteModel_ShouldReturnSuccess()
	{
		_Logger.Log(LogLevel.Information, "Delete Model One");
		ModelsClient modelsClient = new(_Client);
		var modelDocument = TestDataProvider.GetSampleModel();
		var modelId = modelDocument.RootElement.GetProperty("@id").ToString();

		Task deleteModelTask = modelsClient.DeleteModelAsync(modelId);
		await deleteModelTask;

		Assert.Equal(TaskStatus.RanToCompletion, deleteModelTask.Status);
	}
}
