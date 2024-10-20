using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Dto.Requests;
using DirectoryCore.Entities;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class UpdateCustomerTests : BaseInMemoryTest
    {
        public UpdateCustomerTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerExists_UpdateCustomer_ReturnsCustomer()
        {
            var sigmaConnectionId = Fixture.Create<string>();

            var customer = Fixture.Create<CustomerEntity>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();

                var db = arrangement.CreateDbContext<DirectoryDbContext>();
                db.Customers.Add(customer);
                db.SaveChanges();

                var request = new UpdateCustomerRequest { SigmaConnectionId = sigmaConnectionId };

                var response = await client.PutAsJsonAsync($"customers/{customer.Id}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.Id.Should().Be(customer.Id);
                result.SigmaConnectionId.Should().Be(sigmaConnectionId);
            }
        }

        [Fact]
        public async Task CustomerDoesNotExists_UpdateCustomer_ReturnsNotFound()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var request = new UpdateCustomerRequest
                {
                    SigmaConnectionId = Fixture.Create<string>()
                };

                var response = await client.PutAsJsonAsync(
                    $"customers/{customer.Id}",
                    new UpdateCustomerRequest() { }
                );

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task SigmaConnectionIdInUse_UpdateCustomer_ThrowBadRequest()
        {
            var sigmaConnectionId = Fixture.Create<string>();

            var customers = Fixture
                .Build<CustomerEntity>()
                .With(x => x.SigmaConnectionId, sigmaConnectionId)
                .CreateMany(2);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();

                var db = arrangement.CreateDbContext<DirectoryDbContext>();
                db.Customers.AddRange(customers);
                db.SaveChanges();

                var request = new UpdateCustomerRequest
                {
                    SigmaConnectionId = customers.ElementAt(1).SigmaConnectionId
                };

                var response = await client.PutAsJsonAsync(
                    $"customers/{customers.ElementAt(0).Id}",
                    request
                );

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }
    }
}
