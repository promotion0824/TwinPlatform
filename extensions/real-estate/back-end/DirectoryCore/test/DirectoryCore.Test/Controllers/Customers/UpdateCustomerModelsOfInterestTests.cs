using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class UpdateCustomerModelsOfInterestTests : BaseInMemoryTest
    {
        public UpdateCustomerModelsOfInterestTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_UpdateCustomerModelsOfInterest_ReturnsNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var existingModelsOfInterest = Fixture
                    .Build<CustomerModelOfInterest>()
                    .CreateMany(3);
                var requestModeOfInterest = Fixture
                    .Build<UpdateCustomerModelOfInterestRequest>()
                    .CreateMany(2);
                var request = new UpdateCustomerModelsOfInterestRequest
                {
                    ModelsOfInterest = requestModeOfInterest.ToList()
                };
                var guid = Guid.NewGuid();
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        x => x.ModelsOfInterestJson,
                        JsonSerializerExtensions.Serialize(existingModelsOfInterest)
                    )
                    .With(x => x.ModelsOfInterestETag, guid)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var dbContextInServer = server.Arrange().CreateDbContext<DirectoryDbContext>();
                var existingCustomer = dbContextInServer.Customers.FirstOrDefault(
                    a => a.Id == customer.Id
                );
                existingCustomer.ModelsOfInterestJson.Should().NotBeNull();
                existingCustomer
                    .ModelsOfInterestJson.Should()
                    .Be(
                        JsonSerializerExtensions.Serialize(
                            CustomerModelOfInterest.MapFrom(requestModeOfInterest)
                        )
                    );
                CustomerEntity
                    .MapCustomerModelsOfInterest(existingCustomer.ModelsOfInterestJson)
                    .Should()
                    .HaveCount(2);
            }
        }

        [Fact]
        public async Task CustomerDoesNotExists_UpdateCustomerModelsOfInterest_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(x => x.ModelsOfInterestJson, (string)null)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/modelsOfInterest",
                    new UpdateCustomerModelsOfInterestRequest()
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
