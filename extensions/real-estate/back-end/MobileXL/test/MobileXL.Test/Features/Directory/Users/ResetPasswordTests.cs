using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Directory.Users
{
    public class ResetPasswordTests : BaseInMemoryTest
    {
        public ResetPasswordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CustomerUserEmailExists_ResetPassword_ReturnsNoContent()
        {
            var customerUserEmail = "test@test123.com";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(customerUserEmail)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.CustomerUser });
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{WebUtility.UrlEncode(customerUserEmail)}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"users/{WebUtility.UrlEncode(customerUserEmail)}/password/reset", new object());

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task CustomerUserEmailDoesNotExists_ResetPassword_ReturnsNotFound()
        {
            var customerUserEmail = "nonexisting@test123.com";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(customerUserEmail)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.CustomerUser });
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{WebUtility.UrlEncode(customerUserEmail)}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NotFound);

                var response = await client.PostAsJsonAsync($"users/{WebUtility.UrlEncode(customerUserEmail)}/password/reset", new object());

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task SupervisorUserEmailExists_ResetPassword_ReturnsNotFound()
        {
            var supervisorUserEmail = "test@test123.com";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(supervisorUserEmail)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.Supervisor });
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{WebUtility.UrlEncode(supervisorUserEmail)}/password/reset")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"users/{WebUtility.UrlEncode(supervisorUserEmail)}/password/reset", new object());

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Failed to reset password");
            }
        }
    }
}
