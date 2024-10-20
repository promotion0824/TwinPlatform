//-----------------------------------------------------------------------
// <copyright file="PurchasedEnergyQueryResponse.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1
{
    /// <summary>
    /// This class represents the contract that will be shared via a Public Api to the customer
    /// or at least reachable via Microsoft Sustainability Logic App Connector(s).
    /// </summary>
    public class PurchasedEnergyQueryResponse
    {
        /// <summary>
        /// Data is a collection of individual records representing different days or assets.
        /// </summary>
        public IList<CustomerPurchasedEnergyRecord> Data { get; set; } = new List<CustomerPurchasedEnergyRecord>();

        /// <summary>
        /// To scope down the volume of data returned to avoid throttling, pagination is 
        /// highly recommended.
        /// </summary>
        public string? ContinuationToken { get; set; }
    }
}
