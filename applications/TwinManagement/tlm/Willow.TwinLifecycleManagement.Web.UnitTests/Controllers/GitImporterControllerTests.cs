using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.Helpers.Adapters;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
	public class GitImporterControllerTests
	{
		private GitImportController _sut;
		private Mock<IGitImporterService> _importerServiceMock;
		private Mock<IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest>> _requestAdapter;

		public GitImporterControllerTests()
		{
			_importerServiceMock = new Mock<IGitImporterService>();
			_requestAdapter = new Mock<IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest>>();

			_sut = new GitImportController(_importerServiceMock.Object, _requestAdapter.Object);
		}

		[Fact]
		public async void ImportModelsFromGitRepoAsync_ShouldReturnsOkOnSuccess()
		{
			_requestAdapter.Setup(m => m.AdaptData(It.IsAny<GitRepoRequest>()))
				.Returns(() => TestDataFactory.GetUpgradeModelsRepoRequest());
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<UpgradeModelsRepoRequest>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Models));

			var response = await _sut.ModelsFromRepoAsync(TestDataFactory.GetGitRepoRequest());
			var result = response.Result as ObjectResult;

			Assert.NotNull(result);
			Assert.Equal((int)result.StatusCode, (int)HttpStatusCode.OK);
			Assert.IsType<AdtBulkImportJob>(result.Value);
		}

		[Fact]
		public async void ImportModelsFromGitRepoAsync_ShouldAdaptOnceBeforeImportOnce()
		{
			int adaptDataCount = 0;
			int importDataCount = 0;
			_requestAdapter.Setup(m => m.AdaptData(It.IsAny<GitRepoRequest>()))
				.Returns(() => TestDataFactory.GetUpgradeModelsRepoRequest())
				.Callback(() =>
				{
					adaptDataCount++;
					Assert.Equal(1, adaptDataCount);
					Assert.Equal(0, importDataCount);
				});
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<UpgradeModelsRepoRequest>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Models))
				.Callback(() =>
				{
					importDataCount++;
					Assert.Equal(1, adaptDataCount);
					Assert.Equal(1, importDataCount);
				});

			var response = await _sut.ModelsFromRepoAsync(TestDataFactory.GetGitRepoRequest());
			var result = response.Result as ObjectResult;

			Assert.NotNull(result);
			Assert.Equal((int)result.StatusCode, (int)HttpStatusCode.OK);
			Assert.IsType<AdtBulkImportJob>(result.Value);
		}

		[Fact]
		public async void ImportModelsFromGitRepoAsync_ShouldThrowWhenThrown()
		{
			_requestAdapter.Setup(m => m.AdaptData(It.IsAny<GitRepoRequest>()))
				.Throws(new DivideByZeroException("test"));
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<UpgradeModelsRepoRequest>(), It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Models));

			await Assert.ThrowsAsync<DivideByZeroException>(async () =>
			{
				await _sut.ModelsFromRepoAsync(TestDataFactory.GetGitRepoRequest());
			});
		}
	}
}
