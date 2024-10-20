using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Infrastructure.Json;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Inspections
{
    public class GetInspectionLastRecordTests : BaseInMemoryTest
    {
        public GetInspectionLastRecordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task LastRecordExists_GetInspectionLastRecord_ReturnsLastRecord()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
            var attachments = "[{\"id\":\"036daa7e-7597-433e-90a6-0298c872a454\",\"type\":\"image\",\"fileName\":\"FileName07d0b93e-51b4-4911-b9aa-3f87503acf0c\",\"createdDate\":\"2021-01-06T22:02:18.725Z\"},{\"id\":\"1222677f-dff5-4fa3-ab20-82ca94d62e5a\",\"type\":\"file\",\"fileName\":\"FileNamee59022a4-5a7f-467a-81a9-9999ef224352\",\"createdDate\":\"2020-04-22T00:08:46.511Z\"},{\"id\":\"2218d2c2-ce61-4f7e-98d7-5e26a37d1020\",\"type\":\"image\",\"fileName\":\"FileName31017cf6-fd0a-4f8c-a1c3-cae1c48baceb\",\"createdDate\":\"2022-10-23T10:06:59.679Z\"}]";
			var inspectionEntity = Fixture.Build<InspectionEntity>()
							  .With(x => x.SiteId, siteId)
                              .Without(i => i.FrequencyDaysOfWeekJson)
                              .Without(x => x.Zone)
                              .Without(x => x.Checks)
							  .Without(x => x.LastRecord)
							  .Create();
			var inspectionRecordEntity = Fixture.Build<InspectionRecordEntity>()
										.With(x => x.Id, inspectionEntity.LastRecordId)
										.With(x => x.SiteId, siteId)
										.With(x => x.InspectionId, inspectionEntity.Id)
										.Create();
            var checkEntities = Fixture.Build<CheckEntity>()
                                       .With(x => x.InspectionId, inspectionEntity.Id)
                                       .With(x => x.IsArchived, false)
                                       .Without(x => x.LastSubmittedRecord)
                                       .Without(x => x.LastRecord)
                                       .CreateMany()
                                       .ToList();
            var checkRecordEntities = Fixture.Build<CheckRecordEntity>()
                                             .With(x => x.InspectionId, inspectionEntity.Id)
                                             .With(x => x.InspectionRecordId, inspectionEntity.LastRecordId.Value)
                                             .With(x => x.Attachments, attachments)
                                             .CreateMany()
                                             .ToList();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi().SetupRequest(HttpMethod.Get, $"sites/{siteId}").ReturnsJson(site);

                var imagePathHelper = server.Arrange().GetImagePathHelper();
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
				db.InspectionRecords.Add(inspectionRecordEntity);
                db.Checks.AddRange(checkEntities);
                db.CheckRecords.AddRange(checkRecordEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/lastRecord");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<InspectionRecordDto>();
                result.Inspection.Should().BeEquivalentTo(InspectionDto.MapFromModel(InspectionEntity.MapToModel(inspectionEntity)));
                var checkRecordDtos = CheckRecordDto.MapFromModels(CheckRecordEntity.MapToModels(checkRecordEntities));

                checkRecordDtos.ForEach(x =>
                {
                    var checkRecordAttachments = checkRecordEntities.Where(y => y.Id == x.Id).First().Attachments;
                    x.Attachments = AttachmentDto.MapFromCheckRecordModels(
                        JsonSerializer.Deserialize<List<AttachmentBase>>(checkRecordAttachments, JsonSerializerExtensions.DefaultOptions),
                        imagePathHelper, site.CustomerId, site.Id, x.Id);
                });
                result.CheckRecords.Should().BeEquivalentTo(checkRecordDtos);
            }
        }

        [Fact]
        public async Task LastRecordDoesNotExist_GetInspectionLastRecord_ReturnsNotFound()
        {
            var siteId = Guid.NewGuid();
            var site = Fixture.Build<Site>().With(x => x.Id, siteId).Create();
            var inspectionEntity = Fixture.Build<InspectionEntity>()
                                          .With(x => x.SiteId, siteId)
                                          .Without(i => i.FrequencyDaysOfWeekJson)
                                          .Without(x => x.Zone)
                                          .Without(x => x.LastRecordId)
                                          .Without(x => x.Checks)
                                          .Without(x => x.LastRecord)
                                          .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(site);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Inspections.Add(inspectionEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{inspectionEntity.SiteId}/inspections/{inspectionEntity.Id}/lastRecord");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
