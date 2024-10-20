using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Services;

public class ModelsServiceTests
{
	private readonly ModelsService _sut;
	private IEnumerable<ModelResponse> _modelsResponseData;
	private Mock<IModelsClient> _modelsClientMock;
	private IEnumerable<InterfaceTwinsInfo> _modelsData;

	public ModelsServiceTests()
	{
		_modelsResponseData = TestDataFactory.GetModelResponses();
		_modelsClientMock = new Mock<IModelsClient>();
		_sut = new ModelsService(_modelsClientMock.Object);
	}

	[Fact]
	public async Task GetModelsAsync_ReturnsProcessedModels()
	{
		// TODO: Re-write this test or just nuke it - doesn't really test anything useful
		//_modelsClientMock.Setup(client => client.GetModelsAsync()).ReturnsAsync(adtModelResponse);
		//var result = await _sut.GetAllModelsAsync();
		//Assert.Equal(modelIdNameEnumerable, result);
	}
}
