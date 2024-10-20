using System;
using System.Collections.Generic;
using System.Text;
using DTDLParser;
using DTDLParser.Models;
using Moq;
using Willow.Model.Adt;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Services
{
	public class FileExporterServiceTests
	{
		private FileExporterService _sut;
		private Mock<ITwinsService> _twinsServiceMock;
		private Mock<IModelsService> _modelsMock;
		private TwinWithRelationships _twinWithRelationships;
		private IEnumerable<TwinWithRelationships> _twinsWithRelationships;
		private string[] _modelId;
		private string _locationId;
		private bool _exactModelMatch;
		private bool _includeRelationships;
		private bool _includeIncomingRelationships;
		private bool? _isTemplateExportOnly;
		private readonly IReadOnlyDictionary<Dtmi, DTEntityInfo> _modelsData;
		private Mock<Microsoft.Extensions.Logging.ILogger<FileExporterService>> _loggerMock;

		public FileExporterServiceTests()
		{
			_loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<FileExporterService>>(/*MockBehavior.Strict*/);
			_modelsMock = new Mock<IModelsService>();
			_twinsServiceMock = new Mock<ITwinsService>();
			_sut = new FileExporterService(_twinsServiceMock.Object, _modelsMock.Object, _loggerMock.Object);
			_twinWithRelationships = new Mock<TwinWithRelationships>().Object;
			_twinsWithRelationships = new List<TwinWithRelationships>() { _twinWithRelationships };
			_modelId = new string[] { "testModelId" };
			_locationId = "";
			_exactModelMatch = false;
			_includeRelationships = false;
			_includeIncomingRelationships = false;
			_isTemplateExportOnly = false;
			_modelsData = TestDataFactory.GetModelsData();

		}

		public async void ExportZippedTwinsAsync_ShouldReturnAValidFile()
		{
			_modelsMock.Setup(m => m.GetParsedModelsAsync()).ReturnsAsync(_modelsData);
			_twinsServiceMock.Setup(m => m.GetAllTwinsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),It.IsAny<SourceType>()))
				.ReturnsAsync(TestDataFactory.GetTestTwinsWithRelationships());

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var result = await _sut.ExportZippedTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, _isTemplateExportOnly);

			_modelsMock.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
			_twinsServiceMock.Verify(m => m.GetAllTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, SourceType.Adx), Times.Exactly(1));

			Assert.NotNull(result);
			Assert.IsType<byte[]>(result);
			Assert.True(result.Length > 0);
		}

		public async void ExportZippedTwinsAsync_GetModelsShouldThrowThrowException()
		{
			_modelsMock.Setup(m => m.GetParsedModelsAsync()).ThrowsAsync(new Exception("Unit test exception"));
			_twinsServiceMock.Setup(m => m.GetAllTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, SourceType.Adx))
				.ReturnsAsync(TestDataFactory.GetTestTwinsWithRelationships());


			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ExportZippedTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, _isTemplateExportOnly));

			_modelsMock.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
			_twinsServiceMock.Verify(m => m.GetAllTwinsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceType>()), Times.Never);

			Assert.True(result.Message == "Unit test exception");
		}

		public async void ExportZippedTwinsAsync_GetAllTwinsShouldThrowThrowException()
		{
			_modelsMock.Setup(m => m.GetParsedModelsAsync()).ReturnsAsync(_modelsData);
			_twinsServiceMock.Setup(m => m.GetAllTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, SourceType.Adx))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ExportZippedTwinsAsync(_locationId, _modelId, _exactModelMatch, _includeRelationships, _includeIncomingRelationships, _isTemplateExportOnly));

			_modelsMock.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
			_twinsServiceMock.Verify(m => m.GetAllTwinsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<SourceType>()), Times.Exactly(1));

			Assert.True(result.Message == "Unit test exception");
		}

		public async void ExportZippedTwinsByTwinIdsAsync_ShouldReturnAValidFile()
		{
			_modelsMock.Setup(m => m.GetModelAsync(It.IsAny<string>())).ReturnsAsync(It.IsAny<DTInterfaceInfo>());
			_twinsServiceMock.Setup(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false)).ReturnsAsync(TestDataFactory.GetTestTwinWithRelationships);

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var result = await _sut.ExportZippedTwinsByTwinIdsAsync(It.IsAny<string[]>());

			_modelsMock.Verify(m => m.GetModelAsync(It.IsAny<string>()), Times.Exactly(1));
			_twinsServiceMock.Verify(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false), Times.Exactly(1));

			Assert.NotNull(result);
			Assert.IsType<byte[]>(result);
			Assert.True(result.Length > 0);
		}

		public async void ExportZippedTwinsByTwinIdsAsync_GetModelsShouldThrowThrowException()
		{
			_modelsMock.Setup(m => m.GetModelAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Unit test exception"));
			_twinsServiceMock.Setup(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false)).ReturnsAsync(TestDataFactory.GetTestTwinWithRelationships);

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ExportZippedTwinsByTwinIdsAsync(It.Is<string[]>(x => x == new string[] { "TwinId1" })));

			_modelsMock.Verify(m => m.GetModelAsync(It.IsAny<string>()), Times.Exactly(1));
			_twinsServiceMock.Verify(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false), Times.Exactly(1));

			Assert.True(result.Message == "Unit test exception");
		}

		public async void ExportZippedTwinsByTwinIdsAsync_GetAllTwinsShouldThrowThrowException()
		{
			_twinsServiceMock.Setup(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false)).ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ExportZippedTwinsByTwinIdsAsync(It.Is<string[]>(x => x == new string[] { "TwinId1" })));

			_modelsMock.Verify(m => m.GetModelAsync(It.IsAny<string>()), Times.Never);
			_twinsServiceMock.Verify(m => m.GetTwinAsync(It.IsAny<string>(), SourceType.AdtQuery, false), Times.Exactly(1));

			Assert.True(result.Message == "Unit test exception");
		}

	}
}
