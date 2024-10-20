namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;
    using Willow.LiveData.Core.Infrastructure.Database.Adx;

    public class AdxLiveDataRepositoryTests
    {
        [Ignore("")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]
        public void AdxLiveDataRepository_GetTelemetryAsync_Returns_Telemetry(
            Guid clientId,
            Guid connectorId,
            DateTime start,
            DateTime end)
        {
            //arrange
            var adxQueryRunner = new Mock<IAdxQueryRunner>();

            var continuationTokenProvider = new AdxStoredQueryResultTokenProvider();
            var adxLiveDataRepository = new AdxLiveDataRepository(adxQueryRunner.Object, continuationTokenProvider);
            var dtIds = new List<string>() { Guid.NewGuid().ToString() };
            var trendIds = new List<Guid>() { Guid.NewGuid() };
            var query = adxLiveDataRepository.GetTelemetryStoredQueryResult(connectorId, start, end, dtIds, trendIds);
            var storedProcedureName = continuationTokenProvider.GetToken(query);
            var pagedQuery = $"stored_query_result('{storedProcedureName}') | where RowNumber between(1...10)";
            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunner.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);
            adxQueryRunner.Setup(x => x.QueryAsync(clientId, pagedQuery)).ReturnsAsync(mockDataReader.Object);

            //act
            var request = new GetTelemetryRequest(
                clientId,
                connectorId,
                start,
                end,
                10,
                string.Empty,
                dtIds,
                trendIds);
            var pagedTelemetry = adxLiveDataRepository.GetTelemetryAsync(request).Result;

            //assert
            pagedTelemetry.Telemetry.Count.Should().NotBe(0);
            pagedTelemetry.ContinuationToken.Should().Be(continuationTokenProvider.GetToken(query));
        }

        [Ignore("")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]

        public void AdxLiveDataRepository_GetTelemetryAsync_Only_Runs_Once(
            Guid clientId,
            Guid connectorId,
            DateTime start,
            DateTime end)
        {
            //arrange
            var adxQueryRunner = new Mock<IAdxQueryRunner>();

            var continuationTokenProvider = new AdxStoredQueryResultTokenProvider();
            var adxLiveDataRepository = new AdxLiveDataRepository(adxQueryRunner.Object, continuationTokenProvider);
            var dtIds = new List<string>() { Guid.NewGuid().ToString() };
            var trendIds = new List<Guid>() { Guid.NewGuid() };
            var query = adxLiveDataRepository.GetTelemetryStoredQueryResult(connectorId, start, end, dtIds, trendIds);
            var storedProcedureName = continuationTokenProvider.GetToken(query);
            var pagedQuery = $"stored_query_result('{storedProcedureName}') | where RowNumber between(1...10)";
            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunner.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);
            adxQueryRunner.Setup(x => x.QueryAsync(clientId, pagedQuery)).ReturnsAsync(mockDataReader.Object);

            //act
            var request = new GetTelemetryRequest(
                clientId,
                connectorId,
                start,
                end,
                10,
                string.Empty,
                dtIds,
                trendIds);
            var pagedTelemetry = adxLiveDataRepository.GetTelemetryAsync(request).Result;

            //assert
            adxQueryRunner.Verify(x => x.ControlQueryAsync(clientId, It.IsAny<string>()), Times.Once);
            adxQueryRunner.Verify(x => x.QueryAsync(clientId, pagedQuery), Times.Once);
        }

        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]
        public void AdxLiveDataRepository_GetTelemetryAsync_Throws_KustoClientException_When_Null_DataReader(
            Guid clientId,
            Guid connectorId,
            DateTime start,
            DateTime end)
        {
            //arrange
            var adxQueryRunner = new Mock<IAdxQueryRunner>();

            var continuationTokenProvider = new AdxStoredQueryResultTokenProvider();
            var adxLiveDataRepository = new AdxLiveDataRepository(adxQueryRunner.Object, continuationTokenProvider);
            var dtIds = new List<string>() { Guid.NewGuid().ToString() };
            var trendIds = new List<Guid>() { Guid.NewGuid() };
            var query = adxLiveDataRepository.GetTelemetryStoredQueryResult(connectorId, start, end, dtIds, trendIds);
            var storedProcedureName = continuationTokenProvider.GetToken(query);
            var pagedQuery = $"stored_query_result('{storedProcedureName}') | where RowNumber between(1...10)";
            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunner.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);

            //adxQueryRunner.Setup(x => x.QueryAsync(clientId, pagedQuery)).ReturnsAsync(mockDataReader.Object);

            //act
            var request = new GetTelemetryRequest(
                clientId,
                connectorId,
                start,
                end,
                10,
                string.Empty,
                dtIds,
                trendIds);
            Func<Task> action = async () => await adxLiveDataRepository.GetTelemetryAsync(request);
            action.Should().ThrowAsync<Kusto.Data.Exceptions.KustoClientException>();
        }

        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]

        public void AdxLiveDataRepository_GetTelemetryAsync_Throws_KustoException_When_Kusto_Operation_Failure(
            Guid clientId,
            Guid connectorId,
            DateTime start,
            DateTime end)
        {
            //arrange
            var adxQueryRunner = new Mock<IAdxQueryRunner>();

            var continuationTokenProvider = new AdxStoredQueryResultTokenProvider();
            var adxLiveDataRepository = new AdxLiveDataRepository(adxQueryRunner.Object, continuationTokenProvider);
            var dtIds = new List<string>() { Guid.NewGuid().ToString() };
            var trendIds = new List<Guid>() { Guid.NewGuid() };
            var query = adxLiveDataRepository.GetTelemetryStoredQueryResult(connectorId, start, end, dtIds, trendIds);
            var storedProcedureName = continuationTokenProvider.GetToken(query);
            var pagedQuery = $"stored_query_result('{storedProcedureName}') | where RowNumber between(1...10)";
            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunner.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);
            adxQueryRunner.Setup(x => x.QueryAsync(clientId, It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException());

            //act
            var request = new GetTelemetryRequest(
                clientId,
                connectorId,
                start,
                end,
                10,
                string.Empty,
                dtIds,
                trendIds);
            Func<Task> action = async () => await adxLiveDataRepository.GetTelemetryAsync(request);
            action.Should().ThrowAsync<Kusto.Data.Exceptions.KustoRequestException>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "This is a test and the fields only relate to the code below")]
        private const string Column1 = "ConnectorId";
        private const string Column2 = "DtId";
        private const string ExpectedValue1 = "Value1";
        private const string ExpectedValue2 = "Value1";

        private static Mock<IDataReader> CreateTelemetryDataReader()
        {
            var dataReader = new Mock<IDataReader>();

            dataReader.Setup(m => m.FieldCount).Returns(2);
            dataReader.Setup(m => m.GetName(0)).Returns(Column1);
            dataReader.Setup(m => m.GetName(1)).Returns(Column2);

            dataReader.Setup(m => m.GetFieldType(0)).Returns(typeof(string));
            dataReader.Setup(m => m.GetFieldType(1)).Returns(typeof(string));

            dataReader.Setup(m => m.GetOrdinal("First")).Returns(0);
            dataReader.Setup(m => m.GetValue(0)).Returns(ExpectedValue1);
            dataReader.Setup(m => m.GetValue(1)).Returns(ExpectedValue2);

            dataReader.SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);
            return dataReader;
        }
    }
}
