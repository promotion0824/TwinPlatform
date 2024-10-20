using System;
using Moq;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Services
{
	public class GitImporterServiceTests
	{
		private GitImporterService _sut;
		private Mock<IModelsService> _modelsService;
		private string _userDataMock;
		private string _userIdMock;

		public GitImporterServiceTests()
		{
			_modelsService = new Mock<IModelsService>();
			_userDataMock = "Test user data";
			_userIdMock = "test_user@willow.com";
			_sut = new GitImporterService(_modelsService.Object);
		}

		[Fact]
		public async void ImportAsync_ShouldBeCalledOnceAndReturnReceived()
		{
			_modelsService.Setup(m => m.PostModelsFromGitAsync(It.IsAny<UpgradeModelsRepoRequest>(), _userDataMock, _userIdMock))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Models));

			var result = await _sut.ImportAsync(TestDataFactory.GetUpgradeModelsRepoRequest(), _userDataMock, _userIdMock);

			_modelsService.Verify(m => m.PostModelsFromGitAsync(It.IsAny<UpgradeModelsRepoRequest>(), _userDataMock, _userIdMock), Times.Exactly(1));
			Assert.IsType<AdtBulkImportJob>(result);
		}

		[Fact]
		public async void ImportModelsFromGitRepoAsync_ShouldThrowWhenThrown()
		{
			_modelsService.Setup(m => m.PostModelsFromGitAsync(It.IsAny<UpgradeModelsRepoRequest>(), _userDataMock, _userIdMock))
				.ThrowsAsync(new DivideByZeroException("test"));

			await Assert.ThrowsAsync<DivideByZeroException>(async () =>
			{
				await _sut.ImportAsync(TestDataFactory.GetUpgradeModelsRepoRequest(), _userDataMock, _userIdMock);
			});
		}
	}
}
