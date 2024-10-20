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
    public class InitializeUserTests : BaseInMemoryTest
    {
        public InitializeUserTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_InitializeUserTests_ReturnsNoContent()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<OldInitializeUserRequest>()                                 
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiInitializeUserRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userEmail}/initialize", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiInitializeUserRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"users/{userEmail}/initialize", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        [Fact]
        public async Task GivenInvalidInput_InitializeUserTests_ReturnsNoContent_deprecated()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<OldInitializeUserRequest>()                                 
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiInitializeUserRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userEmail}/initialize", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiInitializeUserRequest>();
                        return true;
                    })
                    .Throws( new NotFoundException());

                var response = await client.PostAsJsonAsync($"users/{userEmail}/initialize", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        [Fact]
        public async Task GivenInvalidInput_InitializeUserTests_ReturnsNoContent()
        {
            var userEmail = "test@test123.com";
            var request = Fixture.Build<InitializeUserRequest>()                                 
                                 .With( x=> x.Email, (string)null)
                                 .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiInitializeUserRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Post, $"users/{userEmail}/initialize", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiInitializeUserRequest>();
                        return true;
                    })
                    .Throws( new NotFoundException());

                var response = await client.PostAsJsonAsync($"users/{userEmail}/initialize", request);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                requestToDirectoryApi.Password.Should().Be(request.Password);
                requestToDirectoryApi.EmailToken.Should().Be(request.Token);
            }
        }

        public class DirectoryApiInitializeUserRequest
        {
            public string Password { get; set; }
            public string EmailToken { get; set; }
        }

    }
}
