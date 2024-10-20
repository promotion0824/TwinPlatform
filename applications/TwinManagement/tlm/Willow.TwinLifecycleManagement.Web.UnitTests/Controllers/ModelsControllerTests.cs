using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Willow.Model.Adt;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
	public class ModelsControllerTests
	{
		private readonly ModelsController _sut;
		private readonly Mock<IModelsService> _modelsServiceMock;

		private readonly Task<List<InterfaceTwinsInfo>> _modelIdNames;

		public ModelsControllerTests()
		{
			_modelIdNames = (Task<List<InterfaceTwinsInfo>>)TestDataFactory.GetInterfaceModelsData();
			_modelsServiceMock = new Mock<IModelsService>();
			_sut = new ModelsController(_modelsServiceMock.Object);
		}

		[Fact]
		public async void Get_ReturnsNamesOfAllModels()
		{
			_modelsServiceMock.Setup(ms => ms.GetModelsInterfaceInfoAsync(SourceType.Adx)).Returns(_modelIdNames);

			var response = await _sut.GetAllModelsAsync();
			var result = (response.Result as OkObjectResult)?.Value as Task<IEnumerable<InterfaceTwinsInfo>>;

			Assert.Equal(result.Result, _modelIdNames.Result);
		}
	}
}
