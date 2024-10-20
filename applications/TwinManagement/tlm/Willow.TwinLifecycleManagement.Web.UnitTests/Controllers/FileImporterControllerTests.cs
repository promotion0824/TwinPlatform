using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
	public class FileImporterControllerTests
	{
		private FileImportController _sut;
		private Mock<IFileImporterService> _importerServiceMock;
		private List<IFormFile> _formFiles;
		private string _siteId;
		private string _userData;

		public FileImporterControllerTests()
		{
			_importerServiceMock = new Mock<IFileImporterService>();
			_formFiles = new List<IFormFile>()
			{
				TestDataFactory.GetFormFile("csvTestData.csv"),
				TestDataFactory.GetFormFile("excelTestData.xlsx")

			};
			_siteId = "testSiteId";
			_userData = "testUserData";

			_sut = new FileImportController(_importerServiceMock.Object);
		}

		[Fact]
		public async void ImportTwinsAndRelationshipsAsync_ShouldReturnAValidListOfJobStatuses()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ReturnsAsync(GetAdtBulkImportJobs().First());

			var response = await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, true, false, _userData);
			var result = ((ObjectResult)response.Result).Value as AdtBulkImportJob;


			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, true, _userData, false), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test job id - Twin and Relationship");
			Assert.Equal((int)((ObjectResult)response.Result).StatusCode, (int)HttpStatusCode.OK);
		}

		[Fact]
		public async void ImportTwinsAndRelationshipsAsync_ImportAsyncShouldThrowAnException()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, true, false, _userData));

			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, true, _userData, false), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void ImportTwinsAsync_ShouldReturnAValidListOfJobStatuses()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ReturnsAsync(GetAdtBulkImportJobs().First());

			var response = await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, false, false, _userData);
			var result = ((ObjectResult)response.Result).Value as AdtBulkImportJob;


			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, false, _userData, false), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test job id - Twin and Relationship");
			Assert.Equal((int)((ObjectResult)response.Result).StatusCode, (int)HttpStatusCode.OK);
		}

		[Fact]
		public async void ImportTwinsAsync_ImportAsyncShouldThrowAnException()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, true, false, _userData));

			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, true, _userData, false), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void ImportRelationshipsAsync_ShouldReturnAValidListOfJobStatuses()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ReturnsAsync(GetAdtBulkImportJobs().First());

			var response = await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, true, true, _userData);
			var result = ((ObjectResult)response.Result).Value as AdtBulkImportJob;


			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, true, _userData, true), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.True(result?.Target.First() == EntityType.Twins && result.JobId == "Test job id - Twin and Relationship");
			Assert.Equal((int)((ObjectResult)response.Result).StatusCode, (int)HttpStatusCode.OK);
		}

		[Fact]
		public async void ImportRelationshipsAsync_ImportAsyncShouldThrowAnException()
		{
			_importerServiceMock.Setup(m => m.ImportAsync(It.IsAny<IEnumerable<IFormFile>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ThrowsAsync(new Exception("Unit test exception"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.TwinsAndRelationshipsAsync(_formFiles, _siteId, true, true, _userData));

			_importerServiceMock.Verify(m => m.ImportAsync(_formFiles, _siteId, true, _userData, true), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DocumentsAsync_ShouldThrowAnException()
		{
			_importerServiceMock.Setup(m => m.CreateFileTwinsAsync(It.IsAny<Models.CreateDocumentRequest>()))
				.ThrowsAsync(new Exception("Unit test exception"));
			var files = new List<IFormFile>();
			files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));
			files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));
			files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));

			var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.DocumentsAsync(new Models.CreateDocumentRequest() { Files = files }));

			_importerServiceMock.Verify(m => m.CreateFileTwinsAsync(It.IsAny<Models.CreateDocumentRequest>()), Times.Exactly(1));
			Assert.True(result.Message == "Unit test exception");
		}

		[Fact]
		public async void DocumentsAsync_ShouldReturnValidDocumentTwins()
		{
			var dr = new CreateDocumentResponse();
			List<CreateDocumentResponse> docs = new List<CreateDocumentResponse>();
			docs.Add(TestDataFactory.GetCreateDocumentResponse());
			_importerServiceMock.Setup(m => m.CreateFileTwinsAsync(It.IsAny<Models.CreateDocumentRequest>()))
				.ReturnsAsync(docs);
			var files = new List<IFormFile>();
			files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));

			var result = await _sut.DocumentsAsync(new Models.CreateDocumentRequest() { Files = files });

			_importerServiceMock.Verify(m => m.CreateFileTwinsAsync(It.IsAny<Models.CreateDocumentRequest>()), Times.Exactly(1));
			Assert.NotNull(result);
			Assert.IsType<ActionResult<IEnumerable<CreateDocumentResponse>>>(result);
			Assert.NotNull(result.Result);
		}

		#region private

		private IEnumerable<AdtBulkImportJob> GetAdtBulkImportJobs()
		{
			return new List<AdtBulkImportJob>()
			{

				new AdtBulkImportJob("Test job id - Twin and Relationship", EntityType.Twins),
				new AdtBulkImportJob("Test job id - Twin", EntityType.Twins),
				new AdtBulkImportJob("Test job id - Relationship", EntityType.Relationships)
			};
		}

		#endregion
	}
}
