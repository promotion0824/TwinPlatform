// -----------------------------------------------------------------------
// <copyright file="IngestionManagerOptions.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion
{
    using System.ComponentModel.DataAnnotations;
    using Willow.AppContext;

    /// <summary>
    /// Configures the connection to the Azure Digital Twins Instance for Topology Ingestion.
    /// </summary>
    public class IngestionManagerOptions : WillowContextOptions
    {
        /// <summary>
        /// Gets or sets the Url of the ADTApi instance to connect to.
        /// </summary>
        [Required]
        public string AdtApiEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure Active Directory Tenant Id.
        /// </summary>
        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Maximum number of things to query from Mapped in a single call.
        /// </summary>
        public int ThingQueryBatchSize { get; set; } = 25;

        /// <summary>
        /// Gets or sets a value indicating whether or not to enable updates to the ADT instance.
        /// </summary>
        public bool EnableUpdates { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not to enable resetting the external id for all twins.
        /// </summary>
        public bool EnableResetExternalId { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not to enable Twin Replace.
        /// </summary>
        public bool EnableTwinReplace { get; set; } = false;
    }
}
