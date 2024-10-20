using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Controllers.Requests;
using DirectoryCore.Domain;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Sites
{
    public class UpsertConnectorAccountTests : BaseInMemoryTest
    {
        public UpsertConnectorAccountTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task WhenConnectorAccountNotExist_AccountIsCreated()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
            var connectorId = Guid.NewGuid();
            var email = $"{connectorId:N}@connector.willowinc.com";
            var request = Fixture.Create<CreateConnectorAccountRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var handler = arrangement.GetAuth0Api();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/sites/{site.Id}/connectors/{connectorId}/account",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var createdUsers = server.Assert().GetAuth0ManagementService().CreatedUsers;
                createdUsers.Should().HaveCount(1);
                createdUsers[0].Id.Should().Be(connectorId);
                createdUsers[0].FirstName.Should().Be(site.Name);
                createdUsers[0].LastName.Should().Be(customer.Name);
                createdUsers[0].Email.Should().Be(email);
                createdUsers[0].InitialPassword.Should().Be(request.Password);
                createdUsers[0].UserType.Should().Be(UserTypeNames.Connector);
            }
        }

        [Fact]
        public async Task WhenConnectorAccountAndCustomerDoesNotExist_AccountIsCreated()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
            var connectorId = Guid.NewGuid();
            var email = $"{connectorId:N}@connector.willowinc.com";
            var request = Fixture.Create<CreateConnectorAccountRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var handler = arrangement.GetAuth0Api();

                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(site);

                var response = await client.PutAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/sites/{site.Id}/connectors/{connectorId}/account",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task WhenConnectorAccountAndSiteDoesNotExist_AccountIsCreated()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
            var connectorId = Guid.NewGuid();
            var email = $"{connectorId:N}@connector.willowinc.com";
            var request = Fixture.Create<CreateConnectorAccountRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var handler = arrangement.GetAuth0Api();

                var siteId = new Guid();
                Site dummy = null;
                var url = $"sites/{siteId}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(dummy);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/sites/{siteId}/connectors/{connectorId}/account",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task SiteDoesNotExist_AccountIsCreated()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
            var connectorId = Guid.NewGuid();
            var email = $"{connectorId:N}@connector.willowinc.com";
            var request = Fixture.Create<CreateConnectorAccountRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var handler = arrangement.GetAuth0Api();

                Site dummy = null;
                var url = $"sites/{site.Id}/extend";
                server
                    .Arrange()
                    .GetSiteCoreApi()
                    .SetupRequest(HttpMethod.Get, url)
                    .ReturnsJson(dummy);

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/sites/{site.Id}/connectors/{connectorId}/account",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task WhenConnectorAccountExist_PasswordIsUpdated()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var site = Fixture.Build<Site>().With(s => s.CustomerId, customer.Id).Create();
            var connectorId = Guid.NewGuid();
            var email = $"{connectorId:N}@connector.willowinc.com";
            var request = Fixture.Create<CreateConnectorAccountRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var dbContext = arrangement.CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();
                await server
                    .Assert()
                    .GetAuth0ManagementService()
                    .CreateUser(
                        connectorId,
                        email,
                        site.Name,
                        customer.Name,
                        "password",
                        UserTypeNames.Connector
                    );

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/sites/{site.Id}/connectors/{connectorId}/account",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                var changedPasswords = server.Assert().GetAuth0ManagementService().ChangedPasswords;
                changedPasswords.Should().HaveCount(1);
                changedPasswords[0].Should().Be(request.Password);
            }
        }
    }
}
