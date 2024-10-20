namespace Willow.Msm.Connector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Get MSM Related Twin Details for the Facility.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public async Task<List<PurchasedEnergyTwinDetail>> GetMsmRelatedTwinDetailsForFacility(string siteId)
        {
            this.log!.LogInformation($"Getting MSM Related Twin Details for Facility {siteId}");

            // Get all the twins of interest for the site
            using var client = httpClientFactory.CreateClient();
            var twinsEndpoint = $"https://{this.carbonActivityRequestMessage!.OrganizationShortName}.app.willowinc.com/publicapi/v2/sites/{siteId}/twins/query";
            using var twinsQueryRequest = new HttpRequestMessage(HttpMethod.Post, twinsEndpoint);
            twinsQueryRequest.Content = new StringContent(
                "{\"rootModels\" : [\"dtmi:com:willowinc:Portfolio;1\", \"dtmi:com:willowinc:Building;1\", \"dtmi:com:willowinc:UtilityAccount;1\", \"dtmi:com:willowinc:BilledActiveElectricalEnergy;1\", \"dtmi:com:willowinc:ElectricalEnergySensor;1\", \"dtmi:com:willowinc:ElectricalMeter;1\"]}",
                Encoding.UTF8,
                "application/json");
            twinsQueryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.willowToken.Token);

            using var responseMessage = await client.SendAsync(twinsQueryRequest);
            var responseBody = responseMessage.Content.ReadAsStringAsync().Result;

            // Get basic properties for all twins
            var appTwinProperties = this.ParseAllTwinBasicProperties(responseBody);

            // Get all the Purchased Energy Twins for the site
            this.ParseMsmRelatedTwins(siteId, responseBody);

            // Iterate the purchasedEnergyTwinDetails for the site (facilty) and get the Utility Account details from the isCapabilityOf dtmi:com:willowinc:UtilityAccount;1 Twin
            var facility = this.msmOrganization.MsmFacilities!.Find(f => f.SiteId == siteId);

            foreach (var purchasedEnergyTwin in facility!.PurchasedEnergyTwinDetails)
            {
                // get the relationships for the purchasedEnergyTwin to find the Utility Account which will be the isCapabilityOf target twin
                var twinsWithRelationshipsEndpoint = $"https://{this.carbonActivityRequestMessage.OrganizationShortName}.app.willowinc.com/publicapi/v2/sites/{siteId}/twins/withrelationships";
                using var twinsWithRelationshipsQueryRequest = new HttpRequestMessage(HttpMethod.Post, twinsWithRelationshipsEndpoint);
                twinsWithRelationshipsQueryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.willowToken.Token);
                twinsWithRelationshipsQueryRequest.Content = new StringContent($"[\"{purchasedEnergyTwin.TwinId}\"]", Encoding.UTF8, "application/json");

                using var twinsWithRelationshipsResponseMessage = await client.SendAsync(twinsWithRelationshipsQueryRequest);
                responseBody = twinsWithRelationshipsResponseMessage.Content.ReadAsStringAsync().Result;

                var twinsArray = JArray.Parse(responseBody);
                foreach (var twin in twinsArray)
                {
                    var relationships = (JArray?)twin["relationships"];
                    if (relationships != null)
                    {
                        foreach (var relationship in relationships)
                        {
                            if (relationship != null && relationship["name"] != null && relationship["name"]?.ToString() == "isCapabilityOf")
                            {
                                var targetTwinId = relationship["targetId"]?.ToString();

                                // If the target twin is of modelId dtmi:com:willowinc:UtilityAccount;1, then update the PurchasedEnergyTwinDetail with the name of the Twin, which is the Utility Account Name
                                if (targetTwinId != null && appTwinProperties.ContainsKey(targetTwinId) && appTwinProperties[targetTwinId] != null)
                                {
                                    if (appTwinProperties[targetTwinId].ModelId == "dtmi:com:willowinc:UtilityAccount;1")
                                    {
                                        purchasedEnergyTwin.Msdyn_energyprovidername = appTwinProperties[targetTwinId].Name;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return facility.PurchasedEnergyTwinDetails;
        }

        /// <summary>
        /// Get the name and modelId for each TwinId.
        /// </summary>
        /// <param name="twinsJson">Twins Object.</param>
        /// <returns>Dictionary of TwinId and TwinName.</returns>
        private Dictionary<string, TwinBasicProperties> ParseAllTwinBasicProperties(string twinsJson)
        {
            var twinsArray = JArray.Parse(twinsJson);
            var twinBasicProperties = new Dictionary<string, TwinBasicProperties>();

            foreach (var twin in twinsArray)
            {
                var twinId = twin["id"]?.ToString();
                var modelId = twin["metadata"]!["modelId"]?.ToString();
                var name = twin["name"]?.ToString();

                twinBasicProperties.Add(twinId!, new TwinBasicProperties() { ModelId = modelId, Name = name });
            }

            return twinBasicProperties;
        }

        /// <summary>
        /// Parse the response from the sites endpoint and populate the MsmFacilities model.
        /// </summary>
        private void ParseMsmRelatedTwins(string siteId, string twinsJson)
        {
            var twinsArray = JArray.Parse(twinsJson);

            var purchasedEnergyTwinDetails = new List<PurchasedEnergyTwinDetail>();

            foreach (var twin in twinsArray)
            {
                var modelId = twin["metadata"]!["modelId"]?.ToString();

                // If its a Utility or Sensor Twin, add the Purchased Energy Twin Details
                if (modelId != null && (modelId == "dtmi:com:willowinc:BilledActiveElectricalEnergy;1" || modelId == "dtmi:com:willowinc:ElectricalEnergySensor;1"))
                {
                    // Set msdyn_dataqualitytype
                    var dataQualityType = DataQualityType.Actual;
                    if (modelId.Contains("Sensor", StringComparison.OrdinalIgnoreCase))
                    {
                        dataQualityType = DataQualityType.Metered;
                    }

                    // Set msdyn_energytype and msdyn_description
                    var energyType = EnergyType.Electricity;
                    var msdyn_description = "Whole building purchased electricity";

                    var twinId = twin["id"]?.ToString();
                    if (twinId == null)
                    {
                        continue;
                    }

                    var name = twin["name"]?.ToString();
                    if (name == null)
                    {
                        continue;
                    }

                    var unit = twin["unit"]?.ToString();
                    if (unit == null)
                    {
                        continue;
                    }

                    var trendId = twin["trendID"]?.ToString();
                    if (trendId == null)
                    {
                        continue;
                    }

                    var purchasedEnergyTwinDetail = new PurchasedEnergyTwinDetail
                    {
                        ModelId = modelId,
                        TwinId = twinId,
                        Name = name,
                        Unit = unit,
                        TrendId = trendId,
                        Msdyn_dataqualitytype = dataQualityType,
                        Msdyn_energytype = energyType,
                        Msdyn_description = msdyn_description,
                    };

                    purchasedEnergyTwinDetails.Add(purchasedEnergyTwinDetail);
                }
            }

            // Update the Facility with the Purchased Energy Twin Details
            var facility = this.msmOrganization.MsmFacilities!.Find(f => f.SiteId == siteId);
            facility!.PurchasedEnergyTwinDetails = purchasedEnergyTwinDetails;
        }
    }
}
