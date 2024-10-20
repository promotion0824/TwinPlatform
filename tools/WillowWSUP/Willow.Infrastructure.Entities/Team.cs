#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// The Application development team.
    /// </summary>
    public class Team
    {
        public Team()
        {
            Applications = new HashSet<Application>();
        }

        /// <summary>
        /// Gets the unique identifier for the team.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the name of the team in the organization.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the team.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the applications that the team is responsible for.
        /// </summary>
        public virtual ICollection<Application> Applications { get; init; }
    }
}
