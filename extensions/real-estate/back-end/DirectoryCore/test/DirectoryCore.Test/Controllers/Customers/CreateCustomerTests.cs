using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using DirectoryCore.Enums;
using FluentAssertions;
using Willow.Directory.Models;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class CreateCustomerTests : BaseInMemoryTest
    {
        public CreateCustomerTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_CreateCustomer_ReturnNewCustomer()
        {
            var customerName = Fixture.Create<string>();
            var customerCountry = Fixture.Create<string>();
            var sigmaConnectionId = Fixture.Create<string>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers",
                    new CreateCustomerRequest
                    {
                        Name = customerName,
                        Country = customerCountry,
                        SigmaConnectionId = sigmaConnectionId
                    }
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.Name.Should().Be(customerName);
                result.Country.Should().Be(customerCountry);
                var dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Customers.Should().HaveCount(1);
                var customerEntity = dbContext.Customers.First();
                customerEntity.Name.Should().Be(customerName);
                customerEntity.Country.Should().Be(customerCountry);
                customerEntity.SigmaConnectionId.Should().Be(sigmaConnectionId);
            }
        }

        [Fact]
        public async Task GivenValidInput_CreateCustomer_SupportUserIsCreatedAndCustomerAdminRoleAssigned()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync(
                    $"customers",
                    new CreateCustomerRequest
                    {
                        Name = Fixture.Create<string>(),
                        Country = Fixture.Create<string>()
                    }
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                var dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Users.Should().HaveCount(1);
                var userEntity = dbContext.Users.First();
                userEntity.FirstName.Should().Be(WellKnownUsers.CustomerSupport.FirstName);
                userEntity.LastName.Should().Be(WellKnownUsers.CustomerSupport.LastName);
                userEntity.Initials.Should().Be(WellKnownUsers.CustomerSupport.Initials);
                userEntity.Email.Should().Be(WellKnownUsers.CustomerSupport.Email);
                userEntity.Auth0UserId.Should().NotBeEmpty();
                var assignmentEntity = dbContext.Assignments.First();
                assignmentEntity.PrincipalId.Should().Be(userEntity.Id);
                assignmentEntity.PrincipalType.Should().Be(PrincipalType.User);
                assignmentEntity.ResourceId.Should().Be(userEntity.CustomerId);
                assignmentEntity.ResourceType.Should().Be(RoleResourceType.Customer);
                assignmentEntity
                    .RoleId.Should()
                    .Be(Guid.Parse("48174c3b-57ed-4d0d-badc-6c9d8d0afab1"));
            }
        }

        [Fact]
        public async Task SigmaConnectionIdInUse_CreateCustomer_ThrowBadRequest()
        {
            var customerName = Fixture.Create<string>();
            var customerCountry = Fixture.Create<string>();
            var sigmaConnectionId = Fixture.Create<string>();

            var otherCustomerEntity = Fixture
                .Build<CustomerEntity>()
                .With(x => x.SigmaConnectionId, sigmaConnectionId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Customers.Add(otherCustomerEntity);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers",
                    new CreateCustomerRequest
                    {
                        Name = customerName,
                        Country = customerCountry,
                        SigmaConnectionId = sigmaConnectionId
                    }
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
