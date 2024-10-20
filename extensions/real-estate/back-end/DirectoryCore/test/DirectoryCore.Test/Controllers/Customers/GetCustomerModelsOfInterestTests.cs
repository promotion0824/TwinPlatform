using System;
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
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class GetCustomerModelsOfInterestTests : BaseInMemoryTest
    {
        public GetCustomerModelsOfInterestTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerExist_GetCustomerModelsOfInterest_ReturnCustomerModelsOfInterest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture
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
                                    Name = "abc"
                                },
                                new CustomerModelOfInterest
                                {
                                    Color = "black",
                                    ModelId = "xyz",
                                    Name = "xyz"
                                }
                            }
                        )
                    )
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.AddRange(customer);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"customers/{customer.Id}/modelsOfInterest");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<CustomerModelOfInterestDto>>();
                result
                    .Should()
                    .BeEquivalentTo(
                        CustomerModelOfInterestDto.MapFrom(
                            CustomerEntity.MapCustomerModelsOfInterest(
                                customer.ModelsOfInterestJson
                            )
                        )
                    );
            }
        }

        [Fact]
        public async Task CustomerNotExist_GetCustomer_ReturnNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync(
                    $"customers/{Guid.NewGuid()}/modelsOfInterest"
                );
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerExist_GetCustomerModelOfInterest_ReturnCustomerModelOfInterest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var modelsOfInterest = new List<CustomerModelOfInterest>
                {
                    new CustomerModelOfInterest
                    {
                        Id = Guid.NewGuid(),
                        Color = "red",
                        ModelId = "abc",
                        Name = "xyz"
                    },
                    new CustomerModelOfInterest
                    {
                        Id = Guid.NewGuid(),
                        Color = "black",
                        ModelId = "123",
                        Name = "abc"
                    }
                };
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        c => c.ModelsOfInterestJson,
                        JsonSerializerExtensions.Serialize(modelsOfInterest)
                    )
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.SaveChanges();

                var response = await client.GetAsync(
                    $"customers/{customer.Id}/modelsOfInterest/{modelsOfInterest.FirstOrDefault().Id}"
                );
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
                result
                    .Should()
                    .BeEquivalentTo(
                        CustomerModelOfInterestDto.MapFrom(
                            CustomerEntity
                                .MapCustomerModelsOfInterest(customer.ModelsOfInterestJson)
                                .FirstOrDefault(x => x.ModelId == "abc")
                        )
                    );
            }
        }
    }
}
