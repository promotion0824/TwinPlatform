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
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class CreateWidgetTests : BaseInMemoryTest
    {
        public CreateWidgetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_CreateWidget_WidgetIsCreated()
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

                var db = server.Arrange().CreateDbContext<SiteDbContext>();

                db.Sites.Add(site);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"internal-management/widgets", widgetDto);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<Widget>();

                db.Widgets.Should().HaveCount(1);
                db.SiteWidgets.Should().HaveCount(1);

                var widgetEntity = db.Widgets.First();

                result.Metadata.Should().Be(widgetEntity.Metadata);
            }
        }
    }
}