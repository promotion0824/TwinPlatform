using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Services;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DirectoryCore.Test.Controllers.Customers
{
    public class UpdateCustomerLogoTests : BaseInMemoryTest
    {
        public UpdateCustomerLogoTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public async Task CustomerExists_UpdateCustomerLogo_ReturnsCustomerLogo()
        {
            var customer = Fixture
                .Build<CustomerEntity>()
                .With(x => x.LogoId, (Guid?)null)
                .Create();
            var logoImageBinary = Fixture.CreateMany<byte>(10).ToArray();
            var logoImageId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<DirectoryDbContext>();
                db.Customers.Add(customer);
                db.SaveChanges();
                arrangement
                    .GetImageHubApi()
                    .SetupRequest(HttpMethod.Post, $"{customer.Id}/logo")
                    .ReturnsJson(
                        new ImageHubService.OriginalImageDescriptor() { ImageId = logoImageId }
                    );

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoImageBinary)
                {
                    Headers = { ContentLength = logoImageBinary.Length }
                };
                dataContent.Add(fileContent, "logoImage", "abc.jpg");
                var response = await client.PutAsync($"customers/{customer.Id}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.Id.Should().Be(customer.Id);
                result.LogoId.HasValue.Should().BeTrue();
                result.LogoPath.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task CustomerDoesNotExists_UpdateCustomerLogo_ReturnsNotFound()
        {
            var customer = Fixture.Build<CustomerEntity>().Create();
            var logoImageBinary = Fixture.CreateMany<byte>(10).ToArray();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoImageBinary)
                {
                    Headers = { ContentLength = logoImageBinary.Length }
                };
                dataContent.Add(fileContent, "logoImage", "abc.jpg");
                var response = await client.PutAsync($"customers/{customer.Id}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
