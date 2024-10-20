using AutoFixture;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Willow.Platform.Models;
using Xunit.Abstractions;
using Xunit;
using Willow.Tests.Infrastructure;
using System.Net.Http;
using FluentAssertions;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Services.ContactUs;

namespace PlatformPortalXL.Test.Features.ContactUs
{
	public class GetCategoriesTests : BaseInMemoryTest
	{
		public GetCategoriesTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task AuthorizeToken_GetListOfCategory()
		{
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites,Guid.NewGuid()))
			{
                var response = await client.GetAsync($"contactus/categories");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<List<KeyValuePair<int,string>>>();
                var expectedResult = typeof(ContactUsCategory).ToEnumKeyValueDto();
                result.Should().BeEquivalentTo(expectedResult);

            }
		}
        [Fact]
		public async Task UserDoesNotHaveCorrectPermission_GetMaps_ReturnsForbidden()
		{
			 
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClient())
			{
                var response = await client.GetAsync($"contactus/categories");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
			}
		}
	}
}
