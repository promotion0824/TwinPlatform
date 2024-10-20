//-----------------------------------------------------------------------
// <copyright file="IOutputGraphManager.cs" Company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Interfaces
{
    using System.Threading.Tasks;
    using global::Azure.DigitalTwins.Core;

    /// <summary>
    /// Methods for working with an output graph.
    /// </summary>
    public interface IOutputGraphManager
    {
        /// <summary>
        /// Gets a dictionary of errors that occurred during the last upload.
        /// </summary>
        public Dictionary<string, string> Errors { get; }

        /// <summary>
        /// Asynchronously loads the model for a graph.
        /// </summary>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>Task wrapping a collection of strings which describe the model used by the output graph.</returns>
        public Task<IEnumerable<string>> GetModelsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously loads the twins and relationships into an output graph.
        /// </summary>
        /// <param name="twins">Twins to be loaded into output graph.</param>
        /// <param name="relationships">Relationships to be loaded into output graph.</param>
        /// <param name="buildingId">The building id which was passed in to the request.</param>
        /// <param name="connectorId">The connector id which was passed in to the request.</param>
        /// <param name="autoApprove">Whether or not to autoapprove the twin creation.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>An awaitable task.</returns>
        public Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships, string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the siteId for a building.
        /// </summary>
        /// <param name="buildingId">The Willow Building Id.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>The Willow Site Id.</returns>
        public Task<string> GetSiteIdForBuilding(string buildingId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the siteId for a building.
        /// </summary>
        /// <param name="buildingId">The Mapped Building Id.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>The Willow Site Id.</returns>
        public Task<string> GetSiteIdForMappedBuildingId(string buildingId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the WillowId for a twin given a MappedId.
        /// </summary>
        /// <param name="mappedId">The Mapped Id.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>The Willow Twin.</returns>
        public Task<BasicDigitalTwin?> GetTwinForMappedId(string mappedId, CancellationToken cancellationToken);
    }
}
