using Alba;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using SiteCore.ServiceTests.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SiteCore.ServiceTests.Controllers
{
    public class FloorsControllerTests : BaseServerFixture
    {
        public FloorsControllerTests()
        {   
        }

        [Fact]
        public async Task Add_new_floor()
        {
            // Arrange - create a new site 
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSiteNoFloors.json"));

            var newSite = await Host.Scenario(s =>
            {
                var custId = Guid.NewGuid();
                var portfolioId = Guid.NewGuid();

                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId + "/portfolios/" + portfolioId + "/sites").ContentType("application/json");
            });
            var newSiteResponse = JToken.Parse(newSite.ReadAsText());
            var siteId = newSiteResponse["id"];

            // Act - Add a new floor
            var newFloorPayload = File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Floors", "NewFloor.json"));

            var addFloor = await Host.Scenario(s =>
            {
                s.Post.Text(newFloorPayload).ToUrl("/sites/" + siteId + "/floors").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            var addFloorResponse = JToken.Parse(addFloor.ReadAsText());
         
            // Assert - Check the response from service
            addFloorResponse.Should().HaveElement("id").And.NotBeNull();
            addFloorResponse.Should().HaveElement("siteId").And.NotBeNull();
            addFloorResponse.Should().HaveElement("name").And.NotBeNull();
            addFloorResponse.Should().HaveElement("code").And.NotBeNull();
            addFloorResponse.Should().HaveElement("sortOrder");
            addFloorResponse.Should().HaveElement("geometry");
            addFloorResponse.Should().HaveElement("modelReference").And.NotBeNull();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
