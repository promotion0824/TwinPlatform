#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a customer.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class.
        /// </summary>
        public Customer()
        {
            CustomerInstances = new HashSet<CustomerInstance>();
        }

        /// <summary>
        /// Gets the identifier of the customer.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the name of the customer.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the short name of the customer.
        /// </summary>
        public string ShortName { get; init; }

        /// <summary>
        /// Gets the sales identifier of the customer.
        /// </summary>
        public string SalesId { get; init; }

        /// <summary>
        /// Gets the customer status identifier.
        /// </summary>
        public int CustomerStatusId { get; init; }

        /// <summary>
        /// Gets the customer instances associated with the customer.
        /// </summary>
        public virtual ICollection<CustomerInstance> CustomerInstances { get; init; }

        /// <summary>
        /// Gets the status of the customer.
        /// </summary>
        public virtual CustomerStatus CustomerStatus { get; init; }
    }
}
