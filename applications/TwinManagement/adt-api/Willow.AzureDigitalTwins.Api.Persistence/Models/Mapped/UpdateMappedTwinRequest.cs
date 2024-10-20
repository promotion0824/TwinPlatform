using System.ComponentModel.DataAnnotations;

namespace Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped
{
    /// <summary>
    /// MTI will make request for potential twin updates for TLM users to review.
    /// </summary>
    public class UpdateMappedTwinRequest
    {
        /// <summary>
        /// Id for twin update request.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Willow twin id.
        /// </summary>
        [MaxLength(256)]
        public required string WillowTwinId { get; set; }

        /// <summary>
        /// Collection of Json Patch Entries for Patching a Twin. Storing as string in database. 
        /// </summary>
        public required string ChangedProperties { get; set; }

        /// <summary>
        /// Time of when the update was requested.
        /// </summary>
        public required DateTimeOffset TimeCreated { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Time of when the update request was last updated.
        /// </summary>
        public required DateTimeOffset TimeLastUpdated { get; set; } = DateTimeOffset.UtcNow;

    }
}
