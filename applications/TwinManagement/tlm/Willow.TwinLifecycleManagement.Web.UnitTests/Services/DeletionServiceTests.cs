using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Moq;
using Willow.Exceptions.Exceptions;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Helpers.Converters;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Services
{
	public class DeletionServiceTests
	{
		private DeletionService _sut;
		private Mock<ITwinsService> _twinsServiceMock;
		private Mock<IFileImporterService> _importerService;
		private string _userId;
		private string _userData;
		private string _siteId;
		private string _resourceName;
		private List<string> _twinIds;

		public DeletionServiceTests()
		{
			_twinsServiceMock = new Mock<ITwinsService>();
			_importerService = new Mock<IFileImporterService>();
			_userId = "testUserId";
			_userData = "testUserData";
			_siteId = "testSiteId";
			_resourceName = "csvTestData.csv";
			_twinIds = new List<string>()
			{
				"testId"
			};

			_sut = new DeletionService(_twinsServiceMock.Object, _importerService.Object);
		}

		[Fact]
		public async void DeleteSiteIdTwins_ShouldReturnAValidResult()
		{
			_importerService.Setup(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var result = await _sut.DeleteSiteIdTwins(_siteId, _userId, _userData, false);

			_importerService.Verify(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, _userData), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Twins, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteSiteIdTwins_ShouldReturnAValidResultForPassedNullUserData()
		{
			_importerService.Setup(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, It.Is<string>(m => m == null)))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var result = await _sut.DeleteSiteIdTwins(_siteId, _userId, It.Is<string>(m => m == null), false);

			_importerService.Verify(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, It.Is<string>(m => m == null)), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Twins, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteSiteIdTwins_ShouldThrowAnException()
		{
			_importerService.Setup(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteSiteIdTwins(_siteId, _userId, _userData, false));

			_importerService.Verify(m => m.DeleteSiteIdTwinsAsync(_siteId, _userId, _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteSiteIdRelationships_ShouldReturnAValidResult()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Relationships));

			var result = await _sut.DeleteSiteIdTwins(_siteId, _userId, _userData, true);

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Relationships, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteSiteIdRelationships_ShouldReturnAValidResultForPassedNullUserData()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), It.Is<string>(m => m == null)))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Relationships));

			var result = await _sut.DeleteSiteIdTwins(_siteId, _userId, It.Is<string>(m => m == null), true);

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), It.Is<string>(m => m == null)), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Relationships, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteSiteIdRelationships_ShouldThrowAnException()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteSiteIdTwins(_siteId, _userId, _userData, true));

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteAllTwins_ShouldReturnAValidResult()
		{
			_importerService.Setup(m => m.DeleteAllTwinsAsync(_userId, _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var result = await _sut.DeleteAllTwins(_userId, false, _userData);

			_importerService.Verify(m => m.DeleteAllTwinsAsync(_userId, _userData), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Twins, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteAllTwins_ShouldReturnAValidResultForPassedNullUserData()
		{
			_importerService.Setup(m => m.DeleteAllTwinsAsync(_userId, It.Is<string>(m => m == null)))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

			var result = await _sut.DeleteAllTwins(_userId, false);

			_importerService.Verify(m => m.DeleteAllTwinsAsync(_userId, It.Is<string>(m => m == null)), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Twins, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteAllTwins_ShouldThrowAnException()
		{
			_importerService.Setup(m => m.DeleteAllTwinsAsync(_userId, _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteAllTwins(_userId, false, _userData));

			_importerService.Verify(m => m.DeleteAllTwinsAsync(_userId, _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteAllRelationships_ShouldReturnAValidResult()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Relationships));

			var result = await _sut.DeleteAllTwins(_userId, true, _userData);

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Relationships, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteAllRelationships_ShouldReturnAValidResultForPassedNullUserData()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), It.Is<string>(m => m == null)))
				.ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Relationships));

			var result = await _sut.DeleteAllTwins(_userId, true);

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), It.Is<string>(m => m == null)), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.Equal("Test getting AdtBulkImportJob", result.JobId);
			Assert.Equal(EntityType.Relationships, result.Target.FirstOrDefault());
		}

		[Fact]
		public async void DeleteAllRelationships_ShouldThrowAnException()
		{
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteAllTwins(_userId, true, _userData));

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.IsAny<BulkDeleteRelationshipsRequest>(), _userData), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DeleteTwinsByFile_DeleteTwinsByFileAsyncShouldThrowAnException_onNoTwins()
		{
			_importerService.Setup(m => m.DeleteTwinsByFileAsync(It.IsAny<BulkDeleteTwinsRequest>(), It.IsAny<string>()))
				.ThrowsAsync(new Exception("Unit test exception"));

			IList<IFormFile> _formFiles = new List<IFormFile>()
			{
				TestDataFactory.GetFormFile(_resourceName),
				TestDataFactory.GetFormFile(_resourceName)

			};
			var result = await Assert.ThrowsAsync<BadRequestException>(async () => await _sut.DeleteTwinsOrRelationshipsByFile(_formFiles, false, _userData));

			_importerService.Verify(m => m.DeleteTwinsByFileAsync(It.IsAny<BulkDeleteTwinsRequest>(), _userData), Times.Exactly(0));
			Assert.True(result.Message == "File does not contain twins");
		}

		[Fact]
		public async void DeleteRelationshipsByFile_DeleteTwinsByFileAsyncShouldThrowAnException_onNoTwins()
		{
			var deleteReqestMocq = GetBulkDeleteRequest();
			_importerService.Setup(m => m.DeleteRelationshipsAsync(It.Is<BulkDeleteRelationshipsRequest>(m => m == deleteReqestMocq), It.IsAny<string>()))
				.ThrowsAsync(new Exception("Unit test exception"));

			IList<IFormFile> _formFiles = new List<IFormFile>()
			{
				TestDataFactory.GetFormFile(_resourceName),
				TestDataFactory.GetFormFile(_resourceName)

			};
			var result = await Assert.ThrowsAsync<BadRequestException>(async () => await _sut.DeleteTwinsOrRelationshipsByFile(_formFiles, true, _userData));

			_importerService.Verify(m => m.DeleteRelationshipsAsync(It.Is<BulkDeleteRelationshipsRequest>(m => m == deleteReqestMocq), _userData), Times.Exactly(0));
			Assert.True(result.Message == "File does not contain twins");
		}

		#region private

		private static BulkDeleteRelationshipsRequest GetBulkDeleteRequest()
		{
			return new BulkDeleteRelationshipsRequest()
			{
				DeleteAll = false,
				TwinIds = new List<string>()
			};
		}

		#endregion
	}
}
