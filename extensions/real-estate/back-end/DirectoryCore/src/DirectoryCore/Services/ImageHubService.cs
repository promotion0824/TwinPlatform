using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DirectoryCore.Services
{
    public interface IImageHubService
    {
        Task<Guid> CreateCustomerLogo(Guid customerId, byte[] logoFileContent);
    }

    public class ImageHubService : IImageHubService
    {
        private readonly HttpClient _client;
        private readonly IImagePathHelper _imagePathHelper;

        public ImageHubService(
            IHttpClientFactory httpClientFactory,
            IImagePathHelper imagePathHelper
        )
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.ImageHub);
            _imagePathHelper = imagePathHelper;
        }

        public async Task<Guid> CreateCustomerLogo(Guid customerId, byte[] logoFileContent)
        {
            var path = _imagePathHelper.GetCustomerLogoPath(customerId);
            return await CreateImage(path, logoFileContent);
        }

        private async Task<Guid> CreateImage(string path, byte[] imageContent)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageContent)
            {
                Headers = { ContentLength = imageContent.Length }
            };
            dataContent.Add(fileContent, "imageFile", "originalFileName");
            var response = await _client.PostAsync(path, dataContent);
            response.EnsureSuccessStatusCode(ApiServiceNames.ImageHub);
            var result = await response.Content.ReadAsAsync<OriginalImageDescriptor>();
            return result.ImageId;
        }

        public class OriginalImageDescriptor
        {
            public Guid ImageId { get; set; }
            public string FileName { get; set; }
            public string FileExtension { get; set; }
        }
    }
}
