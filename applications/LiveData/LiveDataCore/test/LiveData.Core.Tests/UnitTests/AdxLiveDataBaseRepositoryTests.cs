namespace Willow.LiveData.Core.Tests.UnitTests;

using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Infrastructure.Database.Adx;

public class AdxLiveDataBaseRepositoryTests
{
    [Ignore("")]
    [TestCase("bebaec82-b429-4de2-a9de-af1fbdee46c2", "Telemtry | Take 4", "10", "0", "")]
    public void AdxLiveDataBaseRepository_CreatePagedQueryAsync_Returns_Valid_PagedQuery_When_No_ContinuationToken(
                                                                Guid clientId,
                                                                string query,
                                                                int pageSize,
                                                                int lastRowNumber,
                                                                string continuationToken)
    {
        //arrange
        var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
        var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
        var adxLiveDataTestRepository = new AdxLiveDataTestRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
        adxQueryRunnerMock.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(It.IsAny<IDataReader>());
        continuationTokenProviderMock.Setup(x => x.GetToken(query)).Returns(continuationToken);

        //act
        (var pagedQuery, var storedQueryResultName, int totalRowsCount) = adxLiveDataTestRepository.CreatePagedQueryAsync(clientId, query, pageSize, lastRowNumber, continuationToken).Result;

        //assert
        storedQueryResultName.Should().NotBeNull();
        pagedQuery.Should().Be($"stored_query_result('{storedQueryResultName}') | where RowNumber between({lastRowNumber + 1}...{lastRowNumber + pageSize})");
    }

    [Ignore("")]
    [TestCase("bebaec82-b429-4de2-a9de-af1fbdee46c2", "Telemtry | Take 4", "10", "0", "x1231232")]
    public void AdxLiveDataBaseRepository_CreatePagedQueryAsync_Returns_Valid_PagedQuery_When_ContinuationToken_Provided(
                                                                Guid clientId,
                                                                string query,
                                                                int pageSize,
                                                                int lastRowNumber,
                                                                string continuationToken)
    {
        //arrange
        var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
        var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
        var adxLiveDataTestRepository = new AdxLiveDataTestRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
        adxQueryRunnerMock.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(It.IsAny<IDataReader>());
        continuationTokenProviderMock.Setup(x => x.GetToken(query)).Returns(continuationToken);

        //act
        (var pagedQuery, var storedQueryResultName, int totalRowsCount) = adxLiveDataTestRepository.CreatePagedQueryAsync(clientId, query, pageSize, lastRowNumber, continuationToken).Result;

        //assert
        storedQueryResultName.Should().NotBeNull();
        storedQueryResultName.Should().Be(continuationToken);
        pagedQuery.Should().Be($"stored_query_result('{storedQueryResultName}') | where RowNumber between({lastRowNumber + 1}...{lastRowNumber + pageSize})");
    }

    [Ignore("")]
    [TestCase("bebaec82-b429-4de2-a9de-af1fbdee46c2", "Telemtry | Take 4", "10", "0", "")]
    public void AdxLiveDataBaseRepository_CreatePagedQueryAsync_Creates_SQR_When_No_ContinuationToken(
                                                                Guid clientId,
                                                                string query,
                                                                int pageSize,
                                                                int lastRowNumber,
                                                                string continuationToken)
    {
        //arrange
        var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
        var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
        var adxLiveDataTestRepository = new AdxLiveDataTestRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
        adxQueryRunnerMock.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(It.IsAny<IDataReader>());
        continuationTokenProviderMock.Setup(x => x.GetToken(query)).Returns(continuationToken);

        //act
        (var pagedQuery, var storedQueryResultName, int totalRowsCount) = adxLiveDataTestRepository.CreatePagedQueryAsync(clientId, query, pageSize, lastRowNumber, continuationToken).Result;

        //assert
        storedQueryResultName.Should().NotBeNull();
        pagedQuery.Should().Be($"stored_query_result('{storedQueryResultName}') | where RowNumber between({lastRowNumber + 1}...{lastRowNumber + pageSize})");
        adxQueryRunnerMock.Verify(x => x.ControlQueryAsync(clientId, It.IsAny<string>()), Times.Once());
    }

