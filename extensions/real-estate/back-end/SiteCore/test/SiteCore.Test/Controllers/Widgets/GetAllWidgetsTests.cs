using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class GetAllWidgetsTests : BaseInMemoryTest
    {
        public GetAllWidgetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllWidgets_ReturnsAllWidgets()
        {
            string metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var widgets = Fixture.Build<WidgetEntity>()
                               .Without(x => x.SiteWidgets)
                               .Without(x => x.PortfolioWidgets)
                               .Without(x => x.ScopeWidgets)
                               .With(x => x.Metadata, metadata)
                               .CreateMany(10)
                               .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Widgets.AddRange(widgets);
                db.SaveChanges();

                var response = await client.GetAsync($"internal-management/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();
                var expected = WidgetEntity.MapToDomainObjects(widgets);

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task HasSites_GetAllWidgets_ReturnsAllWidgets()
        {
            var metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var widgetId = Guid.NewGuid();

            var sites = Fixture.Build<SiteEntity>()
                          .Without(x => x.Floors)
                          .Without(x => x.PortfolioId)
                          .With(x => x.Postcode, "111250")
                          .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                          .With(x => x.Status, SiteStatus.Operations)
                          .CreateMany(10)
                          .ToList();

            var rnd = new Random();
            var siteWidgets = sites.Select(s => new SiteWidgetEntity() { SiteId = s.Id, WidgetId = widgetId, Position = rnd.Next(50).ToString() }).ToList();

            var widget = Fixture.Build<WidgetEntity>()
                .With(x => x.Id, widgetId)
                .With(x => x.SiteWidgets, siteWidgets)
                .Without(x => x.PortfolioWidgets)
                .Without(x => x.ScopeWidgets)
                .With(x => x.Metadata, metadata)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.AddRange(sites);
                db.Widgets.Add(widget);
                db.SiteWidgets.AddRange(siteWidgets);
                db.SaveChanges();

                var widgetEntities = await db.Widgets.ToListAsync();

                var response = await client.GetAsync("internal-management/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();

                var expected = new List<Widget>() { WidgetEntity.MapToDomainObject(widget) };

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task HasSitesAndPortfolios_GetAllWidgets_ReturnsAllWidgets()
        {
            var metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var widgetId = Guid.NewGuid();

            var portfolioWidgets = Fixture.Build<PortfolioWidgetEntity>()
                         .Without(x => x.Widget)
                         .With(x => x.WidgetId, widgetId)
                         .CreateMany(10)
                         .ToList();

            var sites = Fixture.Build<SiteEntity>()
                          .Without(x => x.Floors)
                          .Without(x => x.PortfolioId)
                          .With(x => x.Postcode, "111250")
                          .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                          .With(x => x.Status, SiteStatus.Operations)
                          .CreateMany(10)
                          .ToList();

            var rnd = new Random();
            var siteWidgets = sites.Select(s => new SiteWidgetEntity() { SiteId = s.Id, WidgetId = widgetId, Position = rnd.Next(50).ToString() }).ToList();

            var widget = Fixture.Build<WidgetEntity>()
                .With(x => x.Id, widgetId)
                .With(x => x.SiteWidgets, siteWidgets)
                .With(x => x.PortfolioWidgets, portfolioWidgets)
                .Without(x => x.ScopeWidgets)
                .With(x => x.Metadata, metadata)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.AddRange(sites);
                db.Widgets.Add(widget);
                db.SiteWidgets.AddRange(siteWidgets);
                db.PortfolioWidgets.AddRange(portfolioWidgets);
                db.SaveChanges();

                var widgetEntities = await db.Widgets.ToListAsync();

                var response = await client.GetAsync("internal-management/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();

                var expected = new List<Widget>() { WidgetEntity.MapToDomainObject(widget) };

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task HasSitesAndPortfoliosAndScopes_GetAllWidgets_ReturnsAllWidgets()
        {
            var metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var widgetId = Guid.NewGuid();

            var portfolioWidgets = Fixture.Build<PortfolioWidgetEntity>()
                         .Without(x => x.Widget)
                         .With(x => x.WidgetId, widgetId)
                         .CreateMany(10)
                         .ToList();

            var sites = Fixture.Build<SiteEntity>()
                          .Without(x => x.Floors)
                          .Without(x => x.PortfolioId)
                          .With(x => x.Postcode, "111250")
                          .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                          .With(x => x.Status, SiteStatus.Operations)
                          .CreateMany(10)
                          .ToList();

            var rnd = new Random();
            var siteWidgets = sites.Select(s => new SiteWidgetEntity() { SiteId = s.Id, WidgetId = widgetId, Position = rnd.Next(50).ToString() }).ToList();

            var scopeWidgets = Fixture.Build<ScopeWidgetEntity>()
                         .Without(x => x.Widget)
                         .With(x => x.Position, "0")
                         .With(x => x.WidgetId, widgetId)
                         .CreateMany(10)
                         .ToList();

            var widget = Fixture.Build<WidgetEntity>()
                .With(x => x.Id, widgetId)
                .With(x => x.SiteWidgets, siteWidgets)
                .With(x => x.PortfolioWidgets, portfolioWidgets)
                .With(x => x.ScopeWidgets, scopeWidgets)
                .With(x => x.Metadata, metadata)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.AddRange(sites);
                db.Widgets.Add(widget);
                db.SiteWidgets.AddRange(siteWidgets);
                db.PortfolioWidgets.AddRange(portfolioWidgets);
                db.ScopeWidgets.AddRange(scopeWidgets);
                db.SaveChanges();

                var widgetEntities = await db.Widgets.ToListAsync();

                var response = await client.GetAsync("internal-management/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();

                var expected = new List<Widget>() { WidgetEntity.MapToDomainObject(widget) };

                result.Should().BeEquivalentTo(expected);
            }
        }
    }
}
