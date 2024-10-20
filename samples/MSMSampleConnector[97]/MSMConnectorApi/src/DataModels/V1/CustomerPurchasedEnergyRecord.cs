//-----------------------------------------------------------------------
// <copyright file="CustomerPurchasedEnergyRecord.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// This class represents the content of each record that will be surfaced
    /// to the customer and/or Microsoft Sustainability Logic App Connector.
    /// <br/><br/>
    /// It is up to the partner if they would prefer to finalize the mapping to
    /// Microsoft Sustainability Manager's Internal Api schema here or in the
    /// shared Logic App implementation.
    /// </summary>
    public class CustomerPurchasedEnergyRecord
    {
        [Required]
        public DateTime ConsumptionEndDate { get; set; }

        [Required]
        public DateTime ConsumptionStartDate { get; set; }

        public double? Cost { get; set; }

        public string? CostUnit { get; set; }

        [Required]
        public DataQualityType DataQualityType { get; set; } = DataQualityType.Metered;

        [Required]
        public string EnergyProviderName { get; set; } = string.Empty;

        [Required]
        public EnergyType EnergyType { get; set; } = EnergyType.Electricity;

        [Required]
        public string Facility { get; set; } = string.Empty;

        [Required]
        public bool IsRenewable { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string OrganizationalUnit { get; set; } = string.Empty;

        [Required]
        public double Quantity { get; set; }

        [Required]
        public string QuantityUnit { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Evidence { get; set; }

        public string? ContractualInstrumentType { get; set; }

        public string? OriginCorrelationId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string? MeterNumber { get; set; }
    }

    public enum DataQualityType
    {
        Actual = 700610000,
        Estimated = 700610001,
        Metered = 700610002,
    }

    public enum EnergyType
    {
        Electricity = 700610000,
        Steam = 700610001,
        Heating = 700610002,
        Cooling = 700610003,
    }
}
