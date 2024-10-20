namespace Willow.TopologyIngestion
{
    using Willow.TopologyIngestion.Interfaces;

    /// <summary>
    /// Use this when it is not needed to use the Redis Cache for Managing Twins.
    /// </summary>
    public class NullTwinMappingIndexer : ITwinMappingIndexer
    {
        /// <inheritdoc/>
        // This method is not used in this app
        public Task<TwinMapEntry?> GetTwinIndexAsync(string sourceId)
        {
            return Task.FromResult(null as TwinMapEntry);
        }

        /// <inheritdoc/>
        public Task UpsertTwinIndexAsync(string sourceId, TwinMapEntry mapEntry)
        {
            return Task.CompletedTask;
        }
    }
}
