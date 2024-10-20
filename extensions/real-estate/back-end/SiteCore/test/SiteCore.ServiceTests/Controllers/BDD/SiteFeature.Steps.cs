using Alba;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using SiteCore.ServiceTests.Server;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SiteCore.ServiceTests.Controllers.BDD
{
    public partial class SiteFeature : BaseServerFixture
    {
        private string siteId;
        public SiteFeature()
        {
        }

        private async Task Given_i_have_site_admin_access()
        {
            // Empty method - Added for BDD brevity.
            // Can be used to set user access level for test. eg., SiteViewer, ProfileAdmin, etc.
            await Task.CompletedTask;
        }
        private async Task When_i_create_a_new_site()
        {
            // Add new site
            var newSitePayloadText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Sites", "CreateSite.json"));

            var newSite = await Host.Scenario(s =>
            {
                var custId = Guid.NewGuid();
                var portfolioId = Guid.NewGuid();

                s.Post.Text(newSitePayloadText).ToUrl("/customers/" + custId.ToString() + "/portfolios/" + portfolioId.ToString() + "/sites").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });

            siteId = JToken.Parse(newSite.ReadAsText())["id"].ToString();
        }

        private async Task And_add_floors_to_the_site()
        {
            // Add a new floor to the site
            var newFloorPayload = File.ReadAllText(
                Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "TestData", "Floors", "NewFloor.json"));

            var addFloor = await Host.Scenario(s =>
            {
                s.Post.Text(newFloorPayload).ToUrl("/sites/" + siteId + "/floors").ContentType("application/json");
                s.StatusCodeShouldBeOk();
            });
        }

        private async Task Then_i_can_get_my_site_from_a_list_of_all_sites()
        {
            // Get all sites
            var allSitesRequest = await Host.Scenario(s =>
            {
                s.Get.Url("/sites");
                s.StatusCodeShouldBeOk();
            });

            var allSitesResponse = JToken.Parse(allSitesRequest.ReadAsText());

            // Check that siteId is returned in response
            allSitesResponse.SelectToken("$[0].id").Should().HaveValue(siteId);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}