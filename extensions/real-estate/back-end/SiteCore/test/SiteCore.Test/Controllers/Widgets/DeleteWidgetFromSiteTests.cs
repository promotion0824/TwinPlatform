using AutoFixture;
using FluentAssertions;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Tests;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class DeleteWidgetFromSiteTests : BaseInMemoryTest
    {
        public DeleteWidgetFromSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_DeleteWidget_ReturnsUpdatedSite()
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

            var siteWidget = new SiteWidgetEntity() { SiteId = site.Id, WidgetId = widget.Id, Position = "0" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.Add(widget);
                db.SiteWidgets.Add(siteWidget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{site.Id}/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<SiteDbContext>();
                db.SiteWidgets.Should().HaveCount(0);
            }
        }
    }
}
