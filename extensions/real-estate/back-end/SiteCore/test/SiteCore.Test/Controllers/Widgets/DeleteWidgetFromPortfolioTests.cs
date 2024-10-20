using AutoFixture;
using FluentAssertions;
using SiteCore.Entities;
using SiteCore.Enums;
using SiteCore.Tests;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class DeleteWidgetFromPortfolioTests : BaseInMemoryTest
    {
        public DeleteWidgetFromPortfolioTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WidgetExists_DeletePortfolioWidget_ReturnsUpdatedSite()
        {
            var portfolioId = Guid.NewGuid();
            var site = Fixture.Build<SiteEntity>()
                               .Without(x => x.Floors)
                               .With(x => x.PortfolioId, portfolioId)
                               .With(x => x.Postcode, "111250")
                               .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                               .With(x => x.Status, SiteStatus.Operations)
                               .Create();

            var widget = Fixture.Build<WidgetEntity>()
                              .Without(x => x.ScopeWidgets)
                              .Without(x => x.SiteWidgets)
                              .Without(x => x.PortfolioWidgets)
                              .Create();

            var portfolioWidget = new PortfolioWidgetEntity() { PortfolioId = portfolioId, WidgetId = widget.Id, Position = 1 };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.Add(widget);
                db.PortfolioWidgets.Add(portfolioWidget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"portfolios/{portfolioId}/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<SiteDbContext>();
                db.PortfolioWidgets.Should().HaveCount(0);
            }
        }
    }
}
