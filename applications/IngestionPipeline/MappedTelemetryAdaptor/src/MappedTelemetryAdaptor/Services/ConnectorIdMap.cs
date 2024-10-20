namespace Willow.MappedTelemetryAdaptor.Services;

/// <summary>
/// ConnectorMap.
/// </summary>
/// <param name="MappedConnectorId">Connector id.</param>
/// <param name="ExternalId">Id that mapped sends.</param>
public sealed record ConnectorIdMap(string ExternalId, string MappedConnectorId);
