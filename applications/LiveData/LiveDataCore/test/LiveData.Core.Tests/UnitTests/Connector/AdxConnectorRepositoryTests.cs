namespace Willow.LiveData.Core.Tests.UnitTests.Connector
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Features.Connectors.Repositories;
    using Willow.LiveData.Core.Infrastructure.Database.Adx;

    internal class AdxConnectorRepositoryTests
    {
        [Ignore("Subject to discussion on cases")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10")]
        public void AdxConnectorRepository_GetCapabilityCountByLastXHoursAsync_Returns_CapabilityCount(
                                                                Guid clientId,
                                                                Guid connectorId,
                                                                DateTime start,
                                                                DateTime end)
        {
            //arrange
            var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
            var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
            var adxConnectorRepository = new AdxConnectorRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
            var connectorIds = new List<Guid>() { connectorId };

            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunnerMock.Setup(x => x.QueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);

            //act
            var result = adxConnectorRepository.GetTelemetryCountByLastXHoursAsync(
                                                                        clientId,
                                                                        connectorIds,
                                                                        start,
                                                                        end)
                                                                        .Result;

            //assert
            result.Count().Should().NotBe(0);
            adxQueryRunnerMock.Verify(x => x.QueryAsync(clientId, It.IsAny<string>()), Times.Once());
        }

        [Ignore("Subject to discussion on cases")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2")]
        public void AdxConnectorRepository_GetConnectorStatusAsync_Returns_ConnectorStatus(
                                                                Guid clientId,
                                                                Guid connectorId)
        {
            //arrange
            var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
            var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
            var adxConnectorRepository = new AdxConnectorRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
            var connectorIds = new List<Guid>() { connectorId };

            var mockDataReader = CreateTelemetryDataReader();
            adxQueryRunnerMock.Setup(x => x.QueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(mockDataReader.Object);

            //act
            var result = adxConnectorRepository.GetConnectorStatusAsync(
                                                                        clientId,
                                                                        connectorIds)
                                                                        .Result;

            //assert
            result.Count().Should().NotBe(0);
            adxQueryRunnerMock.Verify(x => x.QueryAsync(clientId, It.IsAny<string>()), Times.Once());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "This is a test and the fields only relate to the code below")]
        private const string Column1 = "ConnectorId";
        private const string Column2 = "Timestamp";
        private static Guid expectedValue1 = Guid.NewGuid();
        private static DateTime expectedValue2 = DateTime.Now;

        private static Mock<IDataReader> CreateTelemetryDataReader()
        {
            var dataReader = new Mock<IDataReader>();

            dataReader.Setup(m => m.FieldCount).Returns(2);
            dataReader.Setup(m => m.GetName(0)).Returns(Column1);
            dataReader.Setup(m => m.GetName(1)).Returns(Column2);

            dataReader.Setup(m => m.GetFieldType(0)).Returns(typeof(Guid));
            dataReader.Setup(m => m.GetFieldType(1)).Returns(typeof(DateTime));

            dataReader.Setup(m => m.GetOrdinal("First")).Returns(0);
            dataReader.Setup(m => m.GetValue(0)).Returns(expectedValue1);
            dataReader.Setup(m => m.GetValue(1)).Returns(expectedValue2);

            dataReader.SetupSequence(m => m.Read())
                .Returns(true)
                .Returns(true)
                .Returns(false);
            return dataReader;
        }
    }
}
