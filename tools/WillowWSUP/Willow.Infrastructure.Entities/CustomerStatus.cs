#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents the status of a customer.
    /// </summary>
    public class CustomerStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerStatus"/> class.
        /// </summary>
        public CustomerStatus()
        {
            Customers = new HashSet<Customer>();
        }

        /// <summary>
        /// Gets the identifier of the customer status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the customer status.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the customer status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the customers associated with the customer status.
        /// </summary>
        public virtual ICollection<Customer> Customers { get; init; }
    }
}
