#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a status of a application deployed to a customer instance.
    /// </summary>
    public class CustomerInstanceApplicationStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerInstanceApplicationStatus"/> class.
        /// </summary>
        public CustomerInstanceApplicationStatus()
        {
            CustomerInstanceApplications = new HashSet<CustomerInstanceApplication>();
        }

        /// <summary>
        /// Gets the identifier of the customer instance application status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the customer instance application status.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the customer instance application status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the customer instance applications associated with the customer instance application status.
        /// </summary>
        public virtual ICollection<CustomerInstanceApplication> CustomerInstanceApplications { get; init; }
    }
}
