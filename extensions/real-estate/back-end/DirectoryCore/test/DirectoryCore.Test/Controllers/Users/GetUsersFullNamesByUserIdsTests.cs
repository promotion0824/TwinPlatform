using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users;

public class GetUsersFullNamesByUserIdsTests : BaseInMemoryTest
{
    public GetUsersFullNamesByUserIdsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task TokenNotProvided_GetUsersFullNamesByUserIds_ReturnUnauthorized()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient();
        var userIds = new List<Guid> { Guid.NewGuid() };
        var response = await client.PostAsJsonAsync("/users/fullNames", userIds);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UsersExists_GetUsersFullNamesByUserIds_ReturnUsersFullNames()
    {
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var userIds = Fixture.CreateMany<Guid>(3).ToList();
        var users = new List<UserEntity>();
        foreach (var userId in userIds)
        {
            var user = Fixture.Build<UserEntity>().With(x => x.Id, userId).Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(u => new FullNameDto(u.Id, u.FirstName, u.LastName))
            .ToList();

        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync("/users/fullNames", userIds);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<FullNameDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
