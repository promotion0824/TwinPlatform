using AutoFixture;
using FluentAssertions;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Requests;
using SiteCore.Tests;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class AddWidgetToSiteTests : BaseInMemoryTest
    {
        public AddWidgetToSiteTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_AddWidget_ReturnsUpdatedSite()
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
                              .Without(x => x.Metadata)
                              .Create();

            var request = new AddWidgetRequest()
            {
                WidgetId= widget.Id,
                Position = 0
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.Add(widget);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/widgets", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<SiteDbContext>();
                db.SiteWidgets.Should().HaveCount(1);
                var siteWidgetEntity = db.SiteWidgets.First();
                siteWidgetEntity.SiteId.Should().Be(site.Id);
                siteWidgetEntity.WidgetId.Should().Be(widget.Id);
            }
        }
    }
}
