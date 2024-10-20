namespace Willow.Infrastructure.Entities
{
    /// <summary>
    /// Deployments are phased with approvals between each step.
    /// </summary>
    public enum DeploymentPhase
    {
        /// <summary>
        /// Continuous Integration.
        /// </summary>
        CI = 1,

        /// <summary>
        /// Pre-Production Environment.
        /// </summary>
        PPE = 2,

        /// <summary>
        /// Pilot Stage. Early adopters.
        /// </summary>
        Pilot = 3,

        /// <summary>
        /// Medium Stage. Small to midsize customers tolerant of some instability.
        /// </summary>
        Medium = 4,

        /// <summary>
        /// Heavy Stage. Large customers with high expectations for stability.
        /// </summary>
        Heavy = 5,

        /// <summary>
        /// Public Stage. Publicly available to all customers.
        /// </summary>
        Public = 6,

        /// <summary>
        /// Walmart Stage. Walmart specific deployment which may have deployment restrictions.
        /// </summary>
        Walmart = 7,
    }
}
