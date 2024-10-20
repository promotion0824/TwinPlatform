namespace Willow.Infrastructure.Entities
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Allowed customer instance states.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LifeCycleState
    {
        /// <summary>
        /// Waiting for someone to set status.
        /// </summary>
        Unknown,

        /// <summary>
        /// The customer instance has recently been created and is empty.
        /// </summary>
        Empty,

        /// <summary>
        /// The customer instance is being commissioned.
        /// </summary>
        Commissioning,

        /// <summary>
        /// The customer instance is live.
        /// </summary>
        Live,

        /// <summary>
        /// The customer instance is being decommissioned.
        /// </summary>
        Decommissioning,

        /// <summary>
        /// The customer instance has been decomissioned and will be deleted soon.
        /// </summary>
        Decommissioned,
    }
}
