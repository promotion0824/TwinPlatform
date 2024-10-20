//-----------------------------------------------------------------------
// <copyright file="PartnerPurchasedEnergyRecord.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Azure;
    using Azure.Data.Tables;

    /// <summary>
    /// This class represents the schema the Partner uses to store their data 
    /// in their backend. This schema is up to the Partner to determine what
    /// works best for them based on their implementation and how much logic
    /// they would like this Api Controller to own. 
    /// </summary>
    public class PartnerPurchasedEnergyRecord : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        [Required]
        public DateTime ConsumptionEndDate { get; set; }

        [Required]
        public DateTime ConsumptionStartDate { get; set; }

        public double? Cost { get; set; }

        public string? CostUnit { get; set; }

        [Required]
        public string DataQualityType { get; set; } = string.Empty;

        [Required]
        public string EnergyProviderName { get; set; } = string.Empty;

        [Required]
        public string EnergyType { get; set; } = string.Empty;

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

        public string? Description { get; set; } = string.Empty;

        public string? Evidence { get; set; } = string.Empty;

        public string? ContractualInstrumentType { get; set; } = string.Empty;

        public string? OriginCorrelationId { get; set; } = string.Empty;

        public DateTime? TransactionDate { get; set; }

        public string? MeterNumber { get; set; } = string.Empty;
    }
}
