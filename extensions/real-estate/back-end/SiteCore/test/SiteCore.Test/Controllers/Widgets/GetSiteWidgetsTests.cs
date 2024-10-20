using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class GetSiteWidgetsTests : BaseInMemoryTest
    {
        public GetSiteWidgetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllWidgets_ReturnsAllWidgets()
        {
            string metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var site = Fixture.Build<SiteEntity>()
                      .Without(x => x.Floors)
                      .Without(x => x.PortfolioId)
                      .With(x => x.Postcode, "111250")
                      .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                      .With(x => x.Status, SiteStatus.Operations)
                      .Create();

            var widgets = Fixture.Build<WidgetEntity>()
                               .Without(x => x.SiteWidgets)
                               .Without(x => x.PortfolioWidgets)
                               .Without(x => x.ScopeWidgets)
                               .With(x => x.Metadata, metadata)
                               .CreateMany(10)
                               .ToList();

            var siteWidgets = widgets.Select(w => new SiteWidgetEntity() { SiteId = site.Id, WidgetId = w.Id, Position = "0", Site = site, Widget = w}).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.AddRange(widgets);
                db.SiteWidgets.AddRange(siteWidgets);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{site.Id}/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();
                var expected = SiteWidgetEntity.MapToDomainObjects(siteWidgets);

                result.Should().BeEquivalentTo(expected);
            }
        }
    }
}
