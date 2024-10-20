#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a region.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class.
        /// </summary>
        public Region()
        {
            CustomerInstances = new HashSet<CustomerInstance>();
            Stamps = new HashSet<Stamp>();
        }

        /// <summary>
        /// Gets the short name of the region.
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the display name of the region.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the customer instances associated with the region.
        /// </summary>
        public virtual ICollection<CustomerInstance> CustomerInstances { get; init; }

        /// <summary>
        /// Gets the stamps associated with the region.
        /// </summary>
        public virtual ICollection<Stamp> Stamps { get; init; }
    }
}
