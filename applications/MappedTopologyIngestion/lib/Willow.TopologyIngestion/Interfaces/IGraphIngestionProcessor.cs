//-----------------------------------------------------------------------
// <copyright file="IGraphIngestionProcessor.cs" Company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Interfaces
{
    using Willow.TopologyIngestion.Mapped;

    /// <summary>
    /// Methods for ingesting a graph from a source graph and inserting into a target graph.
    /// </summary>
    public interface IGraphIngestionProcessor
    {
        /// <summary>
        /// Gets the errors that occurred during the ingestion process.
        /// </summary>
        IDictionary<string, string> Errors { get; }

        /// <summary>
        /// Starts the asynchronous organization ingestion process from Mapped.
        /// </summary>
        /// <param name="autoApprove">Auto approve the creation of the twins.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task SyncOrganizationAsync(bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Starts the asynchronous connector ingestion process from Mapped for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="autoApprove">Auto approve the creation of the twins.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task SyncConnectorsAsync(string buildingId, bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Starts the asynchronous spatial ingestion process from Mapped for a building.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="autoApprove">Auto approve the creation of the twins.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task SyncSpatialAsync(string buildingId, bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Starts the asynchronous asset ingestion process from Mapped for a building and a connector.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="connectorId">The connector identifier.</param>
        /// <param name="autoApprove">Auto approve the creation of the twins.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task SyncThingsAsync(string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Starts the asynchronous capabilities ingestion process from Mapped for a building and a connector.
        /// </summary>
        /// <param name="buildingId">The building identifier.</param>
        /// <param name="connectorId">The connector identifier.</param>
        /// <param name="autoApprove">Auto approve the creation of the twins.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task SyncPointsAsync(string buildingId, string connectorId, bool autoApprove, CancellationToken cancellationToken);

        /// <summary>
        /// Get the connector metadata from the source graph.
        /// </summary>
        /// <param name="connectorId">The connector identifier.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An asynchronous task.</returns>
        Task<IEnumerable<TicketMetadataDto>> GetConnectorMetadataAsync(string connectorId, CancellationToken cancellationToken);
    }
}
