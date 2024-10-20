using System;
using System.Collections.Generic;
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
    public class GetCustomerTests : BaseInMemoryTest
    {
        public GetCustomerTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerNotExist_GetCustomer_ReturnOk()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"customers/{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerExist_GetCustomer_ReturnCustomer()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(
                        c => c.FeaturesJson,
                        JsonSerializerExtensions.Serialize(
                            new CustomerFeatures
                            {
                                IsConnectivityViewEnabled = true,
                                IsRulingEngineEnabled = true,
                                IsDynamicsIntegrationEnabled = true,
                                IsSmartPolesEnabled = true
                            }
                        )
                    )
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
                    .Create();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                dbContext.SaveChanges();

                var response = await client.GetAsync($"customers/{customer.Id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result
                    .Should()
                    .BeEquivalentTo(
                        CustomerDto.MapFrom(CustomerEntity.MapTo(customer), new ImagePathHelper())
                    );
            }
        }
    }
}
