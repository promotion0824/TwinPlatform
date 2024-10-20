using System;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
using System.Net;
using WorkflowCore.Dto;
using System.Net.Http;
using WorkflowCore.Entities;
using System.Linq;
using System.Collections.Generic;
using WorkflowCore.Models;
using System.Text.Json;
using WorkflowCore.Infrastructure.Json;

namespace WorkflowCore.Test.Features.Inspections
{
	public class GetCheckSubmittedHistoryTests : BaseInMemoryTest
	{
		public GetCheckSubmittedHistoryTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task CheckWithCheckRecordsExist_GetCheckSubmittedHistory_ReturnThoseCheckRecords()
		{
			int count = 10;
			var utcNow = DateTime.UtcNow;
			var siteId = Guid.NewGuid();
			var site = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
			var nextEffectiveDate = utcNow.AddDays(-1);
			var attachments = Fixture.Build<AttachmentBase>().CreateMany(3).ToList();
			var workgroupEntity = Fixture.Create<WorkgroupEntity>();
			var workgroupMemberEntity = Fixture.Build<WorkgroupMemberEntity>()
											   .With(x => x.WorkgroupId, workgroupEntity.Id)
											   .Create();

			var zoneEntity = Fixture.Build<ZoneEntity>().With(x => x.SiteId, siteId).Create();

			var inspectionEntities = Fixture.Build<InspectionEntity>()
											.With(i => i.SiteId, siteId)
											.With(i => i.ZoneId, zoneEntity.Id)
                                            .Without(i => i.FrequencyDaysOfWeekJson)
                                            .Without(x => x.Zone)
                                            .Without(i => i.Checks)
											.Without(i => i.LastRecord)
											.CreateMany(1).ToList();

			var inspectionRecordEntities = inspectionEntities.Select(i => Fixture.Build<InspectionRecordEntity>()
										.With(ir => ir.InspectionId, i.Id)
										.With(ir => ir.Id, i.LastRecordId)
										.Create()).ToList();

			var checkEntities = inspectionEntities.SelectMany(i => Fixture.Build<CheckEntity>()
										.With(c => c.InspectionId, i.Id)
										.Without(c => c.LastRecord)
										.Without(c => c.LastSubmittedRecord)
										.CreateMany(3)).ToList();

			var checkRecordEntities = checkEntities.Select(c => Fixture.Build<CheckRecordEntity>()
												   .With(cr => cr.InspectionId, c.InspectionId)
												   .With(cr => cr.CheckId, c.Id)
												   .With(cr => cr.Id, c.LastRecordId)
												   .With(cr => cr.Status, CheckRecordStatus.Completed)
												   .With(cr => cr.EffectiveDate, nextEffectiveDate)
												   .With(cr => cr.Attachments, JsonSerializer.Serialize(attachments, JsonSerializerExtensions.DefaultOptions))
												   .Create()).ToList();

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().GetSiteApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}").ReturnsJson(site);

				var imagePathHelper = server.Arrange().GetImagePathHelper();
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Workgroups.Add(workgroupEntity);
				db.WorkgroupMembers.Add(workgroupMemberEntity);
				db.Zones.Add(zoneEntity);
				db.Inspections.AddRange(inspectionEntities);
				db.InspectionRecords.AddRange(inspectionRecordEntities);
				db.Checks.AddRange(checkEntities);
				db.CheckRecords.AddRange(checkRecordEntities);
				db.SaveChanges();

				var inspectionId = inspectionEntities.First().Id;
				var checkId = checkEntities.First().Id;
				var response = await client.GetAsync($"sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory/{count}");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<CheckRecordDto>>();
				var expectedCheckRecordsDto = CheckRecordDto.MapFromModels(CheckRecordEntity.MapToModels(checkRecordEntities.Where(x => x.CheckId == checkId)),
																			imagePathHelper, site.CustomerId, siteId);
				result.Should().BeEquivalentTo(expectedCheckRecordsDto, opt => opt.ComparingByMembers<JsonElement>());
			}
		}
	}
}
