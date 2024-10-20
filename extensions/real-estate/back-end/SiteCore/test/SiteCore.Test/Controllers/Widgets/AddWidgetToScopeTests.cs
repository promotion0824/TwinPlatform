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
    public class AddWidgetToScopeTests : BaseInMemoryTest
    {
        public AddWidgetToScopeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ScopeExists_AddWidget_ReturnsUpdated()
        {
            var scopeId = "TestScopeId";

            var widget = Fixture.Build<WidgetEntity>()
                              .Without(x => x.ScopeWidgets)
                              .Without(x => x.SiteWidgets)
                              .Without(x => x.PortfolioWidgets)
                              .Without(x => x.Metadata)
                              .Create();

            var request = new AddWidgetRequest()
            {
                WidgetId = widget.Id,
                Position = 0
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Widgets.Add(widget);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"scopes/{scopeId}/widgets", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<SiteDbContext>();
                db.ScopeWidgets.Should().HaveCount(1);
                var scopeWidgetEntity = db.ScopeWidgets.First();
                scopeWidgetEntity.ScopeId.Should().Be(scopeId);
                scopeWidgetEntity.WidgetId.Should().Be(widget.Id);
            }
        }
    }
}
