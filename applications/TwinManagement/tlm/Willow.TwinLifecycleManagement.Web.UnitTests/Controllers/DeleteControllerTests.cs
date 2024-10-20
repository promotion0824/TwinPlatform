using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
	public class DeleteControllerTests
	{
		private DeleteController _sut;
		private Mock<IDeletionService> _deletionServiceMock;
		private Mock<ILogger<DeleteController>> _loggerMock;
		private string _userId;
		private string _userData;
		private string _siteId;
		private List<IFormFile> _formFiles;

		public DeleteControllerTests()
		{
			_deletionServiceMock = new Mock<IDeletionService>();
			_loggerMock = new Mock<ILogger<DeleteController>>();
			_userId = "testUserId";
			_userData = "testUserData";
			_siteId = "testSiteId";
			_formFiles = new List<IFormFile>()
			{
				TestDataFactory.GetFormFile("csvTestData.csv"),
				TestDataFactory.GetFormFile("excelTestData.xlsx")

			};

			_sut = new DeleteController(_deletionServiceMock.Object, _loggerMock.Object);
		}

		[Fact]
		public async void DeleteAllTwins_ShouldReturnAValidListOfJobStatuses()
		{
			_deletionServiceMock.Setup(m => m.DeleteAllTwins(_userId, false, _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var response = await _sut.AllTwins(_userId, _userData);

			Assert.IsType<OkObjectResult>(response.Result);
			Assert.NotNull((response.Result as OkObjectResult).Value);
			var result = (response.Result as OkObjectResult).Value as AdtBulkImportJob;
			_deletionServiceMock.Verify(m => m.DeleteAllTwins(_userId, false, _userData), Times.Exactly(1)); ;
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test getting AdtBulkImportJob");
		}

		[Fact]
		public async void DeleteAllTwins_ShouldThrowAnException()
		{
			_deletionServiceMock.Setup(m => m.DeleteAllTwins(_userId, false, _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.AllTwins(_userId, _userData));

			_deletionServiceMock.Verify(m => m.DeleteAllTwins(_userId, false, _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteAllRelationships_ShouldReturnAValidListOfJobStatuses()
		{
			_deletionServiceMock.Setup(m => m.DeleteAllTwins(_userId, false, _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Relationships));

			var response = await _sut.AllTwins(_userId, _userData);

			Assert.IsType<OkObjectResult>(response.Result);
			Assert.NotNull((response.Result as OkObjectResult).Value);
			var result = (response.Result as OkObjectResult).Value as AdtBulkImportJob;
			_deletionServiceMock.Verify(m => m.DeleteAllTwins(_userId, false, _userData), Times.Exactly(1)); ;
			Assert.True(result?.Target.First() == EntityType.Relationships && result.JobId == "Test getting AdtBulkImportJob");
		}

		[Fact]
		public async void DeleteAllRelationships_ShouldThrowAnException()
		{
			_deletionServiceMock.Setup(m => m.DeleteAllTwins(_userId, false, _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.AllTwins(_userId, _userData));

			_deletionServiceMock.Verify(m => m.DeleteAllTwins(_userId, false, _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteTwinsBasedOnSiteId_ShouldReturnAValidListOfJobStatuses()
		{
			_deletionServiceMock.Setup(m => m.DeleteSiteIdTwins(_siteId, _userId, _userData, false))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var response = await _sut.TwinsBasedOnSiteId(_siteId, _userId, _userData);


			Assert.IsType<OkObjectResult>(response.Result);
			Assert.NotNull((response.Result as OkObjectResult).Value);
			var result = (response.Result as OkObjectResult).Value as AdtBulkImportJob;
			_deletionServiceMock.Verify(m => m.DeleteSiteIdTwins(_siteId, _userId, _userData, false), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test getting AdtBulkImportJob");
		}

		[Fact]
		public async void DeleteTwinsBasedOnSiteId_ImportAsyncShouldThrowAnException()
		{
			_deletionServiceMock.Setup(m => m.DeleteSiteIdTwins(_siteId, _userId, _userData, false))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.TwinsBasedOnSiteId(_siteId, _userId, _userData));

			_deletionServiceMock.Verify(m => m.DeleteSiteIdTwins(_siteId, _userId, _userData, false), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteTwinsBasedOnFile_ShouldReturnAValidStatus()
		{
			_deletionServiceMock.Setup(m => m.DeleteTwinsOrRelationshipsByFile(_formFiles, false, _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var response = await _sut.TwinsOrRelationshipsBasedOnFile(_formFiles, _userData);
			var result = ((ObjectResult)response.Result).Value as AdtBulkImportJob;


			_deletionServiceMock.Verify(m => m.DeleteTwinsOrRelationshipsByFile(_formFiles, false, _userData), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test getting AdtBulkImportJob");
			Assert.Equal((int)((ObjectResult)response.Result).StatusCode, (int)HttpStatusCode.OK);
		}

		[Fact]
		public async void DeleteTwinsBasedOnFile_DeleteTwinsByFileShouldThrowAnException()
		{
			_deletionServiceMock.Setup(m => m.DeleteTwinsOrRelationshipsByFile(_formFiles, false, _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () =>
			await _sut.TwinsOrRelationshipsBasedOnFile(_formFiles, _userData));

			_deletionServiceMock.Verify(m => m.DeleteTwinsOrRelationshipsByFile(_formFiles, false, _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		#region private

		#endregion

	}
}
