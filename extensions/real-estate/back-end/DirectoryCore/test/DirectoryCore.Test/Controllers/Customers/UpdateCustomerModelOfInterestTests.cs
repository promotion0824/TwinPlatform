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
    public class UpdateCustomerModelOfInterestTests : BaseInMemoryTest
    {
        public UpdateCustomerModelOfInterestTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task GivenValidInput_UpdateCustomerModelOfInterest_ReturnsUpdatedCustomerModelOfInterest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var modelsOfInterest = new List<CustomerModelOfInterest>()
                {
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "111",
                        Name = "name 1",
                        Color = "color 1",
                        Text = "text 1",
                        Icon = "icon 1"
                    },
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "222",
                        Name = "name 2",
                        Color = "color 2",
                        Text = "text 2",
                        Icon = "icon 3"
                    },
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "333",
                        Name = "name 3",
                        Color = "color 3",
                        Text = "text 3",
                        Icon = "icon 3"
                    }
                };
                var guid = Guid.NewGuid();
                var request = Fixture.Create<UpdateCustomerModelOfInterestRequest>();
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

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest/{modelsOfInterest.FirstOrDefault().Id}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerModelOfInterestDto>();
                result.Should().NotBeNull();
                result.Name.Should().Be(request.Name);
                result.Color.Should().Be(request.Color);
                result.Text.Should().Be(request.Text);
                result.Icon.Should().Be(result.Icon);
            }
        }

        [Fact]
        public async Task CustomerDoesNotExists_UpdateCustomerModelOfInterest_ReturnsNotFound()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var customer = Fixture
                    .Build<CustomerEntity>()
                    .With(x => x.ModelsOfInterestJson, (string)null)
                    .Create();
                var request = Fixture.Create<UpdateCustomerModelOfInterestRequest>();

                var dbContext = server.Arrange().CreateDbContext<DirectoryDbContext>();
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();

                var response = await client.PutAsJsonAsync(
                    $"customers/{Guid.NewGuid()}/modelsOfInterest/{Guid.NewGuid()}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CustomerModelOfInterestNotFound_UpdateCustomerModelOfInterest_ReturnsBadRequest()
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

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest/{Guid.NewGuid()}",
                    customerModelOfInterest
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task CustomerSameModelOfInterest_UpdateCustomerModelOfInterest_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var modelsOfInterest = new List<CustomerModelOfInterest>()
                {
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "111",
                        Name = "name 1",
                        Color = "color 1",
                        Text = "text 1",
                        Icon = "icon 1"
                    },
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "222",
                        Name = "name 2",
                        Color = "color 2",
                        Text = "text 2",
                        Icon = "icon 3"
                    },
                    new CustomerModelOfInterest()
                    {
                        Id = Guid.NewGuid(),
                        ModelId = "333",
                        Name = "name 3",
                        Color = "color 3",
                        Text = "text 3",
                        Icon = "icon 3"
                    }
                };
                var request = modelsOfInterest[0];
                request.ModelId = "222";
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

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}/modelsOfInterest/{request.Id}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
