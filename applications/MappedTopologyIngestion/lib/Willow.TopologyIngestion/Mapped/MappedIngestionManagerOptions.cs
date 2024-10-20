//-----------------------------------------------------------------------
// <copyright file="MappedIngestionManagerOptions.cs" company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped
{
    using System.ComponentModel.DataAnnotations;
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Configuration settings needed for connecting to an instance of the Mapped Graph API.
    /// </summary>
    public class MappedIngestionManagerOptions : IngestionManagerOptions, IInputGraphManagerOptions
    {
        /// <summary>
        /// Gets or sets the value of the Uri used for connecting to the Mapped Graph API.
        /// </summary>
        [Required]
        public string MappedRootUrl { get; set; } = string.Empty;
    }
}
