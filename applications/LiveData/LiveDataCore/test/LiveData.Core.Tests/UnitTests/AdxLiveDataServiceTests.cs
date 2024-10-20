namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Domain;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;

    public class AdxLiveDataServiceTests
    {
        [Ignore("")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10", 1000, "")]
        [TestCase("dd66b500-cac5-4201-951b-bee0655a65df", "bebaec82-b429-4de2-a9de-af1fbdee46c2", "2022-01-01 01:10", "2022-01-10 01:10", 1000, "XXXX-1000")]
        public async Task AdxLiveDataServiceTests_GetTelemetryAsync_Returns_Results(
            Guid clientId,
            Guid connectorId,
            DateTime start,
            DateTime end,
            int pageSize,
            string continuationToken)
        {
            // arrange
            var dateTimeIntervalService = new DateTimeIntervalService();
            var continuationTokenProvider = new AdxCTokenProvider();
            var adxLiveDataRepositoryMock = new Mock<IAdxLiveDataRepository>();
            var adxLiveDataService = new AdxLiveDataService(dateTimeIntervalService, adxLiveDataRepositoryMock.Object, continuationTokenProvider);
            List<Guid> trendIds = null;
            TelemetryRequestBody requestBody = new();

            var telemetryList = new List<Telemetry>();
            for (int i = 1; i < 5000; i++)
            {
                telemetryList.Add(new Telemetry() { RowNumber = i });
            }

            (var storedQueryNameResult, var rowNumber) = continuationTokenProvider.ParseToken(continuationToken);
            adxLiveDataRepositoryMock.Setup(x => x.GetTelemetryAsync(new GetTelemetryRequest(
                    clientId,
                    connectorId,
                    start,
                    end,
                    pageSize,
                    storedQueryNameResult,
                    requestBody.DtIds,
                    trendIds,
                    rowNumber)))
                .ReturnsAsync(new PagedTelemetry
                {
                    Telemetry = telemetryList.OrderBy(x => x.RowNumber).Skip(rowNumber).Take(pageSize).ToList(),
                    ContinuationToken = "XXXX",
                });

            // act
            var result = await adxLiveDataService.GetTelemetryAsync(
                clientId,
                requestBody,
                start,
                end,
                pageSize,
                continuationToken);

            // asset
            result.ContinuationToken.Should().NotBe(continuationToken);
            result.ContinuationToken.Should().EndWith($"XXXX-{pageSize + rowNumber}");
            result.Data.Count.Should().Be(1000);
        }
    }
}
