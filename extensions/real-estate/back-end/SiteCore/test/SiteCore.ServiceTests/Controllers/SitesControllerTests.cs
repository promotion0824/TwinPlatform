using Alba;
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using SiteCore.Dto;
using SiteCore.ServiceTests.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SiteCore.ServiceTests.Controllers
{
    public class SitesControllerTests : BaseServerFixture
    {
        public SitesControllerTests()
        {
        }

        [Fact]
        public async Task Get_all_sites()
        {
            // Arrange - add new site
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSite.json"));

            var newSite = await Host.Scenario(s =>
            {
                var custId = Guid.NewGuid();
                var portfolioId = Guid.NewGuid();

                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites").ContentType("application/json");

            });

            // Act
            var allSitesRequest = await Host.Scenario(s =>
            {
                s.Get.Url("/sites");
                s.StatusCodeShouldBeOk();
            });

            // Assert
            var allSitesResponse = allSitesRequest.ReadAsJson<List<SiteDetailDto>>();
            allSitesResponse.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Get_site_for_given_customer()
        {
            // Arrange - add new site
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSite.json"));

            var custId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var newSite = await Host.Scenario(s =>
            {
                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            // Act - Get the specific site for given customer
            var specificSite = await Host.Scenario(s =>
            {
                s.Get.Url("/customers/" + custId + "/sites");
                s.StatusCodeShouldBeOk();
            });

            var specificSiteResponse = JToken.Parse(specificSite.ReadAsText())[0];

            // Assert - that only site specific to customer is returned
            specificSiteResponse.Should().HaveElement("customerId").Which.Equals(custId.ToString());
            specificSiteResponse.Should().HaveElement("createdDate");
        }

        [Fact]
        public async Task Get_site_for_given_customer_portfolio()
        {
            // Arrange - add new site
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSite.json"));

            var custId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var newSite = await Host.Scenario(s =>
            {
                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            // Act - Get site with portfolio query param
            var specificSite = await Host.Scenario(s =>
            {
                s.Get.Url("/customers/" + custId.ToString() + "/sites").QueryString("portfolioId", portfolioId.ToString());
                s.StatusCodeShouldBeOk();
            });

            var specificSiteResponse = JToken.Parse(specificSite.ReadAsText())[0];

            // Assert 
            specificSiteResponse.Should().HaveElement("portfolioId").Which.Equals(portfolioId.ToString());
            specificSiteResponse.Should().HaveElement("createdDate");
        }

        [Fact]
        public async Task Update_site()
        {
            // Arrange - add new site
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSite.json"));

            var custId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            var newSite = await Host.Scenario(s =>
            {
                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            var newSiteRespone = JToken.Parse(newSite.ReadAsText());
            var newSiteId = newSiteRespone["id"].ToString();

            // Act - update the site
            var updateSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "UpdateSite.json"));
            var updateSite = await Host.Scenario(s =>
            {
                s.Put.Text(updateSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites/" + newSiteId).ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            // Assert - that the site is updated
            var updateSiteRes = JToken.Parse(updateSite.ReadAsText());
            updateSiteRes["name"].ToString().Should().BeEquivalentTo("RS Site 04 - update");
            updateSiteRes["type"].ToString().Should().BeEquivalentTo("Retail");
            updateSiteRes["status"].ToString().Should().BeEquivalentTo("Construction");
            updateSiteRes["area"].ToString().Should().BeEquivalentTo("1500");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
