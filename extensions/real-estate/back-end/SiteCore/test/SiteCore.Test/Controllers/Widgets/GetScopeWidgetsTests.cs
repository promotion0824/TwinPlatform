using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Tests;
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
    public class GetScopeWidgetsTests : BaseInMemoryTest
    {
        public GetScopeWidgetsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetAllWidgets_ReturnsAllWidgets()
        {
            var scopeId = "TestScopeId";
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

            var scopeWidgets = widgets.Select(w => new ScopeWidgetEntity() { ScopeId = scopeId, WidgetId = w.Id, Position = "0", Widget = w}).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Widgets.AddRange(widgets);
                db.ScopeWidgets.AddRange(scopeWidgets);
                db.SaveChanges();

                var response = await client.GetAsync($"scopes/{scopeId}/widgets");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<Widget>>();
                var expected = ScopeWidgetEntity.MapToDomainObjects(scopeWidgets);

                result.Should().BeEquivalentTo(expected);
            }
        }
    }
}
