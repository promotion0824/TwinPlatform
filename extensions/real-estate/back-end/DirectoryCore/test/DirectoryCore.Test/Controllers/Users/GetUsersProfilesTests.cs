using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users;

public class GetUsersProfilesTests : BaseInMemoryTest
{
    public GetUsersProfilesTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task TokenNotProvided_GetUsersProfiles_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var response = await client.PostAsJsonAsync(
                "users/profiles",
                new GetUsersProfilesRequest()
            );
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task UsersExists_GetUsersProfiles_ReturnUsers()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var userIds = Fixture.CreateMany<Guid>(collectionCount).ToList();
        var emails = Fixture.CreateMany<string>(collectionCount).ToList();
        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture
                .Build<UserEntity>()
                .With(x => x.Id, userIds[i])
                .With(x => x.Email, emails[i])
                .Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { Ids = userIds, Emails = emails }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UsersExistsWithSearchByIdsOnly_GetUsersProfiles_ReturnUsers()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var userIds = Fixture.CreateMany<Guid>(collectionCount).ToList();

        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture.Build<UserEntity>().With(x => x.Id, userIds[i]).Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { Ids = userIds }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UsersExistsWithSearchByEmailsOnly_GetUsersProfiles_ReturnUsers()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var emails = Fixture.CreateMany<string>(collectionCount).ToList();
        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture.Build<UserEntity>().With(x => x.Email, emails[i]).Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { Emails = emails }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task SendEmptyRequest_GetUsersProfiles_ReturnEmptyList()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var userIds = Fixture.CreateMany<Guid>(collectionCount).ToList();
        var emails = Fixture.CreateMany<string>(collectionCount).ToList();
        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture
                .Build<UserEntity>()
                .With(x => x.Id, userIds[i])
                .With(x => x.Email, emails[i])
                .Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestsIncludeNullEmailsValue_GetUsersProfiles_ReturnUsers()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);
        var userIds = Fixture.CreateMany<Guid>(collectionCount).ToList();

        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture.Build<UserEntity>().With(x => x.Id, userIds[i]).Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { Ids = userIds, Emails = null }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task RequestsIncludeNullIdsValue_GetUsersProfiles_ReturnUsers()
    {
        var collectionCount = 3;
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
        using var client = server.CreateClient(null);

        var emails = Fixture.CreateMany<string>(collectionCount).ToList();
        var users = new List<UserEntity>();
        for (int i = 0; i < collectionCount; i++)
        {
            var user = Fixture.Build<UserEntity>().With(x => x.Email, emails[i]).Create();
            users.Add(user);
        }

        var expectedResponse = users
            .Select(
                u =>
                    new UserProfileDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Phone = u.Mobile,
                        Company = u.Company,
                    }
            )
            .ToList();
        var extraUsers = Fixture.CreateMany<UserEntity>(3).ToList();
        users.AddRange(extraUsers);
        var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
        dbContext.Users.AddRange(users);
        dbContext.SaveChanges();

        var response = await client.PostAsJsonAsync(
            "users/profiles",
            new GetUsersProfilesRequest { Emails = emails, Ids = null }
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<UserProfileDto>>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
