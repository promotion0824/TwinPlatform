using AutoFixture;
using FluentAssertions;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Tests;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class DeleteWidgetTests : BaseInMemoryTest
    {
        public DeleteWidgetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WidgetExists_DeleteWidget_WidgetIsDeleted()
        {
            var site = Fixture.Build<SiteEntity>()
                                .Without(x => x.Floors)
                                .Without(x => x.PortfolioId)
                                .With(x => x.Postcode, "111250")
                                .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                                .With(x => x.Status, SiteStatus.Operations)
                                .Create();

            var widget = Fixture.Build<WidgetEntity>()
                                .Without(x => x.ScopeWidgets)
                                .Without(x => x.SiteWidgets)
                                .Without(x => x.PortfolioWidgets)
                                .Create();

            var siteWidget = new SiteWidgetEntity() { SiteId = site.Id, WidgetId = widget.Id, Position = "0", Site = site, Widget = widget };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.AddRange(widget);
                db.SiteWidgets.AddRange(siteWidget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"internal-management/widgets/{widget.Id}?resetLinked=true");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db.Widgets.Should().HaveCount(0);
            }
        }

        [Fact]
        public async Task WidgetWithSitesExists_DeleteWidget_WidgetIsDeleted()
        {
            var widget = Fixture.Build<WidgetEntity>()
                                .Without(x => x.SiteWidgets)
                                .Without(x => x.PortfolioWidgets)
                                .Without(x => x.ScopeWidgets)
                                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Widgets.Add(widget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"internal-management/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db.Widgets.Should().HaveCount(0);
            }
        }

        [Fact]
        public async Task WidgetExistLinkedToSite_DeleteWidget_WidgetIsNotDeleted()
        {
            var site = Fixture.Build<SiteEntity>()
                      .Without(x => x.Floors)
                      .Without(x => x.PortfolioId)
                      .With(x => x.Postcode, "111250")
                      .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                      .With(x => x.Status, SiteStatus.Operations)
                      .Create();

            var widget = Fixture.Build<WidgetEntity>()
                              .Without(x => x.ScopeWidgets)
                              .Without(x => x.SiteWidgets)
                              .Without(x => x.PortfolioWidgets)
                              .Create();

            var siteWidget = new SiteWidgetEntity() { SiteId = site.Id, WidgetId = widget.Id, Position = "top" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.Add(widget);
                db.SiteWidgets.Add(siteWidget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"internal-management/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                db.Widgets.Should().HaveCount(1);
            }
        }

    }
}
