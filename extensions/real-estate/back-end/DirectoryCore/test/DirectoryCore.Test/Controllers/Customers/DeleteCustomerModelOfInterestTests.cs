using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class DeleteCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public DeleteCustomerModelOfInterestTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerExist_DeleteCustomerModelOfInterest_ReturnNoContent()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var guid = Guid.NewGuid();
                var modelsOfInterest = new List<CustomerModelOfInterest>
                {
                    new CustomerModelOfInterest
                    {
                        Id = Guid.NewGuid(),
                        Color = "red",
                        ModelId = "abc",
                        Name = "abc"
                    },
                    new CustomerModelOfInterest
                    {
                        Id = Guid.NewGuid(),
                        Color = "black",
                        ModelId = "xyz",
                        Name = "xyz"
                    }
                };
                var modelsOfInterestJson = JsonSerializerExtensions.Serialize(modelsOfInterest);
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(c => c.ModelsOfInterestJson, modelsOfInterestJson)
                    .With(c => c.ModelsOfInterestETag, guid)
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.SaveChanges();

                var response = await client.DeleteAsync(
                    $"customers/{customer.Id}/modelsOfInterest/{modelsOfInterest.FirstOrDefault().Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                customer.ModelsOfInterestJson = JsonSerializerExtensions.Serialize(
                    new List<CustomerModelOfInterest>
                    {
                        new CustomerModelOfInterest
                        {
                            Id = modelsOfInterest[1].Id,
                            Color = "black",
                            ModelId = "xyz",
                            Name = "xyz"
                        }
                    }
                );
                dbContext = server.Assert().GetDirectoryDbContext();
                dbContext.Customers.Should().HaveCount(1);
                dbContext
                    .Customers.Select(c => c.ModelsOfInterestJson)
                    .Should()
                    .BeEquivalentTo(customer.ModelsOfInterestJson);
            }
        }
    }
}
