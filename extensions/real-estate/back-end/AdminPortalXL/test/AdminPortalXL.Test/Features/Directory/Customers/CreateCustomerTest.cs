using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using AdminPortalXL.Dto;
using AdminPortalXL.Models.Directory;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System;
using System.Collections.Generic;

namespace AdminPortalXL.Test.Features.Directory.Customers
{
    public class CreateCustomerTest : BaseInMemoryTest
    {
        public CreateCustomerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenNotExistRegionId_CreateCustomer_ReturnNotFound()
        {
            var nonExistingRegionId = "test-regionId";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegions(ServerFixtureConfigurations.Default, new Guid[0], new Guid[0]);

                var dataContent = new MultipartFormDataContent();
                dataContent.Add(new StringContent(nonExistingRegionId), "RegionId");
                dataContent.Add(new StringContent("gg"), "CustomerName");
                dataContent.Add(new StringContent("mars"), "Country");
                var response = await client.PostAsync($"customers", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenValidInputWithoutLogo_CreateCustomer_ReturnNewCustomer()
        {
            var regionId1= ServerFixtureConfigurations.Default.RegionIds[1];
            var customer = Fixture.Create<Customer>();
            var expectedCustomer = CustomerDto.Map(customer);
            expectedCustomer.RegionId = regionId1;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegions(ServerFixtureConfigurations.Default, new Guid[0], new Guid[0]);
                server.Arrange().GetRegionalDirectoryApi(regionId1)
                                   .SetupRequest(HttpMethod.Post, $"customers")
                                   .ReturnsJson(customer);

                var dataContent = new MultipartFormDataContent();
                dataContent.Add(new StringContent(regionId1), "RegionId");
                dataContent.Add(new StringContent(customer.Name), "CustomerName");
                dataContent.Add(new StringContent(customer.Country), "Country");
                dataContent.Add(new StringContent(customer.SigmaConnectionId.ToString()), "SigmaConnectionId");
                var response = await client.PostAsync($"customers", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.Should().BeEquivalentTo(expectedCustomer);
            }
        }

        [Fact]
        public async Task GivenLogoImage_CreateCustomer_ReturnNewCustomerWithValidLogoId()
        {
            var regionId1= ServerFixtureConfigurations.Default.RegionIds[1];
            var customerAfterCreation = Fixture.Build<Customer>()
                                               .With(x => x.LogoId, (Guid?)null)
                                               .Create();
            var customerAfterUpdateLogo = Fixture.Build<Customer>()
                                                 .With(x => x.Id, customerAfterCreation.Id)
                                                 .With(x => x.LogoId, Guid.NewGuid())
                                                 .Create();
            var expectedCustomer = CustomerDto.Map(customerAfterUpdateLogo);
            expectedCustomer.RegionId = regionId1;
            var logoImageContent = Fixture.CreateMany<byte>(10).ToArray();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegions(ServerFixtureConfigurations.Default, new Guid[0], new Guid[0]);
                server.Arrange().GetRegionalDirectoryApi(regionId1)
                                   .SetupRequest(HttpMethod.Post, $"customers")
                                   .ReturnsJson(customerAfterCreation);
                server.Arrange().GetRegionalDirectoryApi(regionId1)
                                   .SetupRequest(HttpMethod.Put, $"customers/{customerAfterCreation.Id}/logo")
                                   .ReturnsJson(customerAfterUpdateLogo);

                var dataContent = new MultipartFormDataContent();
                dataContent.Add(new StringContent(regionId1), "RegionId");
                dataContent.Add(new StringContent(customerAfterCreation.Name), "CustomerName");
                dataContent.Add(new StringContent(customerAfterCreation.Country), "Country");
                dataContent.Add(
                    new ByteArrayContent(logoImageContent) { Headers = { ContentLength = logoImageContent.Length } },
                    "logoImage",
                    "abc.jpg");
                var response = await client.PostAsync($"customers", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.LogoId.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GivenSigmaConnectionId_UpdateCustomer_ReturnCustomerWithNewSigmaConnectionId()
        {
            var customer = Fixture.Create<Customer>();
            var expectedCustomer = CustomerDto.Map(customer);

            var regionId1 = ServerFixtureConfigurations.Default.RegionIds[1];
            expectedCustomer.RegionId = regionId1;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithSupervisorRole())
            {
                server.Arrange().SetCustomerRegions(ServerFixtureConfigurations.Default, new Guid[0], new Guid[0]);
                server.Arrange().GetRegionalDirectoryApi(regionId1)
                                   .SetupRequest(HttpMethod.Put, $"customers/{customer.Id}")
                                   .ReturnsJson(customer);

                var dataContent = new MultipartFormDataContent();
                dataContent.Add(new StringContent(regionId1), "RegionId");
                dataContent.Add(new StringContent(customer.SigmaConnectionId.ToString()), "SigmaConnectionId");

                var response = await client.PutAsync($"customers/{customer.Id}", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<CustomerDto>();
                result.SigmaConnectionId.Should().NotBeNull();
            }
        }
    }
}
