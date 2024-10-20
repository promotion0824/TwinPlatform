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
    public class PortfolioInsightsStatsRepositoryTests
    {
        [Fact]
        public async Task PortfolioInsightsStatsRepository_Get()
        {
            var directoryCore = new Mock<IRestApi>();
            var insightsCore  = new Mock<IRestApi>();

            var customerId = Guid.NewGuid();
            var portfolioId  = Guid.NewGuid();

            directoryCore.Setup(x => x.Get<List<Site>>($"customers/{customerId}/portfolios/{portfolioId}/sites", It.IsAny<object>())).ReturnsAsync(new List<Site>
            {
                new Site { Id = Guid.NewGuid(), Name = "Fred" },
                new Site { Id = Guid.NewGuid(), Name = "Wilma" },
                new Site { Id = Guid.NewGuid(), Name = "Pebbles" },
            });

            insightsCore.Setup(x => x.Get<List<InsightsStats>>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new List<InsightsStats>
            {
                new InsightsStats { OpenCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4,  LowCount = 5},
                new InsightsStats { OpenCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8,  LowCount = 10},
                new InsightsStats { OpenCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 12, LowCount = 4}
            });

            var repo = new PortfolioInsightsStatsRepository(directoryCore.Object, insightsCore.Object);
            var result = await repo.Get((customerId, portfolioId));

            Assert.NotNull(result);
            Assert.Equal(6, result.OpenCount);
            Assert.Equal(12, result.HighCount);
            Assert.Equal(18, result.MediumCount);
            Assert.Equal(24, result.UrgentCount);
            Assert.Equal(19, result.LowCount);

        }
    }
}
