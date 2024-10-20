namespace Connector.Nunit.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Connector.Nunit.Tests.Infrastructure.Extensions;
    using Connector.Nunit.Tests.TestData;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using FluentAssertions;
    using NUnit.Framework;
    using Snapshooter.NUnit;

    public class LogsTests
    {
        [Test]
        public async Task GetConnectorStatuses_ReturnsStatuses()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var statusRecords = await client.GetJsonAsync<List<ConnectorStatusRecord>>("logs/healthcheck");
                statusRecords.Should().HaveCount(cnt => cnt > 0);

                statusRecords.Should().Contain(r => r.ConnectorId == Constants.ConnectorId1);
                var status1 = statusRecords.First(r => r.ConnectorId == Constants.ConnectorId1);
                status1.SourceStatuses.Should().Contain(s => s.Source == Constants.TestLogsSource && s.Status == ConnectorStatus.OnlineWithErrors);
                status1.OverallStatus.Should().Be(ConnectorStatus.OnlineWithErrors);
                status1.PointsCount.Should().Be(4);
                status1.SiteId.Should().Be(Constants.SiteIdDefault);
                status1.ClientId.Should().Be(Constants.ClientIdDefault);
                status1.SourceStatuses.Should().NotContain(s => s.Source == Constants.SenderLogsSource);

                statusRecords.Should().Contain(r => r.ConnectorId == Constants.ConnectorId2);
                var status2 = statusRecords.First(r => r.ConnectorId == Constants.ConnectorId2);
                status2.SourceStatuses.Should().Contain(s => s.Status == ConnectorStatus.Offline);
                status2.OverallStatus.Should().Be(ConnectorStatus.Offline);
                status2.PointsCount.Should().BeNull();
                status2.SiteId.Should().Be(Constants.SiteIdDefault);
                status2.ClientId.Should().Be(Constants.ClientIdDefault);

                statusRecords.Should().Contain(r => r.ConnectorId == Constants.ConnectorId5);
                var status5 = statusRecords.First(r => r.ConnectorId == Constants.ConnectorId5);
                status5.SourceStatuses.Should().Contain(s => s.Source == Constants.TestLogsSource && s.Status == ConnectorStatus.Online);
                status5.OverallStatus.Should().Be(ConnectorStatus.Online);
                status5.PointsCount.Should().Be(10);
                status5.SiteId.Should().Be(Constants.SiteIdDefault);
                status5.ClientId.Should().Be(Constants.ClientIdDefault);
            }
        }

        [Test]
        public async Task GetLatestLogRecordForConnector_ReturnsLatestLogRecord()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connectorLogs = await client.GetJsonAsync<List<LogRecordEntity>>($"connectors/{Constants.ConnectorId1}/logs/latest");
                connectorLogs.Should().NotBeNull();
                connectorLogs.Should().NotBeEmpty();
                foreach (var logRecordEntity in connectorLogs)
                {
                    logRecordEntity.ConnectorId.Should().Be(Constants.ConnectorId1);
                    logRecordEntity.Source.Should().NotBeNullOrEmpty();
                }
            }
        }

        [Test]
        public async Task GetLatestLogRecordForConnectors_ReturnsLatestLogRecord()
        {
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var connectorLogs = await client.GetJsonAsync<List<LogRecordEntity>>($"connectors/logs/latest?connectorIds={Constants.ConnectorId1}&connectorIds={Constants.ConnectorId6}");
            connectorLogs.Should().NotBeNull();
            connectorLogs.Should().NotBeEmpty();
            connectorLogs.Select(i => i.ConnectorId).Distinct().Count().Should().Be(2);
            connectorLogs.MatchSnapshot(options => options.IgnoreAllFields("CreatedAt").IgnoreAllFields("StartTime").IgnoreAllFields("EndTime"));
        }

        [Test]
        public async Task GetLatestLogRecordForConnectors_ReturnsLogsByPerControllerWhenCountFiltered()
        {
            var countPerController = 2;
            using var client = IntegrationFixture.Server.CreateClientRandomUser();
            var connectorLogs = await client.GetJsonAsync<List<LogRecordEntity>>($"connectors/logs/latest?connectorIds={Constants.ConnectorId1}&connectorIds={Constants.ConnectorId6}&count={countPerController}");
            connectorLogs.Should().NotBeNull();
            connectorLogs.Should().NotBeEmpty();
            connectorLogs.Select(i => i.ConnectorId).Count().Should().Be(4);
        }

        [Test]
        public async Task GetLatestLogRecordForConnector_NonExistentSource_ReturnsEmptySet()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var connectorLogs = await client.GetJsonAsync<List<LogRecordEntity>>($"connectors/{Constants.ConnectorId1}/logs/latest?source=NoSource");
                connectorLogs.Should().NotBeNull();
                connectorLogs.Should().BeEmpty();
            }
        }

        [Test]
        public async Task CreateLogRecord_ReturnsNewLogRecord()
        {
            using (var client = IntegrationFixture.Server.CreateClientRandomUser())
            {
                var newLog = new LogRecordEntity
                {
                    ConnectorId = Constants.ConnectorId1,
                    StartTime = new DateTime(2019, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2019, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    ErrorCount = 11,
                    PointCount = 21,
                    RetryCount = 31,
                    Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                    Source = "TestSource",
                };
                var response = await client.PostFormAsync<LogRecordEntity>("logs", newLog);
                response.Id.Should().NotBe(0);
            }
        }
    }
}
