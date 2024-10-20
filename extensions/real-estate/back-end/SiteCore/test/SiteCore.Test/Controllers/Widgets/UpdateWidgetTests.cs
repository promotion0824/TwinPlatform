using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Entities;
using SiteCore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class UpdateWidgetTests : BaseInMemoryTest
    {
        public UpdateWidgetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_UpdateWidget_WidgetIsUpdated()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var siteId = Guid.NewGuid();
                var site = Fixture.Build<SiteEntity>()
                    .Without(x => x.Floors)
                    .Without(x => x.PortfolioId)
                    .With(x => x.Postcode, "111250")
                    .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                    .With(x => x.Id, siteId)
                    .Create();

                var postion = Fixture.Build<WidgetPosition>()
                                        .With(x => x.SiteId, siteId)
                                        .Create();

                var metadata = new { name = "Hello", category = "N/A", embedPath = "Hello.com", embedLocation = "reportsTab" };
                var widgetDto = Fixture.Build<CreateUpdateWidgetRequest>()
                       .With(w => w.Metadata, metadata)
                       .With(w => w.Positions, new List<WidgetPosition>() { postion })
                       .Create();

                var widget = Fixture.Build<WidgetEntity>()
                        .Without(x => x.SiteWidgets)
                        .Without(x => x.PortfolioWidgets)
                        .Without(x => x.ScopeWidgets)
                        .Create();

                var db = server.Arrange().CreateDbContext<SiteDbContext>();

                db.Sites.Add(site);
                db.Widgets.Add(widget);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"internal-management/widgets/{widget.Id}", widgetDto);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<Widget>();

                db.Widgets.Should().HaveCount(1);
                db.SiteWidgets.Should().HaveCount(1);

                var site2 = Fixture.Build<SiteEntity>()
                    .Without(x => x.Floors)
                    .Without(x => x.PortfolioId)
                    .With(x => x.Postcode, "111250")
                    .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                    .With(x => x.Id, Guid.NewGuid())
                    .Create();

                db.Sites.Add(site2);
                db.SaveChanges();

                var position2 = Fixture.Build<WidgetPosition>()
                        .With(x => x.SiteId, site2.Id)
                        .Create();

                widgetDto = Fixture.Build<CreateUpdateWidgetRequest>()
                   .With(w => w.Metadata, metadata)
                   .With(w => w.Positions, new List<WidgetPosition>() { postion, position2 })
                   .Create();

                response = await client.PutAsJsonAsync($"internal-management/widgets/{widget.Id}", widgetDto);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                result = await response.Content.ReadAsAsync<Widget>();

                Assert.Contains(result.Positions, x => x.SiteId == siteId);
                Assert.Contains(result.Positions, x => x.SiteId == site2.Id);

                db.Widgets.Should().HaveCount(1);
                db.SiteWidgets.Should().HaveCount(2);
            }
        }
    }
}
