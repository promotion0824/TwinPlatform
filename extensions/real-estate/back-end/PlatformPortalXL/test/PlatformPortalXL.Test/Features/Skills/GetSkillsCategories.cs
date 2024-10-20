using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Skills;

public class GetSkillsCategories : BaseInMemoryTest
{
    public GetSkillsCategories(ITestOutputHelper output) : base(output)
    {
    }
    [Fact]
    public async Task ReturnsInsightEnum()
    {

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, new Guid()))
        {
          
            var response = await client.GetAsync("skills/categories");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<EnumKeyValueDto>>();

            var expected = typeof(SkillCategory).ToEnumKeyValueDto();

            result.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task UnauthorizedUser_GetSkills_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("skills/categories");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }
}
