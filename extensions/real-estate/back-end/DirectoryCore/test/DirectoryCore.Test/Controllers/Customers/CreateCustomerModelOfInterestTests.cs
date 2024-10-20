using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class CreateCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public CreateCustomerModelOfInterestTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_CreateCustomerModelOfInterest_ReturnsNewCustomerModelOfInterest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var guid = Guid.NewGuid();
                var request = Fixture.Create<CreateCustomerModelOfInterestRequest>();
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(x => x.ModelsOfInterestJson, (string)null)
                    .With(x => x.ModelsOfInterestETag, guid)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
                result.ModelId.Should().Be(request.ModelId);
                result.Name.Should().Be(request.Name);
                result.Color.Should().Be(request.Color);
                result.Text.Should().Be(request.Text);
                result.Icon.Should().Be(result.Icon);
                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var existingCustomer = dbContextInServer.Customers.FirstOrDefault(
                    a => a.Id == customer.Id
                );
                existingCustomer.ModelsOfInterestJson.Should().NotBeNull();
                CustomerEntity
                    .MapCustomerModelsOfInterest(existingCustomer.ModelsOfInterestJson)
                    .Should()
                    .HaveCount(1);
            }
        }

        [Fact]
        public async Task CustomerDoesNotExists_CreateCustomerModelOfInterest_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(x => x.ModelsOfInterestJson, (string)null)
                    .Create();
                var request = Fixture.Create<CreateCustomerModelOfInterestRequest>();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/modelsOfInterest",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerModelsOfInterestReachTheMaximumLimit_CreateCustomerModelOfInterest_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var modelsOfInterest = Fixture.CreateMany<CustomerModelOfInterest>(16).ToList();
                var guid = Guid.NewGuid();
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        x => x.ModelsOfInterestJson,
                        JsonSerializerExtensions.Serialize(modelsOfInterest)
                    )
                    .With(x => x.ModelsOfInterestETag, guid)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest",
                    new CreateCustomerModelOfInterestRequest()
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CustomerModelsOfInterestAlreadyExists_CreateCustomerModelOfInterest_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customerModelOfInterest = Fixture.Create<CustomerModelOfInterest>();
                var modelsOfInterest = new List<CustomerModelOfInterest>()
                {
                    customerModelOfInterest
                };
                var guid = Guid.NewGuid();
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        x => x.ModelsOfInterestJson,
                        JsonSerializerExtensions.Serialize(modelsOfInterest)
                    )
                    .With(x => x.ModelsOfInterestETag, guid)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PostAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest",
                    customerModelOfInterest
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
