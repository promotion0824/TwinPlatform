namespace Willow.LiveData.Core.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
    using Willow.Tests.Infrastructure.Extensions;

    public class GetTrendlogTests
    {
        private readonly string dateFormat = "yyyy-MM-dd HH:mm";

        private readonly Guid pointEntityId = Guid.Parse("0979c55a-d2fe-482a-9eff-23a96789c5c5");
        private readonly Guid clientId = IntegrationFixture.ClientId;
        private readonly DateTime startDate = new DateTime(2019, 09, 19);
        private readonly DateTime endDate = new DateTime(2019, 09, 20);

        private string GetUrlTrendlog(Guid pointEntityId, DateTime? startDate, DateTime? endDate, string continuationToken = null, int? pageSize = null, string dateFormat = null)
        {
            var format = dateFormat ?? this.dateFormat;
            var clientId = this.clientId;

            var queryString = new List<string> { $"clientId={clientId}" };

            if (startDate != null)
            {
                queryString.Add($"start={startDate.Value.ToString(format)}");
            }

            if (endDate != null)
            {
                queryString.Add($"end={endDate.Value.ToString(format)}");
            }

            if (!string.IsNullOrEmpty(continuationToken))
            {
                queryString.Add($"continuationToken={continuationToken}");
            }

            if (pageSize != null)
            {
                queryString.Add($"pageSize={pageSize.ToString()}");
            }

            var url = $"api/livedata/points/{pointEntityId}/trendlog";

            if (queryString.Any())
            {
                url = string.Concat(url, "?", string.Join("&", queryString));
            }

            return url;
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_Returns_Data()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                result.Data.Should().NotBeEmpty();
            }
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_ContinuationToken_Empty_Returns_FirstPage()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var resultAll = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                resultAll.Data.Should().NotBeEmpty();

                var pageSize = 5;

                var result = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                result.Data.Should().NotBeEmpty();
                result.Data.Count.Should().Be(pageSize);
                result.Data.Should().BeEquivalentTo(resultAll.Data.Take(pageSize), c => c.WithStrictOrdering());
                result.ContinuationToken.Should().NotBeNullOrEmpty();
            }
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_ContinuationToken_Returns_NextPage()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var resultAll = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                resultAll.Data.Should().NotBeEmpty();

                var pageSize = 5;

                var result1 = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                result1.Data.Should().NotBeEmpty();
                result1.Data.Count.Should().Be(pageSize);
                result1.Data.Should().BeEquivalentTo(resultAll.Data.Take(pageSize), c => c.WithStrictOrdering());
                result1.ContinuationToken.Should().NotBeNullOrEmpty();

                var result2 = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate, result1.ContinuationToken)).GetAwaiter().GetResult();
                result2.Data.Should().NotBeEmpty();
                result2.Data.Count.Should().Be(pageSize);
                result2.Data.Should().BeEquivalentTo(resultAll.Data.Skip(pageSize).Take(pageSize), c => c.WithStrictOrdering());
                result2.ContinuationToken.Should().NotBeNullOrEmpty();
            }
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_ContinuationToken_Returns_LastPageWithoutNextToken()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var resultAll = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                resultAll.Data.Should().NotBeEmpty();

                var pageSize = (resultAll.Data.Count / 2) + 2;

                var result1 = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();
                result1.Data.Should().NotBeEmpty();
                result1.Data.Count.Should().Be(pageSize);
                result1.Data.Should().BeEquivalentTo(resultAll.Data.Take(pageSize), c => c.WithStrictOrdering());
                result1.ContinuationToken.Should().NotBeNullOrEmpty();

                var result2 = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate, result1.ContinuationToken)).GetAwaiter().GetResult();
                result2.Data.Should().NotBeEmpty();
                result2.Data.Count.Should().Be(resultAll.Data.Count - pageSize);
                result2.Data.Should().BeEquivalentTo(resultAll.Data.Skip(pageSize).Take(pageSize), c => c.WithStrictOrdering());
                result2.ContinuationToken.Should().BeNull();
            }
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_WithPageSize_Returns_DataOfSpecifiedSize()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.GetJsonAsync<GetTrendlogResult>(GetUrlTrendlog(pointEntityId, startDate, endDate, null, 6)).GetAwaiter().GetResult();
                result.Data.Should().NotBeEmpty();
                result.Data.Should().HaveCount(6);
            }
        }

        [Test]
        [Ignore("")]
        public void GetTrendlog_WrongDateFormat_Returns_BadRequest()
        {
        }
    }
}
