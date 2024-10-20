namespace Willow.LiveData.Core.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.LiveData.Core.Domain;
    using Willow.Tests.Infrastructure.Extensions;

    public class GetBatchLastTrendlogTests
    {
        private readonly Guid pointEntityId1 = Guid.Parse("0979c55a-d2fe-482a-9eff-23a96789c5c5");
        private readonly Guid pointEntityId3 = Guid.Parse("ebfbe65f-57b4-4ef8-8d32-9d5b8fa819c6");

        private readonly Guid siteId = Guid.Parse("76c0cdf8-430d-46ed-9c07-9dd127aa479f");

        private readonly Guid clientId = IntegrationFixture.ClientId;

        private string GetUrlBatchLastTrendlog(Guid siteId, params Guid[] pointIds)
        {
            var clientId = this.clientId;

            var queryString = new List<string>();
            queryString.Add($"clientId={clientId}");

            if (pointIds != null)
            {
                foreach (var pointId in pointIds)
                {
                    queryString.Add($"pointId={pointId}");
                }
            }

            var url = $"api/livedata/sites/{siteId}/lastTrendlogs";

            if (queryString.Any())
            {
                url = string.Concat(url, "?", string.Join("&", queryString));
            }

            return url;
        }

        [Test]
        [Ignore("")]
        public void GetBatchLastTrendlogs_Returns_Data()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.PostJsonAsync<List<PointTimeSeriesRawData>>(GetUrlBatchLastTrendlog(siteId), null).GetAwaiter().GetResult();
                result.Should().HaveCount(2);
                result.Select(q => q.PointEntityId).Should().BeEquivalentTo(new[] { pointEntityId1, pointEntityId3 });
            }
        }

        [Test]
        [Ignore("")]
        public void GetBatchLastTrendlogs_Returns_SingleValuePerPoint()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.PostJsonAsync<List<PointTimeSeriesRawData>>(GetUrlBatchLastTrendlog(siteId), null).GetAwaiter().GetResult();
                result.Should().HaveCount(2);
                result.GroupBy(q => q.PointEntityId).Select(q => q.Count()).Should().AllBeEquivalentTo(1);
            }
        }

        [Test]
        [Ignore("")]
        public void GetBatchLastTrendlogs_PointIdFilter_Returns_ExactPointData()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.PostJsonAsync<List<PointTimeSeriesRawData>>(GetUrlBatchLastTrendlog(siteId, pointEntityId1), null).GetAwaiter().GetResult();
                result.Should().HaveCount(1);
                result.Select(q => q.PointEntityId).Should().BeEquivalentTo(new[] { pointEntityId1 });
            }
        }
    }
}
