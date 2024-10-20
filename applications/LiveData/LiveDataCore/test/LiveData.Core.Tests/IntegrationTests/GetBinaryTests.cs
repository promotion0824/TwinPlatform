namespace Willow.LiveData.Core.Tests.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Domain;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
    using Willow.Tests.Infrastructure.Extensions;

    public class GetBinaryTests
    {
        private readonly string dateFormat = "yyyy-MM-dd HH:mm";

        private readonly Guid pointEntityId = Guid.Parse("0979c55a-d2fe-482a-9eff-23a96789c5c5");
        private readonly Guid clientId = IntegrationFixture.ClientId;
        private readonly DateTime startDate = new DateTime(2019, 09, 19);
        private readonly DateTime endDate = new DateTime(2019, 09, 20);

        private string GetUrl(Guid pointEntityId, DateTime? startDate, DateTime? endDate, string continuationToken = null, int? pageSize = null, string dateFormat = null)
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

            var url = $"api/livedata/point/binary/{pointEntityId}";

            if (queryString.Any())
            {
                url = string.Concat(url, "?", string.Join("&", queryString));
            }

            return url;
        }

        [Test]
        [Ignore("")]
        public void GetBinary_Returns_Data()
        {
            using (var client = IntegrationFixture.Server.CreateClient())
            {
                var result = client.GetJsonAsync<IEnumerable<TimeSeriesBinaryData>>(GetUrl(pointEntityId, startDate, endDate)).GetAwaiter().GetResult();

                //how many 5 minutes in 24hrs + 5 minutes from 00 of next day
                result.Should().HaveCount((24 * 60 / 5) + 1);
            }
        }
    }
}
