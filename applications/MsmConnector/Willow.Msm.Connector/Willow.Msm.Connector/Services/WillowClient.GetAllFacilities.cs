namespace Willow.Msm.Connector.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Get All Facilities for the Organization.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public async Task<List<MsmFacility>> GetAllFacilities()
        {
            this.log!.LogInformation("Getting all Facilities for the Organization");

            // Get all the Facilities (sites) for the Organization (customer)
            using var client = httpClientFactory.CreateClient();
            var facilitiesEndpoint = $"https://{carbonActivityRequestMessage!.OrganizationShortName}.app.willowinc.com/publicapi/v2/sites";
            using var req = new HttpRequestMessage(HttpMethod.Get, facilitiesEndpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.willowToken.Token);

            using var responseMessage = await client.SendAsync(req);
            var responseBody = responseMessage.Content.ReadAsStringAsync().Result;

            this.ParseFacilities(responseBody);

            return this.msmOrganization.MsmFacilities!;
        }

        /// <summary>
        /// Parse the Name and TwinId for all sites and populate the MsmFacilities for the MsmOrganization.
        /// </summary>
        /// <param name="facilitiesJson">JSON response from /v2/sites endpoint.</param>
        private void ParseFacilities(string facilitiesJson)
        {
            var facilitiesArray = JArray.Parse(facilitiesJson);
            this.msmOrganization.MsmFacilities = new List<MsmFacility>();

            foreach (var facility in facilitiesArray)
            {
                if (facility is JObject obj && obj["name"] != null && obj["id"] != null)
                {
                    var msmFacility = new MsmFacility
                    {
                        Msdyn_facilityid = obj["name"]!.ToString(),
                        SiteId = obj["id"]!.ToString(),
                    };

                    this.msmOrganization.MsmFacilities.Add(msmFacility);
                }
            }
        }
    }
}
