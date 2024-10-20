#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents a application deployed to a customer instance.
    /// </summary>
    public class CustomerInstanceApplication
    {
        /// <summary>
        /// Gets the identifier of the customer instance.
        /// </summary>
        public Guid CustomerInstanceId { get; init; }

        /// <summary>
        /// Gets the identifier of the application.
        /// </summary>
        public int ApplicationId { get; init; }

        /// <summary>
        /// Gets the status of the application on the customer instance.
        /// </summary>
        public int CustomerInstanceApplicationStatusId { get; init; }

        /// <summary>
        /// Gets the customer instance associated with the application.
        /// </summary>
        public virtual CustomerInstance CustomerInstance { get; init; }

        /// <summary>
        /// Gets the application associated with the customer instance.
        /// </summary>
        public virtual Application Application { get; init; }

        /// <summary>
        /// Gets the status of the application on the customer instance.
        /// </summary>
        public virtual CustomerInstanceApplicationStatus CustomerInstanceApplicationStatus { get; init; }
    }
}
