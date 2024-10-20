namespace Connector.Nunit.Tests.TestData
{
    using System;
    using System.Collections.Generic;
    using ConnectorCore.Entities;

    public class LogsTestData
    {
        public static List<LogRecordEntity> LogRecords = new List<LogRecordEntity>
        {
            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId1,
                StartTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2019, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                ErrorCount = 1,
                PointCount = 2,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId1,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(1),
                ErrorCount = 8,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId1,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(1),
                ErrorCount = 800,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.SenderLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId5,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(1),
                ErrorCount = 2,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId6,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(1),
                ErrorCount = 2,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId6,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(4),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                ErrorCount = 2,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow,
            },

            new LogRecordEntity
            {
                ConnectorId = Constants.ConnectorId6,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(5),
                EndTime = DateTime.UtcNow - TimeSpan.FromMinutes(3),
                ErrorCount = 2,
                PointCount = 12,
                RetryCount = 3,
                Errors = @"[{errorMessage: ""string"", deviceId: ""guid"", externaldeviceid: ""string""}]",
                Source = Constants.TestLogsSource,
                CreatedAt = DateTime.UtcNow
            },
        };
    }
}
