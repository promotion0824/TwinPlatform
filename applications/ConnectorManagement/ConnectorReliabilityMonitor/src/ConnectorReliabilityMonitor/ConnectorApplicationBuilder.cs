namespace Willow.ConnectorReliabilityMonitor;

using Microsoft.Extensions.Options;
using Willow.Adx;

internal class ConnectorApplicationBuilder(IAdxService adxService,
                                            IOptions<ConnectorOverridesOption> connectorOverridesOption,
                                            IMetricsCollector metricsCollector) : IConnectorApplicationBuilder
{
    private readonly ConnectorOverridesOption connectorOverrides = connectorOverridesOption.Value;
    private const string ConnectorApplicationModelId = "dtmi:com:willowinc:ConnectorApplication;1";
    private const string BuildingRelationshipName = "servedBy";
    private const string DefaultMappedConnectorId = "00000000-35c5-4415-a4b3-7b798d0568e8";
    private const int DefaultIntervalInSeconds = 300;

    public async Task<IEnumerable<ConnectorApplicationDto>> GetConnectorsAsync()
    {
        var connectorApplicationParameters = new Dictionary<string, string>
        {
            { nameof(ConnectorApplicationModelId), ConnectorApplicationModelId },
            { nameof(BuildingRelationshipName), BuildingRelationshipName },
        };

        var connectorStateList = (await adxService.QueryAsync<ConnectorApplicationDto>("ConnectorState | where IsArchived == false | summarize arg_max(TimestampUTC, *) by ConnectorId | extend Id = tostring(ConnectorId)")).ToList();

        var connectorApplications = (await adxService.QueryAsync<ConnectorApplicationDto>(
                """
                declare query_parameters(ConnectorApplicationModelId:string, BuildingRelationshipName:string);

                ActiveTwins
                | where ModelId == ConnectorApplicationModelId
                | join kind=leftouter ActiveRelationships on $left.Id == $right.TargetId
                | where Name1 in (BuildingRelationshipName, "")
                | join kind = leftouter ActiveTwins on $left.SourceId == $right.Id
                | extend ConnectorType = tostring(Raw["customProperties"]["connectorType"]["id"])
                | summarize Buildings = make_list(Name2) by Id, Name, ConnectorType
                """,
                connectorApplicationParameters))
            .ToList();

        connectorApplications.Add(CreateDefaultMappedConnectorState());

        UpdateConnectorsList(connectorStateList, connectorApplications);

        LogIntervalMetric(connectorApplications);

        return connectorApplications;
    }

    private static ConnectorApplicationDto CreateDefaultMappedConnectorState()
    {
        return new ConnectorApplicationDto
        {
            Id = DefaultMappedConnectorId,
            Name = "Default Mapped Connector",
            IsEnabled = true,
            Interval = DefaultIntervalInSeconds,
            ConnectorType = "mapped",
        };
    }

    private void UpdateConnectorsList(List<ConnectorApplicationDto> connectors, List<ConnectorApplicationDto> connectorApplications)
    {
        foreach (var connectorApplication in connectorApplications)
        {
            if (string.IsNullOrEmpty(connectorApplication.Name))
            {
                continue;
            }

            var connectorOverride = connectorOverrides.Overrides.FirstOrDefault(c => c.Name == connectorApplication.Name);
            if (connectorOverride is not null)
            {
                connectorApplication.Interval = connectorOverride.Interval;
            }
        }

        foreach (var connector in connectors.ToList())
        {
            var connectorApplication = connectorApplications.FirstOrDefault(c => c.Id == connector.Id);

            if (connectorApplication != null)
            {
                connectorApplication.IsEnabled = connector.IsEnabled;
                connectorApplication.Interval = connector.Interval;
            }
            else if (string.IsNullOrEmpty(connector.Name))
            {
                connectors.Remove(connector);
            }
        }
    }

    private void LogIntervalMetric(List<ConnectorApplicationDto> connectors)
    {
        const string metricName = "ConnectorInterval";

        foreach (var connector in connectors)
        {
            var dimensions = new Dictionary<string, string>
            {
                { "ConnectorName", connector.Name },
                { "IsEnabled", connector.IsEnabled.ToString() },
                { "Source", connector.Source },
            };

            metricsCollector.TrackMetric(metricName, connector.Interval, MetricType.Counter, "Connector collection interval", dimensions);
        }
    }
}
