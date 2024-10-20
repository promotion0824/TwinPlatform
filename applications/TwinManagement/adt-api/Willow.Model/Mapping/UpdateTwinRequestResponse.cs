namespace Willow.Model.Mapping
{
    /// <summary>
    /// MTI will make request for potential twin updates for TLM users to review 
    /// </summary>
    public class UpdateMappedTwinRequestResponse
    {
        /// <summary>
        /// Id for twin update request
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Willow twin id
        /// </summary>
        public required string WillowTwinId { get; set; }

        /// <summary>
        /// Collection of Json Patch Entries for Patching a Twin
        /// </summary>
        public required List<JsonPatchOperation> ChangedProperties { get; set; }

        /// <summary>
        /// Time of when the update was requested
        /// </summary>
        public required DateTimeOffset TimeCreated { get; set; } 

        /// <summary>
        /// Time of when the update request was last updated
        /// </summary>
        public required DateTimeOffset TimeLastUpdated { get; set; }

    }
}
