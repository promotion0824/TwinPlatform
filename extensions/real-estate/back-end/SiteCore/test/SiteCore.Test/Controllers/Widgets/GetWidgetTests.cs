using AutoFixture;
using FluentAssertions;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Tests;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace SiteCore.Test.Controllers.Widgets
{
    public class GetWidgetTests : BaseInMemoryTest
    {
        public GetWidgetTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WidgetExists_GetWidget_ReturnsWidget()
        {
            string metadata = @"{
                        ""GroupId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0"",
                        ""ReportId"":""db201b19-7cdb-48ce-a680-0923ca09ebf0""
                        }";

            var widget = Fixture.Build<WidgetEntity>()
                               .Without(x => x.SiteWidgets)
                               .Without(x => x.PortfolioWidgets)
                               .Without(x => x.ScopeWidgets)
                               .With(x => x.Metadata, metadata)
                               .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<SiteDbContext>();
                db.Widgets.Add(widget);
                db.SaveChanges();

                var response = await client.GetAsync($"internal-management/widgets/{widget.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<Widget>();
                var expected = WidgetEntity.MapToDomainObject(widget);

                result.Should().BeEquivalentTo(expected);
            }
        }

    }
}
