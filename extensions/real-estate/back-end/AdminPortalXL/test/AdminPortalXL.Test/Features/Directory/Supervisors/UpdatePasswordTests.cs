using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AdminPortalXL.Features.Directory;
using AutoFixture;
using FluentAssertions;
using Moq.Contrib.HttpClient;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;

namespace AdminPortalXL.Test.Features.Directory.Users
{
    public class UpdatePasswordTests : BaseInMemoryTest
    {
        public UpdatePasswordTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_UpdatePassword_ReturnsNoContent()
        {
            var supervisorEmail = "test@test123.com";
            var request = Fixture.Create<UpdatePasswordRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                DirectoryApiUpdatePasswordRequest requestToDirectoryApi = null;
                server.Arrange().GetDirectoryApi()
                    .SetupRequest(HttpMethod.Put, $"supervisors/{supervisorEmail}/password", async requestMessage =>
                    {
                        requestToDirectoryApi = await requestMessage.Content.ReadAsAsync<DirectoryApiUpdatePasswordRequest>();
                        return true;
                    })
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PutAsJsonAsync($"supervisors/{supervisorEmail}/password", request);

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