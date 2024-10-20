using FluentAssertions;
using PlatformPortalXL.Services.Copilot;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Utilities
{
    public class CopilotChatTests : BaseInMemoryTest
    {
        public CopilotChatTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task CopilotChat_ReturnResponse()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"chat", new CopilotChatRequest()
                {
                    UserInput = "Test",
                    Context = new CopilotContext()
                    {
                        SessionId = "sessionId"
                    }
                });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task CopilotGetDocInfo_ReturnResponse()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"docs", new CopilotDocInfoRequest()
                {
                    Filenames = ["Test"]
                });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
