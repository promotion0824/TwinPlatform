#nullable disable

namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Represents an application.
    /// </summary>
    public class Environment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        public Environment()
        {
            Stamps = new HashSet<Stamp>();
        }

        /// <summary>
        /// Gets the name of the environment.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the description of the environment.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets the stamps associated with the environment.
        /// </summary>
        public virtual ICollection<Stamp> Stamps { get; init; }
    }
}
