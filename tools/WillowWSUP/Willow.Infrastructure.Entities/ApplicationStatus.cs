#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// The status of an application.
    /// </summary>
    public class ApplicationStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStatus"/> class.
        /// </summary>
        public ApplicationStatus()
        {
            Applications = new HashSet<Application>();
        }

        /// <summary>
        /// Gets the unique identifier for the application status.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets or sets the name of the application status.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the description of the application status.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the applications associated with the application status.
        /// </summary>
        public virtual ICollection<Application> Applications { get; init; }
    }
}
