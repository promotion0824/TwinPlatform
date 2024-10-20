using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Users
{
    public class GetRolesTests : BaseInMemoryTest
    {
        public GetRolesTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task RolesExist_GetRoles_ReturnAllRoles()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var roles = Fixture.CreateMany<RoleEntity>(5);

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Roles.RemoveRange(dbContext.Roles.ToList());
                dbContext.Roles.AddRange(roles);
                await dbContext.SaveChangesAsync();

                var response = await client.GetAsync($"roles");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<IList<RoleDto>>();
                result.Should().HaveCount(5);
                result.Should().BeEquivalentTo(roles);
            }
        }
    }
}
