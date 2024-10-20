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
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class GetPortfolioWidgetTests : BaseInMemoryTest
    {
        public GetPortfolioWidgetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetPortfolioWidgets_ReturnsAllPortfolioWidgets()
        {
            var portfolioId = Guid.NewGuid();
            string metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var site = Fixture.Build<SiteEntity>()
                      .Without(x => x.Floors)
                      .With(x => x.PortfolioId, portfolioId)
                      .With(x => x.Postcode, "111250")
                      .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                      .With(x => x.Status, SiteStatus.Operations)
                      .Create();

            var widgets = Fixture.Build<WidgetEntity>()
                               .Without(x => x.SiteWidgets)
                               .Without(x => x.PortfolioWidgets)
                               .Without(x => x.ScopeWidgets)
                               .With(x => x.Metadata, metadata)
                               .With(x => x.Type, WidgetType.SigmaReport)
                               .CreateMany(10)
                               .ToList();

            var portfolioWidgets = widgets.Select(w => new PortfolioWidgetEntity { PortfolioId = portfolioId, WidgetId = w.Id, Position = 1, Widget = w });

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.Widgets.AddRange(widgets);
                db.PortfolioWidgets.AddRange(portfolioWidgets);
                db.SaveChanges();

                var response = await client.GetAsync($"portfolios/{portfolioId}/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();
                var expected = PortfolioWidgetEntity.MapToDomainObjects(portfolioWidgets);

                result.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public async Task GetPortfolioWidgets_IncludeSiteWidgets_ReturnsAllPortfolioAndSiteWidgets(bool? includeSiteWidgets)
        {
            var portfolioId = Guid.NewGuid();
            string metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var sites = Fixture.Build<SiteEntity>()
                      .Without(x => x.Floors)
                      .With(x => x.PortfolioId, portfolioId)
                      .With(x => x.Postcode, "111250")
                      .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                      .With(x => x.Status, SiteStatus.Operations)
                      .CreateMany(5);

            var widgets = Fixture.Build<WidgetEntity>()
                               .Without(x => x.SiteWidgets)
                               .Without(x => x.PortfolioWidgets)
                               .Without(x => x.ScopeWidgets)
                               .With(x => x.Metadata, metadata)
                               .With(x => x.Type, WidgetType.SigmaReport)
                               .CreateMany(2)
                               .ToList();

            var portfolioWidgets = widgets.Select(w => new PortfolioWidgetEntity { PortfolioId = portfolioId, WidgetId = w.Id, Position = 1, Widget = w }).ToList();

            var siteWidgets = new List<SiteWidgetEntity>();

            foreach (var site in sites)
            {
                siteWidgets.AddRange(widgets.Select(w => new SiteWidgetEntity { SiteId = site.Id, WidgetId = w.Id, Position = "1", Site = site, Widget = w }).ToList());
            }

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Sites.AddRange(sites);
                db.Widgets.AddRange(widgets);
                db.PortfolioWidgets.AddRange(portfolioWidgets);
                db.SiteWidgets.AddRange(siteWidgets);
                db.SaveChanges();

                var response = await client.GetAsync($"portfolios/{portfolioId}/widgets?includeSiteWidgets={includeSiteWidgets}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();

                var expectedPortfolioWidgets = PortfolioWidgetEntity.MapToDomainObjects(portfolioWidgets);

                var expectedSiteWidgets = (includeSiteWidgets ?? false) ?  SiteWidgetEntity.MapToDomainObjects(siteWidgets): new List<Widget>();

                result.Should().BeEquivalentTo(expectedPortfolioWidgets.Union(expectedSiteWidgets.GroupBy(x => x.Id).Select(x => new Widget()
                {
                    Id = x.Key,
                    Metadata = x.First().Metadata,
                    Type = x.First().Type,
                    Positions = x.SelectMany(y => y.Positions)
                }).ToList()));
            }
        }

    }
}
