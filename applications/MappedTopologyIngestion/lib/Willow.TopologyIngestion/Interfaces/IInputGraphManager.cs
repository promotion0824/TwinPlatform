//-----------------------------------------------------------------------
// <copyright file="IInputGraphManager.cs" Company="Willow">
//   Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Interfaces
{
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Methods for accessing an input graph source.
    /// </summary>
    public interface IInputGraphManager
    {
        /// <summary>
        /// Get a DTMI for an interfaceType.
        /// </summary>
        /// <param name="interfaceType">The name of the interface.</param>
        /// <param name="dtmi">The found DTMI.</param>
        /// <returns><c>true</c> if the DTMi is found, otherwise <c>false</c>.</returns>
        public bool TryGetDtmi(string interfaceType, out string dtmi);

        /// <summary>
        /// Loads a twin graph from a source based on a passed in graph query.
        /// </summary>
        /// <param name="query">A well-formed graph query.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>A JsonDocument containing the results of the query.</returns>
        public Task<JsonDocument?> GetTwinGraphAsync(string query, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a graph query to return the connectors for an organization.
        /// </summary>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetConnectorsQuery();

        /// <summary>
        /// Gets a graph query to return the connectors for a building.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetBuildingConnectorsQuery(string buildingId);

        /// <summary>
        /// Gets a graph query to return an organization.
        /// </summary>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetOrganizationQuery();

        /// <summary>
        /// Gets a graph query to return all accounts for an organization.
        /// </summary>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetAccountsQuery();

        /// <summary>
        /// Gets a graph query to return all the buildings on a site.
        /// </summary>
        /// <param name="siteId">Twin ID of site.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetBuildingsForSiteQuery(string siteId);

        /// <summary>
        /// Gets a graph query to return a the building.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetBuildingQuery(string buildingId);

        /// <summary>
        /// Gets a graph query to return all the floors for a building.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetFloorQuery(string buildingId);

        /// <summary>
        /// Gets a graph query to return all the things in a building for a connector.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <param name="connectorId">Twin ID of connector. Use string.Empty for all connectors.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetBuildingThingsQuery(string buildingId, string connectorId);

        /// <summary>
        /// Gets a graph query to return all the BMS Points for a thing
        /// (e.g., a room, a piece of equipment, some smart furniture, or any
        /// other asset that can have data points assigned).
        /// </summary>
        /// <param name="thingIds">List of Twin IDs of things.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetPointsForThingsQuery(IList<string> thingIds);

        /// <summary>
        /// Gets a graph query to return all the connector metadata for a connector.
        /// </summary>
        /// <param name="connectorId">The connectorId to query.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string, CancellationToken)"/>.</returns>
        public string GetConnectorMetadataQuery(string connectorId);
    }
}
