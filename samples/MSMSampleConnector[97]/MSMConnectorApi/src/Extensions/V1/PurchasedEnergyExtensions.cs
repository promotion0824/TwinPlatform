//-----------------------------------------------------------------------
// <copyright file="PurchasedEnergyExtensions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.Extensions.V1
{
    using System;
    using Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1;

    /// <summary>
    /// This class is the bit responsible for handling translations between
    /// the various data models. 
    /// </summary>
    public static class PurchasedEnergyExtensions
    {
        /// <summary>
        /// Extends <see cref="PartnerPurchasedEnergyRecord"/> to return a <see cref="CustomerPurchasedEnergyRecord"/> record
        /// allowing one or both data models to grow through multiple api versions without breaking contracts.
        /// </summary>
        /// <param name="storedPurchasedEnergyRecord">The data that was retrieved from persistent storage</param>
        /// <returns>The data that can be returned to a customer</returns>
        public static CustomerPurchasedEnergyRecord ToPurchasedEnergy(this PartnerPurchasedEnergyRecord storedPurchasedEnergyRecord)
        {
            var purchasedEnergy = new CustomerPurchasedEnergyRecord()
            {
                ConsumptionStartDate = storedPurchasedEnergyRecord.ConsumptionStartDate,
                ConsumptionEndDate = storedPurchasedEnergyRecord.ConsumptionEndDate,
                Cost = storedPurchasedEnergyRecord.Cost,
                CostUnit = storedPurchasedEnergyRecord.CostUnit,
                EnergyProviderName = storedPurchasedEnergyRecord.EnergyProviderName,
                Facility = storedPurchasedEnergyRecord.Facility,
                IsRenewable = storedPurchasedEnergyRecord.IsRenewable,
                Name = storedPurchasedEnergyRecord.Name,
                OrganizationalUnit = storedPurchasedEnergyRecord.OrganizationalUnit,
                Quantity = storedPurchasedEnergyRecord.Quantity,
                QuantityUnit = storedPurchasedEnergyRecord.QuantityUnit,
                Description = storedPurchasedEnergyRecord.Description,
                Evidence = storedPurchasedEnergyRecord.Evidence,
                ContractualInstrumentType = storedPurchasedEnergyRecord.ContractualInstrumentType,
                OriginCorrelationId = storedPurchasedEnergyRecord.OriginCorrelationId,
                TransactionDate = storedPurchasedEnergyRecord.TransactionDate,
                MeterNumber = storedPurchasedEnergyRecord.MeterNumber,
            };

            if (Enum.TryParse(storedPurchasedEnergyRecord.DataQualityType, out DataQualityType dataQualityType))
            {
                purchasedEnergy.DataQualityType = dataQualityType;
            }

            if (Enum.TryParse(storedPurchasedEnergyRecord.EnergyType, out EnergyType energyType))
            {
                purchasedEnergy.EnergyType = energyType;
            }

            return purchasedEnergy;
        }
    }
}