    [Ignore("")]
    [TestCase("bebaec82-b429-4de2-a9de-af1fbdee46c2", "Telemtry | Take 4", "10", "0", "x1231232")]
    public void AdxLiveDataBaseRepository_CreatePagedQueryAsync_Does_Not_Create_SQR_When_ContinuationToken_Provided(
                                                                Guid clientId,
                                                                string query,
                                                                int pageSize,
                                                                int lastRowNumber,
                                                                string continuationToken)
    {
        //arrange
        var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
        var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
        var adxLiveDataTestRepository = new AdxLiveDataTestRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
        adxQueryRunnerMock.Setup(x => x.ControlQueryAsync(clientId, It.IsAny<string>())).ReturnsAsync(It.IsAny<IDataReader>());

        var dataReaderMock = new Mock<IDataReader>();
        var values = new List<StoredQueryResult>();
        for (var i = 0; i < 1; i++)
        {
            values.Add(new StoredQueryResult() { Name = continuationToken });
        }

        continuationTokenProviderMock.Setup(x => x.GetToken(query)).Returns(continuationToken);

        //act
        (var pagedQuery, var storedQueryResultName, int totalRowsCount) = adxLiveDataTestRepository.CreatePagedQueryAsync(clientId, query, pageSize, lastRowNumber, continuationToken).Result;

        //assert
        storedQueryResultName.Should().NotBeNull();
        storedQueryResultName.Should().Be(continuationToken);
        pagedQuery.Should().Be($"stored_query_result('{storedQueryResultName}') | where RowNumber between({lastRowNumber + 1}...{lastRowNumber + pageSize})");
        adxQueryRunnerMock.Verify(x => x.ControlQueryAsync(clientId, It.IsAny<string>()), Times.Never);
    }

    [TestCase]
    public void AdxLiveDataBaseRepository_GetdtIdsClause_Returns_Valid_Where_Clause_For_ADX_Query()
    {
        //arrange
        var adxQueryRunnerMock = new Mock<IAdxQueryRunner>();
        var continuationTokenProviderMock = new Mock<IContinuationTokenProvider<string, string>>();
        var adxLiveDataTestRepository = new AdxLiveDataTestRepository(adxQueryRunnerMock.Object, continuationTokenProviderMock.Object);
        var list = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(Guid.NewGuid().ToString());
        }

        //act
        var whereClause = adxLiveDataTestRepository.GetdtIdsClause(list);

        //assert
        whereClause.Should().StartWith("| where DtId in (");
        whereClause.Should().EndWith(")");
        foreach (var item in list)
        {
            whereClause.Should().Contain($"'{item}'");
        }
    }

    [TestCase]
    public void AdxLiveDataBaseRepository_GetTrendIdsClause_Returns_Valid_Where_Clause_For_ADX_Query()
    {
        //arrange
        var list = new List<Guid>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(Guid.NewGuid());
        }

        //act
        var whereClause = AdxLiveDataTestRepository.GetTrendIdsClause(list);

        //assert
        whereClause.Should().StartWith("| where TrendId in (");
        whereClause.Should().EndWith(")");
        foreach (var item in list)
        {
            whereClause.Should().Contain($"'{item}'");
        }
    }

    [TestCase]
    public void AdxLiveDataBaseRepository_GetConnectorIdClause_Returns_Valid_Where_Clause_For_ADX_Query()
    {
        //arrange
        var guid = Guid.NewGuid();

        //act
        var whereClause = AdxLiveDataTestRepository.GetConnectorIdClause(guid);

        //assert
        whereClause.Should().StartWith($"| where ConnectorId  == '{guid}'");
    }
}

internal class AdxLiveDataTestRepository : AdxBaseRepository
{
    public AdxLiveDataTestRepository(IAdxQueryRunner adxQueryRunner, IContinuationTokenProvider<string, string> continuationTokenProvider)
        : base(adxQueryRunner, continuationTokenProvider)
    {
    }

    public new string GetdtIdsClause(List<string> dtdIds)
    {
        return base.GetdtIdsClause(dtdIds);
    }

    public static new string GetTrendIdsClause(List<Guid> trendIds)
    {
        return AdxBaseRepository.GetTrendIdsClause(trendIds);
    }

    public static new string GetConnectorIdClause(Guid connectorId)
    {
        return AdxBaseRepository.GetConnectorIdClause(connectorId);
    }
}
