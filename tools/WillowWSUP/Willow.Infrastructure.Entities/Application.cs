#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents an application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            CustomerInstanceApplications = new HashSet<CustomerInstanceApplication>();
        }

        /// <summary>
        /// Gets the identifier of the application.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the container.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the Display name of the application.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the description of the application.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the team identifier responsible for the application.
        /// </summary>
        public int TeamId { get; init; }

        /// <summary>
        /// Gets the application status identifier.
        /// </summary>
        public int ApplicationStatusId { get; init; }

        /// <summary>
        /// Gets the path of the application.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// Gets the health endpoint path of the application.
        /// </summary>
        public string HealthEndpointPath { get; init; }

        /// <summary>
        /// Gets a value indicating whether the application has a publically expsosed endpoint.
        /// </summary>
        public bool HasPublicEndpoint { get; init; }

        /// <summary>
        /// Gets the role name of the application.
        /// </summary>
        public string RoleName { get;init; }

        /// <summary>
        /// Gets the team responsible for the application.
        /// </summary>
        public virtual Team Team { get; init; }

        /// <summary>
        /// Gets the status of the application.
        /// </summary>
        public virtual ApplicationStatus ApplicationStatus { get; init; }

        /// <summary>
        /// Gets the customer instance applications associated with the application.
        /// </summary>
        public virtual ICollection<CustomerInstanceApplication> CustomerInstanceApplications { get; init; }
    }
}
