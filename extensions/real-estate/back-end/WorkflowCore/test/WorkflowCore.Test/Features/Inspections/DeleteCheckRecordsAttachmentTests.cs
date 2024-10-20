using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;
using Moq.Contrib.HttpClient;
using WorkflowCore.Infrastructure.Json;

namespace WorkflowCore.Test.Features.Inspections
{
	public class DeleteCheckRecordAttachmentsTests : BaseInMemoryTest
	{
		public DeleteCheckRecordAttachmentsTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task DeleteCheckRecordsAttachment_ReturnsNoContent()
		{
			var siteId = Guid.NewGuid();
			var site = Fixture.Build<Site>().With(x => x.Id, siteId).Create();

			var checkEntities = Fixture.Build<CheckEntity>()
										.Without(c => c.LastRecord)
										.Without(c => c.LastSubmittedRecord)
										.CreateMany(1).ToList();

			var attachments = Fixture.Build<AttachmentBase>().CreateMany(1).ToList();

			var checkRecordEntities = checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
												   .With(cr => cr.InspectionId, c.InspectionId)
												   .With(cr => cr.CheckId, c.Id)
												   .With(cr => cr.Id, c.LastRecordId)
												   .With(cr => cr.Status, CheckRecordStatus.Completed)
												   .With(cr => cr.Attachments, JsonSerializer.Serialize(attachments, JsonSerializerExtensions.DefaultOptions))
												   .Create()).ToList();

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var checkRecordId = checkRecordEntities.First().Id;

				server.Arrange().GetSiteApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}").ReturnsJson(site);

				server.Arrange().GetImageHubHttpHandler()
				.SetupRequest(HttpMethod.Delete, $"{site.CustomerId}/sites/{siteId}/checkRecords/{checkRecordId}/{attachments[0].Id}")
				.ReturnsResponse(HttpStatusCode.NoContent);

				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.CheckRecords.AddRange(checkRecordEntities);
				db.SaveChanges();

				var response = await client.DeleteAsync(
					 $"sites/{siteId}/checkRecords/{checkRecordId}/attachments/{attachments[0].Id}");

				response.StatusCode.Should().Be(HttpStatusCode.NoContent);

				db = server.Assert().GetDbContext<WorkflowContext>();
				db.CheckRecords.First().Attachments.Should().Be("[]");
			}
		}
	}
}
