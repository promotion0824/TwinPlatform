using AutoFixture;
using FluentAssertions;
using SiteCore.Entities;
using SiteCore.Tests;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class DeleteWidgetFromScopeTests : BaseInMemoryTest
    {
        public DeleteWidgetFromScopeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ScopeExists_DeleteWidget_ReturnsUpdated()
        {
            var scopeId = "TestScopeId";

            var widget = Fixture.Build<WidgetEntity>()
                              .Without(x => x.ScopeWidgets)
                              .Without(x => x.SiteWidgets)
                              .Without(x => x.PortfolioWidgets)
                              .Create();

            var scopeWidget = new ScopeWidgetEntity() { ScopeId = scopeId, WidgetId = widget.Id, Widget = widget, Position = "0" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                //db.Widgets.Add(widget);
                db.ScopeWidgets.Add(scopeWidget);
                db.SaveChanges();

                var response = await client.DeleteAsync($"scopes/{scopeId}/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<SiteDbContext>();
                db.ScopeWidgets.Should().HaveCount(0);
            }
        }
    }
}
