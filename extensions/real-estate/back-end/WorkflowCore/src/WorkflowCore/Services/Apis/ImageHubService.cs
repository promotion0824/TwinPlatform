using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorkflowCore.Services.Apis
{
    public interface IImageHubService
    {
        Task<OriginalImageDescriptor> CreateAttachment(string path, string originalFileName, byte[] attachmentFileContent);
        Task DeleteAttachment(string path, Guid attachmentId);
    }

    public class ImageHubService : IImageHubService
    {
        private readonly HttpClient _client;

        public ImageHubService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.ImageHub);
        }

        public async Task<OriginalImageDescriptor> CreateAttachment(string path, string originalFileName, byte[] attachmentFileContent)
        {
            return await CreateImage(path, attachmentFileContent, originalFileName);
        }

        public async Task DeleteAttachment(string path, Guid attachmentId)
        {
            await DeleteImage(path, attachmentId);
        }

        private async Task DeleteImage(string path, Guid imageId)
        {
            var response = await _client.DeleteAsync($"{path}/{imageId}");
            response.EnsureSuccessStatusCode(ApiServiceNames.ImageHub);
        }

        private async Task<OriginalImageDescriptor> CreateImage(string path, byte[] imageContent, string originalFileName)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageContent)
            {
                Headers = { ContentLength = imageContent.Length }
            };
            dataContent.Add(fileContent, "imageFile", originalFileName);
            var response = await _client.PostAsync(path, dataContent);
            response.EnsureSuccessStatusCode(ApiServiceNames.ImageHub);
            var result = await response.Content.ReadAsAsync<OriginalImageDescriptor>();
            return result;
        }
    }

    public class OriginalImageDescriptor
    {
        public Guid ImageId { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
    }

}
