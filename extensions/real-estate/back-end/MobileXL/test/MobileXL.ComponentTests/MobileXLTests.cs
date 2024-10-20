using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MobileXL.ComponentTests
{
    public class MobileXLTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public MobileXLTests(WebApplicationFactory<Startup> fixture)
        {
            _client = fixture.CreateClient();
        }
        
        [Fact(Skip = "Failed validation in AssetCore")]
        public async Task MobileXL_Get_notfound()
        {
            var response = await _client.GetAsync($"/sites/{Guid.NewGuid()}/assets/{Guid.NewGuid()}/files/{Guid.NewGuid()}");
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(Skip = "Failed validation in AssetCore")]
        public async Task MobileXL_Get_success()
        {
            var siteId   = Guid.Parse("4e5fc229-ffd9-462a-882b-16b4a63b2a8a");
            var assetId  = Guid.Parse("00600000-0000-0000-0000-000000916159");
            var fileId   = Guid.Parse("27ddb730-573a-444d-aae8-d71a7dd77607");
            var response = await _client.GetAsync($"/sites/{siteId}/assets/{assetId}/files/{fileId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var stream = await response.Content.ReadAsStreamAsync();

            Assert.NotNull(stream);
            Assert.NotEqual(0, stream.Length);
        }
    }
}
