using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

using ImageHub.Models;

namespace ImageHub.ComponentTests
{
    /// <summary>
    /// NOTE: YOU MUST RUN THE STORAGE EMULATOR FOR THESE TESTS TO WORK!!
    /// </summary>
    public class ImageControllerTests : IClassFixture<WebApplicationFactory<ImageHub.Startup>>
    {
        private readonly HttpClient _client;

        public ImageControllerTests(WebApplicationFactory<ImageHub.Startup> fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task ImageHub_Get_notfound()
        {
            var rootid   = Guid.NewGuid();
            var response = await _client.GetAsync($"/{rootid}/sites/{Guid.NewGuid()}/logo/{Guid.NewGuid()}_CenterCrop_w400_h300.jpg");
            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("abc", null, null, null, null)]
        [InlineData("abc", "DEF", null, null, null)]
        [InlineData("abc", "DEF", "Opq", null, null)]
        [InlineData("abc", "DEF", "Opq", "rST", null)]
        [InlineData("abc", "DEF", "Opq", "rST", "xyz")]
        public async Task ImageHub_Create_success(string segment0,
                                                  string segment1,
                                                  string segment2,
                                                  string segment3,
                                                  string segment4)
        {
            var rootid   = Guid.NewGuid();
            var pathSegments = new List<string> { segment0, segment1, segment2, segment3, segment4 };
            var urlPath = string.Join('/', pathSegments.Where(x => x != null));
            var response = await _client.PostAsync($"/{rootid}/{urlPath}", GetTestImage("bob"));
            var str = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ImageDescriptor>(str);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var getResponse = await _client.GetAsync($"/{rootid}/{urlPath}/{result.ImageId}_CenterCrop_w400_h300.jpg");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var deleteResponse = await _client.DeleteAsync($"/{rootid}/{urlPath}/{result.ImageId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var getResponse2 = await _client.GetAsync($"/{rootid}/{urlPath}/{result.ImageId}_CenterCrop_w400_h300.jpg");

            Assert.Equal(HttpStatusCode.NotFound, getResponse2.StatusCode);
        }

        #region Private

        private HttpContent GetTestImage(string fileName)
        {
            var testImageBytes = GetTestImageBytes();

            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(testImageBytes)
            {
                Headers = { ContentLength = testImageBytes.Length }
            };
            dataContent.Add(fileContent, "imageFile", $"{fileName}.jpg");

            return dataContent;
        }

        private byte[] GetTestImageBytes()
        {
            var image = new Image<Rgba32>(10, 20);

            using (var stream = new MemoryStream())
            {
                image.SaveAsJpeg(stream);
                return stream.ToArray();
            }
        }

        #endregion
    }
}
