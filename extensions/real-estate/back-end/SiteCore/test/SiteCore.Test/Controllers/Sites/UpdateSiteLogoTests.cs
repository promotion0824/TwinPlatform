using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using SiteCore.Dto;
using SiteCore.Entities;
using System.Linq;
using System;
using SiteCore.Services.ImageHub;
using SiteCore.Tests;

namespace SiteCore.Test.Controllers.Sites
{
    public class UpdateSiteLogoTests : BaseInMemoryTest
    {
        public UpdateSiteLogoTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SiteExists_UpdateSiteLogo_ReturnsSiteLogo()
        {
            var site = Fixture.Build<SiteEntity>()
                              .Without(x => x.Floors)
                              .Without(x => x.PortfolioId)
                              .With(x => x.Postcode, "111250")
                              .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                              .With(x => x.LogoId, (Guid?)null)
                              .Create();
            var logoImageBinary = Fixture.CreateMany<byte>(10).ToArray();
            var logoImageId = Guid.NewGuid();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<SiteDbContext>();
                db.Sites.Add(site);
                db.SaveChanges();
                arrangement.GetImageHubApi()
                    .SetupRequest(HttpMethod.Post, $"{site.CustomerId}/sites/{site.Id}/logo")
                    .ReturnsJson(new OriginalImageDescriptor() { ImageId = logoImageId });

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(logoImageBinary)
                {
                    Headers = { ContentLength = logoImageBinary.Length }
                };
                dataContent.Add(fileContent, "logoImage", "abc.jpg");
                var response = await client.PutAsync($"sites/{site.Id}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteDetailDto>();
                result.Id.Should().Be(site.Id);
                result.LogoId.HasValue.Should().BeTrue();
                result.LogoPath.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task SiteDoesNotExists_UpdateSiteLogo_ReturnsNotFound()
        {
            var site = Fixture.Build<SiteEntity>()
                              .Without(x => x.Floors)
                              .Without(x => x.PortfolioId)
                              .With(x => x.Postcode, "111250")
                              .With(x => x.TimezoneId, "AUS Eastern Standard Time")
                              .Create();
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
                var response = await client.PutAsync($"sites/{site.Id}/logo", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}