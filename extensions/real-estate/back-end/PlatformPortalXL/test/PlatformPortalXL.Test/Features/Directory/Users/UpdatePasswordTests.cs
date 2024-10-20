using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using PlatformPortalXL.Features.Directory;
using Willow.ExceptionHandling.Exceptions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Directory.Users
{
    public class UpdatePasswordTests : BaseInMemoryTest
    {
        public UpdatePasswordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_UpdatePassword_ReturnsNoContent_deprecated()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<OldUpdatePasswordRequest>()
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{userEmail}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"users/{userEmail}/password", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdatePassword_ReturnsNoContent()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<UpdatePasswordRequest>()
                                  .With(x => x.Email, userEmail)
                                  .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{userEmail}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"users/{userEmail}/password", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }


        [Fact]
        public async Task GivenInvalidInput_UpdatePassword_ReturnsNoContent()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<UpdatePasswordRequest>()
                                  .With(x => x.Email, userEmail)
                                  .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"users/{userEmail}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .Throws( new NotFoundException());

                var response = await client.PutAsJsonAsync($"users/{userEmail}/password", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        public class DirectoryApiUpdatePasswordRequest
        {
            public string Password { get; set; }
            public string EmailToken { get; set; }
        }

    }
}
