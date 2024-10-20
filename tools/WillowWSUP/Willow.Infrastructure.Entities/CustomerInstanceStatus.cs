#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// The status of a customer instance.
    /// </summary>
    public class CustomerInstanceStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerInstanceStatus"/> class.
        /// </summary>
        public CustomerInstanceStatus()
        {
            CustomerInstances = new HashSet<CustomerInstance>();
        }

        /// <summary>
        /// Gets the identifier of the customer instance status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the customer instance status.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the customer instance status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the resource group name.
        /// </summary>
        public string ResourceGroupName { get; init; }

        /// <summary>
        /// Gets the Url for the ADX database.
        /// </summary>
        public string AdxDatabaseUrl { get; init; }

        /// <summary>
        /// Gets the Url for the ADT instance.
        /// </summary>
        public string AdtInstanceUrl { get; init; }

        /// <summary>
        /// Gets the customer instances associated with the customer instance status.
        /// </summary>
        public virtual ICollection<CustomerInstance> CustomerInstances { get; init; }
    }
}
