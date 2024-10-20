using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Moq;

using Willow.Api.Client;
using Willow.Platform.Models;
using Willow.Platform.Statistics;

namespace Willow.Platform.Statistics.UnitTests
{
    public class SiteStatisticsRequestTests
    {
        [Fact]
        public async Task SiteStatisticsRequest_ToString()
        {
            var siteId = Guid.NewGuid();

            Assert.Equal($"{siteId}_", (new SiteStatisticsRequest { SiteId = siteId }).ToString());
            Assert.Equal($"{siteId}_", (new SiteStatisticsRequest { SiteId = siteId, FloorId = "" }).ToString());
            Assert.Equal($"{siteId}_Bob", (new SiteStatisticsRequest { SiteId = siteId, FloorId = "Bob" }).ToString());

        }
    }
}