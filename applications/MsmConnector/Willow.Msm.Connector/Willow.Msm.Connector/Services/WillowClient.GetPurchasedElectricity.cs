namespace Willow.Msm.Connector.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Get Purchased Electricity for the Organization.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public async Task<List<MsmPurchasedEnergy>> GetPurchasedElectricity()
        {
            var purchasedElectricities = new List<MsmPurchasedEnergy>();

            foreach (var facility in this.msmOrganization.MsmFacilities!)
            {
                await GetMsmRelatedTwinDetailsForFacility(facility.SiteId);

                if (facility.PurchasedEnergyTwinDetails.Count == 0)
                {
                    this.log!.LogInformation($"No complete MSM Related Twins found for Facility {facility.SiteId}.  Not attempting to get quantity information.");
                    continue;
                }

                await GetPurchasedEnergyQuantityForFacility(facility.SiteId);
            }

            foreach (var facility in msmOrganization.MsmFacilities)
            {
                foreach (var twinDetails in facility.PurchasedEnergyTwinDetails)
                {
                    var missingTwinDetailsProperties = GetUnsetRequiredProperties(twinDetails);
                    if (!string.IsNullOrEmpty(missingTwinDetailsProperties))
                    {
                        continue;
                    }

                    if (twinDetails.Quantities == null)
                    {
                        continue;
                    }

                    foreach (var quantity in twinDetails.Quantities)
                    {
                        var purchasedElectricity = new Models.MsmPurchasedEnergy
                        {
                            Msdyn_name = "PurchasedElectricity",
                            Msdyn_description = twinDetails.Msdyn_description,
                            Msdyn_facilityid = facility.Msdyn_facilityid,
                            Msdyn_isrenewable = "No",
                            Msdyn_dataqualitytype = twinDetails.Msdyn_dataqualitytype.ToString(),
                            Msdyn_consumptionstartdate = quantity.StartDate.ToString("yyyy-MM-dd"),
                            Msdyn_consumptionenddate = quantity.EndDate.ToString("yyyy-MM-dd"),
                            Msdyn_energyprovidername = twinDetails.Msdyn_energyprovidername!,
                            Msdyn_energytype = twinDetails.Msdyn_energytype.ToString(),
                            Msdyn_organizationalunitid = msmOrganization.Msdyn_organizationalunitid!,
                            Msdyn_quantity = (decimal)quantity.Value!,
                            Msdyn_quantityunit = twinDetails.Unit,
                        };

                        purchasedElectricities.Add(purchasedElectricity);
                    }
                }
            }

            return purchasedElectricities;
        }
    }
}
