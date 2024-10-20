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
    public class PortfolioTicketStatsRepositoryTests
    {
        [Fact]
        public async Task PortfolioTicketStatsRepository_Get()
        {
            var directoryCore = new Mock<IRestApi>();
            var workflowCore  = new Mock<IRestApi>();

            var customerId = Guid.NewGuid();
            var portfolioId  = Guid.NewGuid();

            directoryCore.Setup(x => x.Get<List<Site>>($"customers/{customerId}/portfolios/{portfolioId}/sites", It.IsAny<object>())).ReturnsAsync(new List<Site>
            {
                new Site { Id = Guid.NewGuid(), Name = "Fred" },
                new Site { Id = Guid.NewGuid(), Name = "Wilma" },
                new Site { Id = Guid.NewGuid(), Name = "Pebbles" },
            });

            workflowCore.Setup(x => x.Get<List<TicketStats>>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new List<TicketStats>
            {
                new TicketStats { OverdueCount = 1, HighCount = 2, MediumCount = 3, UrgentCount = 4,  LowCount = 4,  OpenCount = 17},
                new TicketStats { OverdueCount = 2, HighCount = 4, MediumCount = 6, UrgentCount = 8,  LowCount = 8,  OpenCount = 8},
                new TicketStats { OverdueCount = 3, HighCount = 6, MediumCount = 9, UrgentCount = 13, LowCount = 1,  OpenCount = 1}
            });

            var repo = new PortfolioTicketStatsRepository(directoryCore.Object, workflowCore.Object);
            var result = await repo.Get((customerId, portfolioId));

            Assert.NotNull(result);
            Assert.Equal(6, result.OverdueCount);
            Assert.Equal(12, result.HighCount);
            Assert.Equal(18, result.MediumCount);
            Assert.Equal(25, result.UrgentCount);
            Assert.Equal(13, result.LowCount);
            Assert.Equal(26, result.OpenCount);

        }
    }
}
