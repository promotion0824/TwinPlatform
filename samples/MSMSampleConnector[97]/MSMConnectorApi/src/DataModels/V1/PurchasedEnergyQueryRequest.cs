//-----------------------------------------------------------------------
// <copyright file="PurchasedEnergyQueryRequest.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// This class represents how a customer and/or the Microsoft Sustainability
    /// Logic App Connector should communicate with the Partner to retrieve the 
    /// <see cref="CustomerPurchasedEnergyRecord"/> data.
    /// </summary>
    public class PurchasedEnergyQueryRequest
    {
        /// <summary>
        /// Collect all records on or after the given <see cref="DateTime"/>
        /// <br/><br/>
        /// Defaults to UTC Date minus two days.
        /// </summary>
        public DateTime? StartDate { get; set; } = DateTime.UtcNow.Date.AddDays(-2);

        /// <summary>
        /// Collect all records on or before the given <see cref="DateTime"/>
        /// <br/><br/>
        /// Defaults to UTC Date.
        /// </summary>
        public DateTime? EndDate { get; set; } = DateTime.UtcNow.Date;

        /// <summary>
        /// Gets or set the size of pages
        /// <br/><br/>
        /// Defaults to 100.
        /// </summary>
        [Range(1, 1000, ErrorMessage = $"{nameof(MaxNumberOfItems)} must be a value between 1 - 1000")]
        public int MaxNumberOfItems { get; set; } = 100;

        /// <summary>
        /// Gets or sets a token to resume collecting records between <see cref="StartDate"/> 
        /// and <see cref="EndDate"/> exceeding <see cref="MaxNumberOfItems"/>
        /// per response.
        /// </summary>
        public string? ContinuationToken { get; set; }
    }
}
