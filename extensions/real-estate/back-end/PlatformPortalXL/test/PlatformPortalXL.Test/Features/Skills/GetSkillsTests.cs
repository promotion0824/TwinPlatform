using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using Willow.Batch;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Skills;

public class GetSkillsTests : BaseInMemoryTest
{
    public GetSkillsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory]
    [InlineData("statuses=Open&statuses=InProgress")]
    public async Task GetSkills_ReturnsResponse(string queryString)
    {
          
        var expectedResult= Fixture.Build<SkillDto>()
            .CreateMany(10).ToList();


         
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, Guid.NewGuid()))
        {
              

            server.Arrange().GetInsightApi()
                .SetupRequest(HttpMethod.Post, "skills")
                .ReturnsJson(new BatchDto<SkillDto>()
                {
                    Items = expectedResult.ToArray()
                });


            var response = await client.PostAsJsonAsync($"skills", new BatchRequestDto());
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<BatchDto<SkillDto>>();
 
            result.Items.Should().BeEquivalentTo(expectedResult);
        }
    }


    [Fact]
    public async Task UnauthorizedUser_GetSkills_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.PostAsJsonAsync($"skills", new BatchRequestDto());
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
}
