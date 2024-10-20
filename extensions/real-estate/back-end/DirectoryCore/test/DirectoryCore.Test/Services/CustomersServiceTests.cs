using System;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using DirectoryCore.Entities;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using DirectoryCore.Test.MockServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Willow.Common;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Accounts
{
    public class CustomersServiceTests : BaseInMemoryTest
    {
        public CustomersServiceTests(ITestOutputHelper output)
            : base(output) { }

        /// <summary>
        /// With an empty database, Setup_SingleTenant should set up a customer, a user,
        /// and an assignment. But then if we call it again, it should not create any
        /// additional items.
        /// </summary>
        [Fact]
        public async Task EmptyDb_Setup_SingleTenant_Sets_Up_Once()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            {
                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();

                var customersService = new CustomersService(
                    new DateTimeService(),
                    dbContext,
                    new Mock<IImageHubService>().Object,
                    new FakeAuth0ManagementService(),
                    new Mock<IAuth0Service>().Object,
                    new Mock<ISitesService>().Object,
                    Options.Create(
                        new SingleTenantOptions
                        {
                            IsSingleTenant = true,
                            CustomerUserIdForGroupUser = Guid.NewGuid()
                        }
                    ),
                    new Mock<ILogger<CustomersService>>().Object,
                    null
                );

                var customerId = Guid.NewGuid();

                await customersService.SetupSingleTenantData(
                    new()
                    {
                        Id = customerId,
                        Name = "_customerOptions.DisplayName",
                        Country = "",
                        SigmaConnectionId = "",
                        AccountExternalId = ""
                    }
                );

                dbContext.Customers.Count().Should().Be(1);
                dbContext.Users.Count().Should().Be(1);
                dbContext.Assignments.Count().Should().Be(1);

                await customersService.SetupSingleTenantData(
                    new()
                    {
                        Id = customerId,
                        Name = "_customerOptions.DisplayName",
                        Country = "",
                        SigmaConnectionId = "",
                        AccountExternalId = ""
                    }
                );

                dbContext.Customers.Count().Should().Be(1);
                dbContext.Users.Count().Should().Be(1);
                dbContext.Assignments.Count().Should().Be(1);
            }
        }
    }
}
