namespace Willow.LiveData.Core.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.LiveData.Core.Domain;
    using Willow.Tests.Infrastructure.Extensions;

    public class GetBatchTrendlogTests
    {
        private readonly string dateFormat = "yyyy-MM-dd HH:mm";

        private readonly Guid pointEntityId1 = Guid.Parse("0979c55a-d2fe-482a-9eff-23a96789c5c5");
        private readonly Guid pointEntityId3 = Guid.Parse("ebfbe65f-57b4-4ef8-8d32-9d5b8fa819c6");
        private readonly Guid siteId = Guid.Parse("76c0cdf8-430d-46ed-9c07-9dd127aa479f");
        private readonly Guid clientId = IntegrationFixture.ClientId;
        private readonly DateTime startDate = new DateTime(2019, 09, 19);
        private readonly DateTime endDate = new DateTime(2019, 09, 20);

        private string GetUrl(Guid siteId, DateTime? startDate, DateTime? endDate, int? pageSize = null, string dateFormat = null, params Guid[] pointIds)
        {
            var format = dateFormat ?? this.dateFormat;
            var clientId = this.clientId;

            var queryString = new List<string>();
            queryString.Add($"clientId={clientId}");

            if (startDate != null)
            {
                queryString.Add($"start={startDate.Value.ToString(format)}");
            }

            if (endDate != null)
            {
                queryString.Add($"end={endDate.Value.ToString(format)}");
            }

            if (pageSize != null)
            {
                queryString.Add($"pageSize={pageSize.ToString()}");
            }

            if (pointIds != null)
            {
                foreach (var pointId in pointIds)
                {
                    queryString.Add($"pointId={pointId}");
                }
            }

            var url = $"api/livedata/sites/{siteId}/trendlogs";

            if (queryString.Any())
            {
                url = string.Concat(url, "?", string.Join("&", queryString));
            }

            return url;
        }

        [Test]
        [Ignore("")]
        public void GetBatch_Returns_Data()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.PostJsonAsync<List<GetTrendlogsResultItem>>(GetUrl(siteId, startDate, endDate), null).GetAwaiter().GetResult();
                result.Should().HaveCount(2);
                result.Select(q => q.PointEntityId).Should().BeEquivalentTo(new[] { pointEntityId1, pointEntityId3 });
            }
        }

        [Test]
        [Ignore("")]
        public void GetBatch_PointIdFilter_Returns_ExactPointData()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.PostJsonAsync<List<GetTrendlogsResultItem>>(GetUrl(siteId, startDate, endDate, null, null, pointEntityId1), null).GetAwaiter().GetResult();
                result.Should().HaveCount(1);
                result.Select(q => q.PointEntityId).Should().BeEquivalentTo(new[] { pointEntityId1 });
            }
        }
    }
}
