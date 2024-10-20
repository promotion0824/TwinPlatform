namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Domain;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;
    using Willow.LiveData.Core.Infrastructure.Database;
    using Willow.LiveData.Core.Infrastructure.Database.Adx;

    public class PointLiveDataServiceTests
    {
        [TestCase]
        [Ignore("")]
        public async Task AdxLiveDataRepository_GetTelemetryAsync_Only_Runs_Once()
        {
            //arrange
            var adxQueryRunner = new Mock<IAdxQueryRunner>();

            var continuationTokenProvider = new AdxStoredQueryResultTokenProvider();
            var adxLiveDataRepository = new AdxLiveDataRepository(adxQueryRunner.Object, continuationTokenProvider);

            var connectorId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var fromDate = DateTime.Now.AddMinutes(-30);
            var toDate = DateTime.Now;
            var dtIds = new List<string>() { Guid.NewGuid().ToString() };
            var trendIds = new List<Guid>() { Guid.NewGuid() };
            var query = adxLiveDataRepository.GetTelemetryStoredQueryResult(connectorId, fromDate, toDate, dtIds, trendIds);
            var storedProcedureName = continuationTokenProvider.GetToken(query);
            var pagedQuery = $"stored_query_result('{storedProcedureName}') | where RowNumber between(1...10)";
            var mockDataReader = CreateTelemetryDataReader();
            var mockSqrDataReader = CreateSQRDataReader(storedProcedureName);

            adxQueryRunner.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);
            adxQueryRunner.Setup(x => x.QueryAsync(It.Is<Guid>(c => c == clientId), It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);
            adxQueryRunner.Setup(x => x.QueryAsync(It.Is<Guid>(c => c == clientId), It.IsAny<string>())).ReturnsAsync(mockSqrDataReader.Object);

            //act
            var request = new GetTelemetryRequest(
                clientId,
                connectorId,
                fromDate,
                toDate,
                10,
                string.Empty,
                dtIds,
                trendIds);
            var pagedTelemetry = await adxLiveDataRepository.GetTelemetryAsync(request);

            //assert
            adxQueryRunner.Verify(x => x.ControlQueryAsync(clientId, It.IsAny<string>()), Times.Once);
            adxQueryRunner.Verify(x => x.QueryAsync(clientId, pagedQuery), Times.Exactly(2));
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

        private static Mock<IDataReader> CreateSQRDataReader(string continuationToken)
        {
            var dataReader = new Mock<IDataReader>();

            dataReader.Setup(m => m.FieldCount).Returns(1);
            dataReader.Setup(m => m.GetName(0)).Returns("Name");

            dataReader.Setup(m => m.GetFieldType(0)).Returns(typeof(string));

            dataReader.Setup(m => m.GetOrdinal("Name")).Returns(0);
            dataReader.Setup(m => m.GetValue(0)).Returns(continuationToken);

            dataReader.SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(false);
            return dataReader;
        }
    }
}
