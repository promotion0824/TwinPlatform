using Alba;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using SiteCore.ServiceTests.Server;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SiteCore.ServiceTests.Controllers
{
    public class WidgetsControllerTests : BaseServerFixture
    {
        public WidgetsControllerTests()
        {
        }

        [Fact]
        public async Task Create_internal_mgmt_widget()
        {
            // Arrange
            var widgetPayload = new
            {
                metadata = "internal widget",
                type = "PowerBIReport"
            };

            // Act - Add a new widget
            var createWidget = await Host.Scenario(s =>
            {
                s.Post.Json(widgetPayload).ToUrl("/internal-management/widgets").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            // Assert - Widget is created
            var createWidgetRes = JToken.Parse(createWidget.ReadAsText());
            createWidgetRes.Should().HaveElement("type").And.NotBeNull();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
