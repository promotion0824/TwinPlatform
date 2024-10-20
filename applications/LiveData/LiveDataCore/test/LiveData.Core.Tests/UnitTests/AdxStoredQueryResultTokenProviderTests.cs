#pragma warning disable SA1118 // Parameter should not span multiple lines
namespace Willow.LiveData.Core.Tests.UnitTests;

using System;
using FluentAssertions;
using NUnit.Framework;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;

public class AdxStoredQueryResultTokenProviderTests
{
    private readonly IContinuationTokenProvider<string, string> adxStoredQueryResultTokenProvider;

    public AdxStoredQueryResultTokenProviderTests()
    {
        adxStoredQueryResultTokenProvider = new AdxStoredQueryResultTokenProvider();
    }

    [TestCase(@"let startTime = datetime(2022-05-01T10:01:43); 
            let endTime = datetime(2022-05-01T10:01:44); 
            Telemetry
            | where SourceTimestamp  between (startTime..endTime)
            | order by SourceTimestamp asc, EnqueuedTimestamp asc
            | where ConnectorId  == '4a94625b-f8b6-4c35-bf3f-d62cd68c7224'
            | where TrendId in ('668ec448-4c6a-48ce-bf72-59e5bd1177e5', '0009c297-9bac-422d-b3e8-e215503a9e0f', 'd5ade195-2ecf-46a9-9935-327fc4283493')")]
    public void AdxStoredQueryResultTokenProvider_GetToken_Returns_Same_Token_When_Same_Text(string query)
    {
        //arrange

        //act
        var uniqueToken = adxStoredQueryResultTokenProvider.GetToken(query);

        //assert
        for (int i = 0; i < 100; i++)
        {
            uniqueToken.Should().Be(adxStoredQueryResultTokenProvider.GetToken(query));
        }
    }

    [TestCase(@"let startTime = datetime(2022-05-01T10:01:43); 
            let endTime = datetime(2022-05-01T10:01:44); 
            Telemetry
            | where SourceTimestamp  between (startTime..endTime)
            | order by SourceTimestamp asc, EnqueuedTimestamp asc
            | where ConnectorId  == '4a94625b-f8b6-4c35-bf3f-d62cd68c7224'
            | where TrendId in ('668ec448-4c6a-48ce-bf72-59e5bd1177e5', '0009c297-9bac-422d-b3e8-e215503a9e0f', 'd5ade195-2ecf-46a9-9935-327fc4283493')",
        @"LET STARTTIME = DATETIME(2022-05-01T10:01:43); 
            LET ENDTIME = DATETIME(2022-05-01T10:01:44); 
            TELEMETRY
            | WHERE SOURCETIMESTAMP  BETWEEN (STARTTIME..ENDTIME)
            | ORDER BY SOURCETIMESTAMP ASC, ENQUEUEDTIMESTAMP ASC
            | WHERE CONNECTORID  == '4A94625B-F8B6-4C35-BF3F-D62CD68C7224'
            | WHERE TRENDID IN ('668EC448-4C6A-48CE-BF72-59E5BD1177E5', '0009C297-9BAC-422D-B3E8-E215503A9E0F', 'D5ADE195-2ECF-46A9-9935-327FC4283493')")]
    public void AdxStoredQueryResultTokenProvider_GetToken_Returns_Different_Token_When_Same_Text_But_Different_Case(string lowerCaseQuery, string upperCaseQuery)
    {
        //arrange

        //act
        var lowerCaseQueryToken = adxStoredQueryResultTokenProvider.GetToken(lowerCaseQuery);
        var upperCaseQueryToken = adxStoredQueryResultTokenProvider.GetToken(upperCaseQuery);

        //assert
        lowerCaseQueryToken.Should().NotBe(upperCaseQueryToken);
    }

    [TestCase(@"let startTime = datetime(2022-05-01T10:01:43); 
            let endTime = datetime(2022-05-01T10:01:44); 
            Telemetry
            | where SourceTimestamp  between (startTime..endTime)
            | order by SourceTimestamp asc, EnqueuedTimestamp asc
            | where ConnectorId  == '4a94625b-f8b6-4c35-bf3f-d62cd68c7224'
            | where TrendId in ('668ec448-4c6a-48ce-bf72-59e5bd1177e5', '0009c297-9bac-422d-b3e8-e215503a9e0f', 'd5ade195-2ecf-46a9-9935-327fc4283493')")]
    public void AdxStoredQueryResultTokenProvider_GetToken_Returns_Different_Token_When_Different_Text(string query)
    {
        //arrange

        //act
        var uniqueToken = adxStoredQueryResultTokenProvider.GetToken(query);

        //assert
        for (int i = 0; i < 1000; i++)
        {
            var changedQuery = query.Replace("4a94625b-f8b6-4c35-bf3f-d62cd68c7224", Guid.NewGuid().ToString());
            uniqueToken.Should().NotBe(adxStoredQueryResultTokenProvider.GetToken(changedQuery));
        }
    }

    [TestCase("XX23422")]
    public void AdxStoredQueryResultTokenProvider_ParseToken_Returns_Exception(string token)
    {
        //arrange

        //act
        Action action = () => adxStoredQueryResultTokenProvider.ParseToken(token);

        //assert
        action.Should().Throw<NotSupportedException>();
    }
}
#pragma warning restore SA1118 // Parameter should not span multiple lines
