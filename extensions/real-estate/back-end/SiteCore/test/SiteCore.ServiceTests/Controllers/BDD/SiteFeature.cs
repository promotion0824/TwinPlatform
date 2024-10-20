using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System.Threading.Tasks;

[assembly: LightBddScope]
namespace SiteCore.ServiceTests.Controllers.BDD
{
    [Label("SITE FEATURE")]
    [FeatureDescription(
@"In order to access site details
As a site admin user
I want to be able to create, update and remove a site")]
    public partial class SiteFeature
    {
        [Label("Scenario - I can create a site, save it and add floors to that site")]
        [Scenario]
        public async Task Create_site_and_add_floors()
        {
            await Runner.RunScenarioAsync(
                Given_i_have_site_admin_access,
                When_i_create_a_new_site,
                And_add_floors_to_the_site,
                Then_i_can_get_my_site_from_a_list_of_all_sites);
        }
    }
}