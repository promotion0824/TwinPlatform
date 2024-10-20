using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Permission
{
    public class AuthorizeUserPermissionTests : BaseInMemoryTest
    {
        public AuthorizeUserPermissionTests(ITestOutputHelper output)
            : base(output) { }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public async Task MoreThanOneResourceIdsAreProvided_CheckPermission_ReturnBadRequest(
            bool givenCustomerId,
            bool givenPortfolioId,
            bool givenSiteId
        )
        {
            var url = $"users/{Guid.NewGuid()}/permissions/ManageBuilding/eligibility?";
            if (givenCustomerId)
            {
                url += $"customerId={Guid.NewGuid()}&";
            }
            if (givenPortfolioId)
            {
                url += $"portfolioId={Guid.NewGuid()}&";
            }
            if (givenSiteId)
            {
                url += $"siteId={Guid.NewGuid()}&";
            }
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("More than one");
            }
        }

        [Fact]
        public async Task NoResourceIdIsProvided_CheckPermission_ReturnBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync(
                    $"users/{Guid.NewGuid()}/permissions/ManageBuilding/eligibility"
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ErrorResponse>(resultJson);
                result.Message.Should().Contain("No resource id");
            }
        }
    }
}
