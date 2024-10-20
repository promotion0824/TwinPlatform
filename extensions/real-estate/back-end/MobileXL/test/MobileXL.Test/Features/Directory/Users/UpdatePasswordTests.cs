using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MobileXL.Features.Directory;
using MobileXL.Models;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Directory.Users
{
    public class UpdatePasswordTests : BaseInMemoryTest
    {
        public UpdatePasswordTests(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public async Task GivenValidInputForCustomerUser_UpdatePassword_ReturnsNoContent()
        {
            var customerUserEmail = "test@test123.com";
            var request = Fixture.Create<UpdatePasswordRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(customerUserEmail)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.CustomerUser });
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{WebUtility.UrlEncode(customerUserEmail)}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"users/{WebUtility.UrlEncode(customerUserEmail)}/password", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        [Fact]
        public async Task GivenValidInputForSupervisor_UpdatePassword_ReturnsNotFound()
        {
            var supervisorUserEmail = "test@test123.com";
            var request = Fixture.Create<UpdatePasswordRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Get, $"accounts/{WebUtility.UrlEncode(supervisorUserEmail)}")
                    .ReturnsJson(new Account { UserType = UserTypeNames.Supervisor });
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{WebUtility.UrlEncode(supervisorUserEmail)}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"users/{WebUtility.UrlEncode(supervisorUserEmail)}/password", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Failed to update password");
            }
        }

        public class DirectoryApiUpdatePasswordRequest
        {
            public string Password { get; set; }
            public string EmailToken { get; set; }
        }

    }
}
