#nullable disable

using System.Globalization;

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a deployment stamp.
    /// </summary>
    public class Stamp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Stamp"/> class.
        /// </summary>
        public Stamp()
        {
            CustomerInstances = new HashSet<CustomerInstance>();
        }

        /// <summary>
        /// Gets the id of the stamp.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the short name of the stamp.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the subscription id of the stamp.
        /// </summary>
        public Guid SubscriptionId { get; init; }

        /// <summary>
        /// Gets the name of the environment for the stamp.
        /// </summary>
        public string EnvironmentName { get; init; }

        /// <summary>
        /// Gets the region short name of the stamp.
        /// </summary>
        public string RegionShortName { get; init; }

        /// <summary>
        /// Gets the customer instances associated with the stamp.
        /// </summary>
        public virtual ICollection<CustomerInstance> CustomerInstances { get; init; }

        /// <summary>
        /// Gets the region associated with the stamp.
        /// </summary>
        public virtual Region Region { get; init; }

        /// <summary>
        /// Gets the environment associated with the stamp.
        /// </summary>
        public virtual Environment Environment { get; init; }
    }
}
