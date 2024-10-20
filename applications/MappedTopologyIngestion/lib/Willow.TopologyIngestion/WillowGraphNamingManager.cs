namespace Willow.TopologyIngestion
{
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Provides a way to name relationships in the graph.
    /// </summary>
    public class WillowGraphNamingManager : IGraphNamingManager
    {
        /// <summary>
        /// Gets the name of a relationship between two twins.
        /// </summary>
        /// <param name="sourceTwinId">The id of the source twin.</param>
        /// <param name="targetTwinId">The id of the target twin.</param>
        /// <param name="relationshipType">The relationship twin.</param>
        /// <param name="properties">A collection of properties of the relationship that will factor into the name of the relationship.</param>
        /// <returns>A name for the relationship.</returns>
        public string GetRelationshipName(string sourceTwinId, string targetTwinId, string relationshipType, IDictionary<string, object> properties)
        {
            var baseRelationshipName = $"{sourceTwinId}_{relationshipType}_{targetTwinId}";

            if (properties == null || properties.Count == 0)
            {
                return baseRelationshipName;
            }

            foreach (var property in properties)
            {
                baseRelationshipName += $"_{property.Key}_{property.Value}";
            }

            return baseRelationshipName;
        }
    }
}
