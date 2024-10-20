using System;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Services;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
	public class ExportControllerTests
	{
		private ExportController _controller;
		private Mock<IFileExporterService> _exporterServiceMock;

		public ExportControllerTests()
		{
			_exporterServiceMock = new Mock<IFileExporterService>();
			_controller = new ExportController(_exporterServiceMock.Object);
		}

		[Fact]
		public async void ExportTwinsAsync_ShouldReturnFile()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.ReturnsAsync(Array.Empty<byte>());

			var response = await _controller.ExportTwinsAsync(null, null, null, null, null, null);

			Assert.NotNull(response);
			Assert.IsType<FileContentResult>(response);
		}

		[Fact]
		public async void ExportTwinsAsync_ShouldThrowException()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
				.ThrowsAsync(new Exception("UT Exception"));

			var response = await Assert.ThrowsAsync<Exception>(async () => await _controller.ExportTwinsAsync(
				It.Is<string[]>(x => x == new string[0]), It.IsAny<string>(), It.Is<bool>(x => x == false), It.Is<bool>(x => x == false), It.Is<bool>(x => x == false), It.Is<bool>(x => !x)));

			Assert.Equal("UT Exception", response.Message);
			Assert.NotNull(response);
			Assert.IsType<Exception>(response);
		}

		[Fact]
		public async void ExportTwinsAsync_ShouldReturnFileForPassedParameters()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsAsync(It.IsAny<string>(), new string[] { "TestModelId" }, It.Is<bool>(x => x == true), It.Is<bool>(x => x == false), It.Is<bool>(x => x == false), It.Is<bool>(x => !x)))
				.ReturnsAsync(Array.Empty<byte>());

			var response = await _controller.ExportTwinsAsync(
				new string[] { "TestModelId" }, "1", It.Is<bool>(x => x == true), It.Is<bool>(x => x == false), It.Is<bool>(x => x == false), It.Is<bool>(x => !x));

			Assert.NotNull(response);
			Assert.IsType<FileContentResult>(response);
		}

		[Fact]
		public async void ExportTwinsByTwinIdsAsync_ShouldReturnFile()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsByTwinIdsAsync(It.IsAny<string[]>()))
				.ReturnsAsync(Array.Empty<byte>());

			var response = await _controller.ExportTwinsByTwinIdsAsync(null);

			Assert.NotNull(response);
			Assert.IsType<FileContentResult>(response);
		}

		[Fact]
		public async void ExportTwinsByTwinIdsAsync_ShouldThrowException()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsByTwinIdsAsync(It.IsAny<string[]>()))
				.ThrowsAsync(new Exception("UT Exception"));

			var response = await Assert.ThrowsAsync<Exception>(async () => await _controller.ExportTwinsByTwinIdsAsync(It.Is<string[]>(x => x == new string[0])));

			Assert.Equal("UT Exception", response.Message);
			Assert.NotNull(response);
			Assert.IsType<Exception>(response);
		}

		[Fact]
		public async void ExportTwinsByTwinIdsAsync_ShouldReturnFileForPassedParameters()
		{
			_exporterServiceMock.Setup(x => x.ExportZippedTwinsByTwinIdsAsync(new string[] { "TestTwinId" }))
				.ReturnsAsync(Array.Empty<byte>());

			var response = await _controller.ExportTwinsByTwinIdsAsync(new string[] { "TestTwinId" });

			Assert.NotNull(response);
			Assert.IsType<FileContentResult>(response);
		}
	}
}
