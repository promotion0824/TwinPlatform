using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Services;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class GetCustomersTests : BaseInMemoryTest
    {
        public GetCustomersTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomersExist_GetCustomers_ReturnCustomers()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customers = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        c => c.ModelsOfInterestJson,
                        JsonSerializerExtensions.Serialize(
                            new List<CustomerModelOfInterest>
                            {
                                new CustomerModelOfInterest
                                {
                                    Color = "red",
                                    ModelId = "abc",
                                    Name = "xyz"
                                },
                                new CustomerModelOfInterest
                                {
                                    Color = "black",
                                    ModelId = "123",
                                    Name = "abc"
                                }
                            }
                        )
                    )
                    .CreateMany(10)
                    .ToList();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.AddRange(customers);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"customers");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<CustomerDto>>();
                result
                    .Should()
                    .BeEquivalentTo(
                        CustomerDto.MapFrom(CustomerEntity.MapTo(customers), new ImagePathHelper())
                    );
            }
        }
    }
}
